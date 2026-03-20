using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventEase.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
            try
            {
                var events = await _context.Event
                    .Include(e => e.Venue)
                    .ToListAsync();
                return View(events);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Events Index: {ex.Message}");
                TempData["ErrorMessage"] = "Unable to load events. Please check database connection.";
                return View(new List<Event>());
            }
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName");
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventName,EventDate,Description,VenueId,ImageUrl")] Event @event)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    @event.CreatedAt = DateTime.Now;
                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating event: {ex.Message}");
                    ModelState.AddModelError("", "Unable to save event. Please try again.");
                }
            }
            ViewBag.Venues = _context.Venue.ToList();
            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewBag.Venues = new SelectList(_context.Venue, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,EventName,EventDate,Description,VenueId,ImageUrl,CreatedAt")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
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
                    Console.WriteLine($"Error updating event: {ex.Message}");
                    ModelState.AddModelError("", "Unable to update event. Please try again.");
                }
            }
            ViewBag.Venues = _context.Venue.ToList();
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .Include(e => e.Venue)  
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            // Check if event has any bookings
            var hasBookings = await _context.Booking.AnyAsync(b => b.EventId == id);
            ViewBag.HasBookings = hasBookings;

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var @event = await _context.Event.FindAsync(id);
                if (@event != null)
                {
                    _context.Event.Remove(@event);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Event deleted successfully!";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting event: {ex.Message}");
                TempData["ErrorMessage"] = "Unable to delete event. It may be referenced by other records.";
                return RedirectToAction(nameof(Index));
            }
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventId == id);
        }
    }
}