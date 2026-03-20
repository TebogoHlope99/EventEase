using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            try
            {
                var bookings = await _context.Booking
                    .Include(b => b.Event)
                    .Include(b => b.Venue)
                    .ToListAsync();
                return View(bookings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Bookings Index: {ex.Message}");
                TempData["ErrorMessage"] = "Unable to load bookings. Please check database connection.";
                return View(new List<Booking>());
            }
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

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
            // Check for double booking
            bool isDoubleBooking = await _context.Booking
                .AnyAsync(b => b.VenueId == booking.VenueId &&
                               b.BookingDate.Date == booking.BookingDate.Date);

            if (isDoubleBooking)
            {
                ModelState.AddModelError("", "This venue is already booked for the selected date.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    booking.CreatedAt = DateTime.Now;
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating booking: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save booking. Please try again.");
                }
            }
            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewBag.Events = new SelectList(_context.Event, "EventId", "EventName", booking.EventId);
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", booking.VenueId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate,CreatedAt")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            // Check for double booking (excluding current booking)
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
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Booking updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating booking: {ex.Message}");
                    ModelState.AddModelError("", "Unable to update booking. Please try again.");
                }
            }
            ViewBag.Events = _context.Event.ToList();
            ViewBag.Venues = _context.Venue.ToList();
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

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
                Console.WriteLine($"Error deleting booking: {ex.Message}");
                TempData["ErrorMessage"] = "Unable to delete booking.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingId == id);
        }
    }
}