using EventEase.Data;
using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace EventEase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get counts for statistics cards
                ViewBag.VenueCount = await _context.Venue.CountAsync();
                ViewBag.EventCount = await _context.Event.CountAsync();
                ViewBag.BookingCount = await _context.Booking.CountAsync();

                // Get recent venues (last 5)
                ViewBag.RecentVenues = await _context.Venue
                    .OrderByDescending(v => v.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                // Get upcoming events (next 5)
                ViewBag.UpcomingEvents = await _context.Event
                    .Include(e => e.Venue)
                    .Where(e => e.EventDate >= DateTime.Now)
                    .OrderBy(e => e.EventDate)
                    .Take(5)
                    .ToListAsync();

                // Get recent bookings (last 5)
                ViewBag.RecentBookings = await _context.Booking
                    .Include(b => b.Event)
                    .Include(b => b.Venue)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");

                // Set default values if error occurs
                ViewBag.VenueCount = 0;
                ViewBag.EventCount = 0;
                ViewBag.BookingCount = 0;
                ViewBag.RecentVenues = new List<Venue>();
                ViewBag.UpcomingEvents = new List<Event>();
                ViewBag.RecentBookings = new List<Booking>();

                TempData["ErrorMessage"] = "Unable to load data. Please check database connection.";

                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}