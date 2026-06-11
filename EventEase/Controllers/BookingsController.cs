using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingsController> _logger;

        public BookingsController(ApplicationDbContext context, ILogger<BookingsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Bookings with Advanced Filtering (SINGLE Index method)
        public async Task<IActionResult> Index(
            string searchTerm,
            string statusFilter,
            DateTime? startDate,
            DateTime? endDate,
            int? venueId,
            int? eventId,
            int? eventTypeId,
            bool? showAvailableVenuesOnly,
            string sortOrder,
            int? pageNumber)
        {
            // Store filter values for the view
            ViewData["CurrentSearchTerm"] = searchTerm;
            ViewData["CurrentStatusFilter"] = statusFilter;
            ViewData["CurrentStartDate"] = startDate;
            ViewData["CurrentEndDate"] = endDate;
            ViewData["CurrentVenueId"] = venueId;
            ViewData["CurrentEventId"] = eventId;
            ViewData["CurrentEventTypeId"] = eventTypeId;
            ViewData["ShowAvailableVenuesOnly"] = showAvailableVenuesOnly;
            ViewData["CurrentSort"] = sortOrder;

            // Get dropdown data
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName");
            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName");
            ViewBag.EventTypes = new SelectList(_context.EventTypes.Where(et => et.IsActive), "EventTypeId", "TypeName");

            // Get lists for active filters display
            ViewBag.VenuesList = await _context.Venue.ToListAsync();
            ViewBag.EventsList = await _context.Event.ToListAsync();
            ViewBag.EventTypesList = await _context.EventTypes.ToListAsync();

            // Get booking counts for stats
            ViewBag.TotalBookings = await _context.Booking.CountAsync();
            ViewBag.UpcomingBookings = await _context.Booking.CountAsync(b => b.BookingDate >= DateTime.Now.Date);
            ViewBag.PastBookings = await _context.Booking.CountAsync(b => b.BookingDate < DateTime.Now.Date);
            ViewBag.UniqueVenuesBooked = await _context.Booking.Select(b => b.VenueId).Distinct().CountAsync();

            // Get event type stats for filter chips
            ViewBag.EventTypeStats = await _context.Booking
                .Where(b => b.Event != null && b.Event.EventType != null)
                .GroupBy(b => new { b.Event.EventType.EventTypeId, b.Event.EventType.TypeName })
                .Select(g => new { g.Key.EventTypeId, g.Key.TypeName, Count = g.Count() })
                .ToListAsync();

            // Get bookings with related data
            var bookings = from b in _context.Booking
                          .Include(b => b.Event)
                              .ThenInclude(e => e.EventType)
                          .Include(b => b.Venue)
                           select b;

            // Apply search filter (by BookingID or EventName)
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (int.TryParse(searchTerm, out int bookingId))
                {
                    bookings = bookings.Where(b => b.BookingId == bookingId);
                }
                else
                {
                    bookings = bookings.Where(b => b.Event != null &&
                         (b.Event.EventName.Contains(searchTerm) ||
                         (b.Event.EventType != null && b.Event.EventType.TypeName.Contains(searchTerm)) ||
                         (b.Venue != null && b.Venue.VenueName.Contains(searchTerm))));
                }
            }

            // Apply date range filter
            if (startDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate.Date >= startDate.Value.Date);
            }
            if (endDate.HasValue)
            {
                bookings = bookings.Where(b => b.BookingDate.Date <= endDate.Value.Date);
            }

            // Apply venue filter
            if (venueId.HasValue && venueId > 0)
            {
                bookings = bookings.Where(b => b.VenueId == venueId);
            }

            // Apply event filter
            if (eventId.HasValue && eventId > 0)
            {
                bookings = bookings.Where(b => b.EventId == eventId);
            }

            // Apply event type filter
            if (eventTypeId.HasValue && eventTypeId > 0)
            {
                bookings = bookings.Where(b => b.Event != null && b.Event.EventTypeId == eventTypeId);
            }

            // Apply venue availability filter
            if (showAvailableVenuesOnly.HasValue)
            {
                if (showAvailableVenuesOnly == true)
                {
                    bookings = bookings.Where(b => b.Venue != null && b.Venue.IsAvailable);
                }
                else if (showAvailableVenuesOnly.Value == false)
                {
                    bookings = bookings.Where(b => b.Venue != null && !b.Venue.IsAvailable);
                }
            }
            // Apply status filter
            if (!string.IsNullOrEmpty(statusFilter))
            {
                var today = DateTime.Now.Date;
                if (statusFilter == "upcoming")
                {
                    bookings = bookings.Where(b => b.BookingDate >= today);
                }
                else if (statusFilter == "past")
                {
                    bookings = bookings.Where(b => b.BookingDate < today);
                }
            }

            // Apply sorting
            bookings = sortOrder switch
            {
                "bookingDate_desc" => bookings.OrderByDescending(b => b.BookingDate),
                "bookingDate_asc" => bookings.OrderBy(b => b.BookingDate),
                "eventName" => bookings.OrderBy(b => b.Event.EventName),
                "eventName_desc" => bookings.OrderByDescending(b => b.Event.EventName),
                "venueName" => bookings.OrderBy(b => b.Venue.VenueName),
                "venueName_desc" => bookings.OrderByDescending(b => b.Venue.VenueName),
                "eventType" => bookings.OrderBy(b => b.Event.EventType.TypeName),
                "eventType_desc" => bookings.OrderByDescending(b => b.Event.EventType.TypeName),
                _ => bookings.OrderByDescending(b => b.BookingDate)
            };

            // Pagination
            int pageSize = 10;
            var result = await PaginatedList<Booking>.CreateAsync(bookings.AsNoTracking(), pageNumber ?? 1, pageSize);

            return View(result);
        }

        // GET: Bookings/Consolidated - Dashboard View
        public async Task<IActionResult> Consolidated()
        {
            var bookings = await (from b in _context.Booking
                                  join e in _context.Event on b.EventId equals e.EventId
                                  join v in _context.Venue on b.VenueId equals v.VenueId
                                  select new BookingViewModel
                                  {
                                      BookingId = b.BookingId,
                                      EventName = e.EventName,
                                      EventDate = e.EventDate,
                                      VenueName = v.VenueName,
                                      Location = v.Location,
                                      Capacity = v.Capacity,
                                      BookingDate = b.BookingDate,
                                      EventDescription = e.Description,
                                      VenueImageUrl = v.ImageUrl,
                                      EventImageUrl = e.ImageUrl
                                  }).ToListAsync();

            return View(bookings);
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName");
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,VenueId,BookingDate")] Booking booking)
        {
            ModelState.Remove("Event");
            ModelState.Remove("Venue");

            if (ModelState.IsValid)
            {
                var isDoubleBooking = await _context.Booking.AnyAsync(b =>
                    b.VenueId == booking.VenueId &&
                    b.BookingDate.Date == booking.BookingDate.Date);

                if (isDoubleBooking)
                {
                    ModelState.AddModelError("", "This venue is already booked for the selected date. Please choose another date or venue.");
                }

                var eventData = await _context.Event.FindAsync(booking.EventId);
                if (eventData != null && eventData.EventDate.Date > booking.BookingDate.Date)
                {
                    ModelState.AddModelError("", "Booking date cannot be before the event date.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    booking.CreatedAt = DateTime.Now;
                    _context.Booking.Add(booking);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Booking created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UQ_Booking_Venue_Date") == true)
                {
                    ModelState.AddModelError("", "A booking for this venue on this date already exists.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating booking");
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again.");
                }
            }

            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null) return NotFound();

            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate,CreatedAt")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();

            ModelState.Remove("Event");
            ModelState.Remove("Venue");

            bool isDoubleBooking = await _context.Booking
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.BookingDate.Date == booking.BookingDate.Date &&
                               b.BookingId != booking.BookingId);

            if (isDoubleBooking)
            {
                ModelState.AddModelError("", "This venue is already booked for the selected date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Booking.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating booking");
                    ModelState.AddModelError("", "Unable to update booking. Please try again.");
                }
            }

            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var booking = await _context.Booking.FindAsync(id);
                if (booking != null)
                {
                    _context.Booking.Remove(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking deleted successfully!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting booking");
                TempData["ErrorMessage"] = "Unable to delete booking.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Bookings/CheckAvailability - AJAX endpoint
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int venueId, DateTime date)
        {
            var isBooked = await _context.Booking.AnyAsync(b =>
                b.VenueId == venueId &&
                b.BookingDate.Date == date.Date);

            return Json(new { isAvailable = !isBooked });
        }

        // GET: Bookings/GetStats - AJAX endpoint for stats
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var total = await _context.Booking.CountAsync();
            var upcoming = await _context.Booking.CountAsync(b => b.BookingDate >= DateTime.Now.Date);
            var past = await _context.Booking.CountAsync(b => b.BookingDate < DateTime.Now.Date);
            var uniqueVenues = await _context.Booking.Select(b => b.VenueId).Distinct().CountAsync();

            return Json(new { total, upcoming, past, uniqueVenues });
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }
    }
}