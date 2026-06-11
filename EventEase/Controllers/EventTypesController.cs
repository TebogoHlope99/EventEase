using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEase.Data;
using EventEase.Models;

namespace EventEase.Controllers
{
    public class EventTypesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventTypesController> _logger;

        public EventTypesController(ApplicationDbContext context, ILogger<EventTypesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: EventTypes
        public async Task<IActionResult> Index(string searchString, string sortOrder, int? pageNumber)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentSort"] = sortOrder;
            // Define sort parameters for each column
            ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DescriptionSortParm"] = sortOrder == "description" ? "description_desc" : "description";
            ViewData["ColorSortParm"] = sortOrder == "color" ? "color_desc" : "color";
            ViewData["StatusSortParm"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["EventsCountSortParm"] = sortOrder == "eventsCount" ? "eventsCount_desc" : "eventsCount";

            // Get event types with event counts
            var eventTypes = await (from et in _context.EventTypes
                                    select new EventType
                                    {
                                        EventTypeId = et.EventTypeId,
                                        TypeName = et.TypeName,
                                        Description = et.Description,
                                        IconClass = et.IconClass,
                                        ColorClass = et.ColorClass,
                                        IsActive = et.IsActive,
                                        CreatedAt = et.CreatedAt,
                                        Events = _context.Event.Where(e => e.EventTypeId == et.EventTypeId).ToList()
                                    }).ToListAsync();

            // Apply sorting
            eventTypes = sortOrder switch
            {
                "name_desc" => eventTypes.OrderByDescending(et => et.TypeName).ToList(),
                "description" => eventTypes.OrderBy(et => et.Description).ToList(),
                "description_desc" => eventTypes.OrderByDescending(et => et.Description).ToList(),
                "color" => eventTypes.OrderBy(et => et.ColorClass).ToList(),
                "color_desc" => eventTypes.OrderByDescending(et => et.ColorClass).ToList(),
                "status" => eventTypes.OrderBy(et => et.IsActive).ToList(),
                "status_desc" => eventTypes.OrderByDescending(et => et.IsActive).ToList(),
                "eventsCount" => eventTypes.OrderBy(et => et.Events?.Count ?? 0).ToList(),
                "eventsCount_desc" => eventTypes.OrderByDescending(et => et.Events?.Count ?? 0).ToList(),
                _ => eventTypes.OrderBy(et => et.TypeName).ToList()
            };

            // Pagination
            int pageSize = 10;
            int pageIndex = pageNumber ?? 1;
            int totalCount = eventTypes.Count;
            var paginatedEventTypes = eventTypes.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

            var paginatedList = new PaginatedList<EventType>(paginatedEventTypes, totalCount, pageIndex, pageSize);

            return View(paginatedList);
        }

        // GET: EventTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var eventType = await _context.EventTypes
                .Include(et => et.Events)
                .FirstOrDefaultAsync(m => m.EventTypeId == id);

            if (eventType == null) return NotFound();

            return View(eventType);
        }

        // GET: EventTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: EventTypes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TypeName,Description,IconClass,ColorClass,IsActive")] EventType eventType)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Check if event type already exists
                    var exists = await _context.EventTypes.AnyAsync(et => et.TypeName == eventType.TypeName);
                    if (exists)
                    {
                        TempData["ErrorMessage"] = $"Event type '{eventType.TypeName}' already exists!";
                        return View(eventType);
                    }

                    eventType.CreatedAt = DateTime.Now;
                    _context.EventTypes.Add(eventType);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Event type '{eventType.TypeName}' created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating event type");
                    ModelState.AddModelError("", "An error occurred while creating the event type.");
                }
            }
            return View(eventType);
        }

        // GET: EventTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var eventType = await _context.EventTypes.FindAsync(id);
            if (eventType == null) return NotFound();

            return View(eventType);
        }

        // POST: EventTypes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventTypeId,TypeName,Description,IconClass,ColorClass,IsActive,CreatedAt")] EventType eventType)
        {
            if (id != eventType.EventTypeId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventType);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Event type '{eventType.TypeName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventTypeExists(eventType.EventTypeId)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating event type");
                    ModelState.AddModelError("", "An error occurred while updating the event type.");
                }
            }
            return View(eventType);
        }

        // GET: EventTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var eventType = await _context.EventTypes
                .Include(et => et.Events)
                .FirstOrDefaultAsync(m => m.EventTypeId == id);

            if (eventType == null) return NotFound();

            // Check if event type has associated events
            ViewBag.HasEvents = eventType.Events != null && eventType.Events.Any();

            return View(eventType);
        }

        // POST: EventTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventType = await _context.EventTypes
                .Include(et => et.Events)
                .FirstOrDefaultAsync(et => et.EventTypeId == id);

            if (eventType == null) return NotFound();

            // Prevent deletion if has associated events
            if (eventType.Events != null && eventType.Events.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete event type '{eventType.TypeName}' because it has associated events. Please reassign or delete those events first.";
                return RedirectToAction(nameof(Index));
            }

            _context.EventTypes.Remove(eventType);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Event type '{eventType.TypeName}' deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool EventTypeExists(int id)
        {
            return _context.EventTypes.Any(e => e.EventTypeId == id);
        }
    }
}