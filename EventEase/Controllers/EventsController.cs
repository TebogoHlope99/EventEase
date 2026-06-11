using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobStorageService _blobService;
        private readonly ILogger<EventsController> _logger;

        public EventsController(ApplicationDbContext context, IBlobStorageService blobService, ILogger<EventsController> logger)
        {
            _context = context;
            _blobService = blobService;
            _logger = logger;
        }

        // GET: Events with Advanced Filtering
        public async Task<IActionResult> Index(
            string searchString,
            string sortOrder,
            DateTime? startDate,
            DateTime? endDate,
            int? venueId,
            int? eventTypeId,
            string status,
            int? minCapacity,
            int? maxCapacity,
            bool? showAvailableVenuesOnly,
            int? pageNumber)
        {
            // Store filter values for the view
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentStartDate"] = startDate;
            ViewData["CurrentEndDate"] = endDate;
            ViewData["CurrentVenueId"] = venueId;
            ViewData["CurrentEventTypeId"] = eventTypeId;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentMinCapacity"] = minCapacity;
            ViewData["CurrentMaxCapacity"] = maxCapacity;
            ViewData["ShowAvailableVenuesOnly"] = showAvailableVenuesOnly;

            // Sort parameters
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["VenueSortParm"] = sortOrder == "venue" ? "venue_desc" : "venue";
            ViewData["CapacitySortParm"] = sortOrder == "capacity" ? "capacity_desc" : "capacity";
            ViewData["TypeSortParm"] = sortOrder == "type" ? "type_desc" : "type";

            // Get venues for filter dropdown
            var venuesList = await _context.Venue.ToListAsync();
            var eventTypesList = await _context.EventTypes.Where(et => et.IsActive).ToListAsync();

            ViewBag.Venues = new SelectList(venuesList, "VenueId", "VenueName");
            ViewBag.EventTypes = new SelectList(eventTypesList, "EventTypeId", "TypeName");

            ViewBag.VenuesList = venuesList;
            ViewBag.EventTypesList = eventTypesList;

            ViewBag.VenuesCount = venuesList.Count;
            ViewBag.EventTypesCount = eventTypesList.Count;

            // Get events with venue information
            var events = from e in _context.Event
                        .Include(e => e.Venue)
                        .Include(e => e.EventType)
                         select e;

            // APPLY SEARCH FILTER
            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(e => e.EventName.Contains(searchString)
                                    || (e.Description != null && e.Description.Contains(searchString))
                                    || (e.Venue != null && e.Venue.VenueName.Contains(searchString))
                                    || (e.EventType != null && e.EventType.TypeName.Contains(searchString)));
            }

            // APPLY DATE RANGE FILTER
            if (startDate.HasValue)
            {
                events = events.Where(e => e.EventDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                events = events.Where(e => e.EventDate.Date <= endDate.Value.Date);
            }

            // APPLY VENUE FILTER
            if (venueId.HasValue && venueId > 0)
            {
                events = events.Where(e => e.VenueId == venueId);
            }

            // Apply event type filter
            if (eventTypeId.HasValue && eventTypeId > 0)
            {
                events = events.Where(e => e.EventTypeId == eventTypeId);
            }

            // APPLY STATUS FILTER (Upcoming/Past/All)
            if (!string.IsNullOrEmpty(status))
            {
                var today = DateTime.Now.Date;
                if (status == "upcoming")
                {
                    events = events.Where(e => e.EventDate.Date >= today);
                }
                else if (status == "past")
                {
                    events = events.Where(e => e.EventDate.Date < today);
                }
            }

            // APPLY CAPACITY RANGE FILTER (via Venue)
            if (minCapacity.HasValue && minCapacity > 0)
            {
                events = events.Where(e => e.Venue != null && e.Venue.Capacity >= minCapacity);
            }

            if (maxCapacity.HasValue && maxCapacity > 0)
            {
                events = events.Where(e => e.Venue != null && e.Venue.Capacity <= maxCapacity);
            }

            // Apply venue availability filter (show only events at available venues)
            if (showAvailableVenuesOnly == true)
            {
                events = events.Where(e => e.Venue != null && e.Venue.IsAvailable);
            }

            // APPLY SORTING
            events = sortOrder switch
            {
                "name_desc" => events.OrderByDescending(e => e.EventName),
                "date" => events.OrderBy(e => e.EventDate),
                "date_desc" => events.OrderByDescending(e => e.EventDate),
                "venue" => events.OrderBy(e => e.Venue.VenueName),
                "venue_desc" => events.OrderByDescending(e => e.Venue.VenueName),
                "capacity" => events.OrderBy(e => e.Venue.Capacity),
                "capacity_desc" => events.OrderByDescending(e => e.Venue.Capacity),
                "type" => events.OrderBy(e => e.EventType.TypeName),
                "type_desc" => events.OrderByDescending(e => e.EventType.TypeName),
                _ => events.OrderBy(e => e.EventName)
            };

            // Pagination
            int pageSize = 10;
            var paginatedEvents = await PaginatedList<Event>.CreateAsync(events.AsNoTracking(), pageNumber ?? 1, pageSize);

            // Get counts for stats with applied filters
            ViewBag.TotalCount = await events.CountAsync();
            ViewBag.UpcomingCount = await events.CountAsync(e => e.EventDate.Date >= DateTime.Now.Date);
            ViewBag.PastCount = await events.CountAsync(e => e.EventDate.Date < DateTime.Now.Date);
            ViewBag.EventTypeStats = await events
                .Where(e => e.EventType != null)
                .GroupBy(e => new { e.EventType.EventTypeId, e.EventType.TypeName })
                .Select(g => new { g.Key.EventTypeId, g.Key.TypeName, Count = g.Count() })
                .ToListAsync();

            return View(paginatedEvents);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewBag.Venues = new SelectList(_context.Venue.Where(v => v.IsAvailable), "VenueId", "VenueName");
            ViewBag.EventTypes = new SelectList(_context.EventTypes.Where(et => et.IsActive), "EventTypeId", "TypeName");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event @event, IFormFile? imageFile)
        {
            try
            {
                _logger.LogInformation("Starting event creation");
                ModelState.Remove("Venue");
                ModelState.Remove("Bookings");
                ModelState.Remove("EventType");

                if (ModelState.IsValid)
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        _logger.LogInformation($"Uploading event image: {imageFile.FileName}, Size: {imageFile.Length} bytes");
                        var imageUrl = await _blobService.UploadImageAsync(imageFile, "event-images");
                        @event.ImageUrl = imageUrl;
                    }
                    else
                    {
                        @event.ImageUrl = _blobService.GetDefaultImageUrl("event");
                    }

                    @event.CreatedAt = DateTime.Now;
                    _context.Event.Add(@event);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Event '{@event.EventName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event");
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", @event.VenueId);
            ViewBag.EventTypes = new SelectList(_context.EventTypes, "EventTypeId", "TypeName", @event.EventTypeId);
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.EventType)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null) return NotFound();

            // Get ALL active event types for the dropdown, not just filtered ones
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", @event.VenueId);
            ViewBag.EventTypes = new SelectList(_context.EventTypes.Where(et => et.IsActive), "EventTypeId", "TypeName", @event.EventTypeId);

            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event @event, IFormFile? imageFile, bool deleteImage = false)
        {
            if (id != @event.EventId) return NotFound();

            ModelState.Remove("Venue");
            ModelState.Remove("Bookings");
            ModelState.Remove("EventType");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Event.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
                    if (existingEvent == null) return NotFound();

                    if (deleteImage && !string.IsNullOrEmpty(existingEvent.ImageUrl))
                    {
                        await _blobService.DeleteImageAsync(existingEvent.ImageUrl, "event-images");
                        @event.ImageUrl = _blobService.GetDefaultImageUrl("event");
                    }
                    else if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(existingEvent.ImageUrl) && !existingEvent.ImageUrl.Contains("placehold.co"))
                        {
                            await _blobService.DeleteImageAsync(existingEvent.ImageUrl, "event-images");
                        }
                        @event.ImageUrl = await _blobService.UploadImageAsync(imageFile, "event-images");
                    }
                    else
                    {
                        @event.ImageUrl = existingEvent.ImageUrl;
                    }

                    @event.CreatedAt = existingEvent.CreatedAt;
                    _context.Event.Update(@event);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Event '{@event.EventName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating event");
                    ModelState.AddModelError("", "An error occurred while updating.");
                }
            }
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", @event.VenueId);
            ViewBag.EventTypes = new SelectList(_context.EventTypes.Where(et => et.IsActive), "EventTypeId", "TypeName", @event.EventTypeId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var @event = await _context.Event
                .Include(e => e.Venue)
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();

            ViewBag.HasBookings = @event.Bookings != null && @event.Bookings.Any();
            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Event
                .Include(e => e.Bookings)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (@event == null) return NotFound();

            if (@event.Bookings != null && @event.Bookings.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete event '{@event.EventName}' because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (!string.IsNullOrEmpty(@event.ImageUrl) && !@event.ImageUrl.Contains("placehold.co"))
            {
                await _blobService.DeleteImageAsync(@event.ImageUrl, "event-images");
            }

            _context.Event.Remove(@event);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Event '{@event.EventName}' deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetEventTypeStats()
        {
            var stats = await _context.Event
                .GroupBy(e => e.EventType.TypeName)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToListAsync();

            return Json(stats);
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventId == id);
        }

        // AJAX endpoint for quick filter counts
        [HttpGet]
        public async Task<IActionResult> GetFilterCounts()
        {
            var total = await _context.Event.CountAsync();
            var upcoming = await _context.Event.CountAsync(e => e.EventDate.Date >= DateTime.Now.Date);
            var past = await _context.Event.CountAsync(e => e.EventDate.Date < DateTime.Now.Date);

            return Json(new { total, upcoming, past });
        }
    }
}