using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;
using EventEase.Services;
using Microsoft.Extensions.Configuration;

namespace EventEase.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBlobStorageService _blobService;
        private readonly ILogger<VenuesController> _logger;
        private readonly IConfiguration _configuration;

        public VenuesController(
            ApplicationDbContext context,
            IBlobStorageService blobService,
            ILogger<VenuesController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _blobService = blobService;
            _logger = logger;
            _configuration = configuration;
        }

        // GET: Venues with Advanced Filtering
        public async Task<IActionResult> Index(
            string searchString,
            string sortOrder,
            int? minCapacity,
            int? maxCapacity,
            string location,
            string availability,
            string status,
            int? minBookings,
            int? maxBookings,
            int? pageNumber)
        {
            // Store filter values for the view
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentMinCapacity"] = minCapacity;
            ViewData["CurrentMaxCapacity"] = maxCapacity;
            ViewData["CurrentLocation"] = location;
            ViewData["CurrentAvailability"] = availability;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentMinBookings"] = minBookings;
            ViewData["CurrentMaxBookings"] = maxBookings;

            // Define sort parameters
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CapacitySortParm"] = sortOrder == "capacity" ? "capacity_desc" : "capacity";
            ViewData["LocationSortParm"] = sortOrder == "location" ? "location_desc" : "location";
            ViewData["CreatedSortParm"] = sortOrder == "created" ? "created_desc" : "created";
            ViewData["BookingsSortParm"] = sortOrder == "bookings" ? "bookings_desc" : "bookings";

            // Get distinct locations for filter dropdown
            var locationsList = await _context.Venue
                .Select(v => v.Location)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
            ViewBag.Locations = new SelectList(locationsList);

            // Get venues with booking counts
            var venues = from v in _context.Venue
                         select new Venue
                         {
                             VenueId = v.VenueId,
                             VenueName = v.VenueName,
                             Location = v.Location,
                             Capacity = v.Capacity,
                             ImageUrl = v.ImageUrl,
                             IsAvailable = v.IsAvailable,
                             CreatedAt = v.CreatedAt,
                             Events = v.Events,
                             Bookings = v.Bookings
                         };

            // Apply search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                venues = venues.Where(v => v.VenueName.Contains(searchString)
                                    || v.Location.Contains(searchString));
            }

            // Apply capacity range filter
            if (minCapacity.HasValue && minCapacity > 0)
            {
                venues = venues.Where(v => v.Capacity >= minCapacity);
            }
            if (maxCapacity.HasValue && maxCapacity > 0)
            {
                venues = venues.Where(v => v.Capacity <= maxCapacity);
            }

            // Apply location filter
            if (!string.IsNullOrEmpty(location))
            {
                venues = venues.Where(v => v.Location.Contains(location));
            }

            // Apply availability filter
            if (!string.IsNullOrEmpty(availability))
            {
                bool isAvailable = availability == "available";
                venues = venues.Where(v => v.IsAvailable == isAvailable);
            }

            // Apply status filter (based on bookings count)
            if (!string.IsNullOrEmpty(status))
            {
                var venuesList = venues.ToList();
                if (status == "hasBookings")
                {
                    venues = venues.Where(v => v.Bookings != null && v.Bookings.Any()).AsQueryable();
                }
                else if (status == "noBookings")
                {
                    venues = venues.Where(v => v.Bookings == null || !v.Bookings.Any()).AsQueryable();
                }
            }

            // Apply bookings count filter
            if (minBookings.HasValue && minBookings > 0)
            {
                venues = venues.Where(v => v.Bookings != null && v.Bookings.Count >= minBookings);
            }
            if (maxBookings.HasValue && maxBookings > 0)
            {
                venues = venues.Where(v => v.Bookings != null && v.Bookings.Count <= maxBookings);
            }

            // Apply sorting
            venues = sortOrder switch
            {
                "name_desc" => venues.OrderByDescending(v => v.VenueName),
                "capacity" => venues.OrderBy(v => v.Capacity),
                "capacity_desc" => venues.OrderByDescending(v => v.Capacity),
                "location" => venues.OrderBy(v => v.Location),
                "location_desc" => venues.OrderByDescending(v => v.Location),
                "created" => venues.OrderBy(v => v.CreatedAt),
                "created_desc" => venues.OrderByDescending(v => v.CreatedAt),
                "bookings" => venues.OrderBy(v => v.Bookings != null ? v.Bookings.Count() : 0),
                "bookings_desc" => venues.OrderByDescending(v => v.Bookings != null ? v.Bookings.Count() : 0),
                _ => venues.OrderBy(v => v.VenueName)
            };

            // Get stats for filter counts
            var venuesListForStats = await venues.ToListAsync();
            ViewBag.TotalCount = venuesListForStats.Count;
            ViewBag.AvailableCount = venuesListForStats.Count(v => v.IsAvailable);
            ViewBag.UnavailableCount = venuesListForStats.Count(v => !v.IsAvailable);
            ViewBag.AvgCapacity = venuesListForStats.Any() ? (int)venuesListForStats.Average(v => v.Capacity) : 0;

            // Pagination
            int pageSize = 10;
            var paginatedVenues = await PaginatedList<Venue>.CreateAsync(venues.AsNoTracking(), pageNumber ?? 1, pageSize);

            return View(paginatedVenues);
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .Include(v => v.Events)
                .Include(v => v.Bookings)
                .ThenInclude(b => b.Event)
                .FirstOrDefaultAsync(m => m.VenueId == id);

            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Venue venue, IFormFile? imageFile)
        {
            try
            {
                // Log the start of the process
                _logger.LogInformation("Starting venue creation");

                // Remove navigation properties from validation
                ModelState.Remove("Events");
                ModelState.Remove("Bookings");

                if (ModelState.IsValid)
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        _logger.LogInformation($"Uploading file: {imageFile.FileName}, Size: {imageFile.Length} bytes");

                        try
                        {
                            var imageUrl = await _blobService.UploadImageAsync(imageFile, "venue-images");
                            venue.ImageUrl = imageUrl;
                            _logger.LogInformation($"Image uploaded successfully: {imageUrl}");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Blob storage error");
                            ModelState.AddModelError("", $"Image upload failed: {ex.Message}");
                            return View(venue);
                        }
                    }
                    else
                    {
                        venue.ImageUrl = _blobService.GetDefaultImageUrl("venue");
                        _logger.LogInformation("No image provided, using default");
                    }

                    venue.CreatedAt = DateTime.Now;
                    venue.IsAvailable = true;
                    _context.Venue.Add(venue);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Venue '{venue.VenueName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }

                // If ModelState is invalid, show the errors
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning($"ModelState invalid: {errors}");
                ModelState.AddModelError("", $"Validation failed: {errors}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating venue");
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(venue);
        }

        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Venue venue, IFormFile? imageFile, bool deleteImage = false)
        {
            if (id != venue.VenueId)
            {
                return NotFound();
            }

            // Remove navigation properties from validation
            ModelState.Remove("Events");
            ModelState.Remove("Bookings");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venue.AsNoTracking().FirstOrDefaultAsync(v => v.VenueId == id);
                    if (existingVenue == null) return NotFound();

                    // Handle image changes
                    if (deleteImage && !string.IsNullOrEmpty(existingVenue.ImageUrl))
                    {
                        await _blobService.DeleteImageAsync(existingVenue.ImageUrl, "venue-images");
                        venue.ImageUrl = _blobService.GetDefaultImageUrl("venue");
                    }
                    else if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image if exists and not default
                        if (!string.IsNullOrEmpty(existingVenue.ImageUrl) &&
                            !existingVenue.ImageUrl.Contains("placehold.co"))
                        {
                            await _blobService.DeleteImageAsync(existingVenue.ImageUrl, "venue-images");
                        }

                        venue.ImageUrl = await _blobService.UploadImageAsync(imageFile, "venue-images");
                    }
                    else
                    {
                        venue.ImageUrl = existingVenue.ImageUrl;
                    }

                    venue.CreatedAt = existingVenue.CreatedAt;
                    _context.Venue.Update(venue);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Venue '{venue.VenueName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueId))
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
                    _logger.LogError(ex, "Error updating venue");
                    ModelState.AddModelError("", "An error occurred while updating.");
                }
            }
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null)
            {
                return NotFound();
            }

            // Check if venue has any bookings
            var hasBookings = venue.Bookings != null && venue.Bookings.Any();
            ViewBag.HasBookings = hasBookings;

            return View(venue);
        }

        // POST: Venues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venue = await _context.Venue
                .Include(v => v.Bookings)
                .FirstOrDefaultAsync(v => v.VenueId == id);

            if (venue == null) return NotFound();

            // Prevent deletion if has bookings
            if (venue.Bookings != null && venue.Bookings.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete venue '{venue.VenueName}' because it has existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            // Delete image from blob storage if not default
            if (!string.IsNullOrEmpty(venue.ImageUrl) && !venue.ImageUrl.Contains("placehold.co"))
            {
                try
                {
                    await _blobService.DeleteImageAsync(venue.ImageUrl, "venue-images");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Failed to delete image from blob storage: {venue.ImageUrl}");
                    // Continue with deletion even if image deletion fails
                }
            }

            _context.Venue.Remove(venue);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Venue '{venue.VenueName}' deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Venues/CheckStorage - Debug method to test storage connection
        public async Task<IActionResult> CheckStorage()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine("<h2>Storage Connection Test</h2>");
            result.AppendLine("<hr/>");

            try
            {
                // Check if connection string exists
                var connString = _configuration["AzureStorage:ConnectionString"];
                result.AppendLine($"<p><strong>Connection String Present:</strong> {(string.IsNullOrEmpty(connString) ? "? NO" : "? YES")}</p>");

                if (string.IsNullOrEmpty(connString))
                {
                    result.AppendLine("<p style='color:red'>ERROR: Connection string not found in configuration!</p>");
                    result.AppendLine("<p>Please add 'AzureStorage:ConnectionString' to your appsettings.json or Azure App Settings.</p>");
                }
                else
                {
                    result.AppendLine($"<p><strong>Connection String Length:</strong> {connString.Length} characters</p>");

                    // Try to test blob service connection
                    try
                    {
                        var testResult = await _blobService.TestConnectionAsync();
                        result.AppendLine($"<p><strong>Blob Service Connection:</strong> {(testResult ? "? Working" : "? Failed")}</p>");
                    }
                    catch (Exception ex)
                    {
                        result.AppendLine($"<p style='color:red'>Blob Service Error: {ex.Message}</p>");
                    }
                }
            }
            catch (Exception ex)
            {
                result.AppendLine($"<p style='color:red'>ERROR: {ex.Message}</p>");
                result.AppendLine($"<pre>{ex.StackTrace}</pre>");
            }

            result.AppendLine("<hr/>");
            result.AppendLine("<p><a href='/Venues' class='btn btn-primary'>Back to Venues</a></p>");

            return Content(result.ToString(), "text/html");
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueId == id);
        }
    }
}