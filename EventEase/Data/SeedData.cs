using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EventEase.Models;
using System;
using System.Linq;

namespace EventEase.Data
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Only seed if the database is empty (no venues - this is our check)
                if (context.Venue.Any())
                {
                    Console.WriteLine("Database already has data. Skipping seed.");
                    return;
                }

                Console.WriteLine("Seeding database...");

                try
                {
                    // ============================================
                    // SEED EVENTTYPES FIRST (This must be first because Events reference them)
                    // ============================================
                    var eventTypes = new EventType[]
                    {
                        new EventType {
                            TypeName = "Conference",
                            Description = "Professional conferences and seminars",
                            IconClass = "bi-people-fill",
                            ColorClass = "primary",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new EventType {
                            TypeName = "Wedding",
                            Description = "Wedding ceremonies and receptions",
                            IconClass = "bi-heart-fill",
                            ColorClass = "danger",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new EventType {
                            TypeName = "Concert",
                            Description = "Live music and performances",
                            IconClass = "bi-music-note-beamed",
                            ColorClass = "success",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new EventType {
                            TypeName = "Corporate",
                            Description = "Business meetings and corporate events",
                            IconClass = "bi-briefcase-fill",
                            ColorClass = "info",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new EventType {
                            TypeName = "Private Party",
                            Description = "Birthdays, anniversaries, private celebrations",
                            IconClass = "bi-gift-fill",
                            ColorClass = "warning",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        },
                        new EventType {
                            TypeName = "Workshop",
                            Description = "Educational workshops and training",
                            IconClass = "bi-mortarboard-fill",
                            ColorClass = "secondary",
                            IsActive = true,
                            CreatedAt = DateTime.Now
                        }
                    };
                    context.EventTypes.AddRange(eventTypes);
                    context.SaveChanges();
                    Console.WriteLine($"Added {eventTypes.Length} event types");

                    // ============================================
                    // SEED VENUES
                    // ============================================
                    var venues = new Venue[]
                    {
                        new Venue
                        {
                            VenueName = "Grand Ballroom",
                            Location = "123 Main St, City Center",
                            Capacity = 500,
                            ImageUrl = "https://placehold.co/600x400/3b82f6/white?text=Grand+Ballroom",
                            IsAvailable = true,
                            CreatedAt = DateTime.Now
                        },
                        new Venue
                        {
                            VenueName = "Conference Hall A",
                            Location = "45 Business Park",
                            Capacity = 200,
                            ImageUrl = "https://placehold.co/600x400/10b981/white?text=Conference+Hall+A",
                            IsAvailable = true,
                            CreatedAt = DateTime.Now
                        },
                        new Venue
                        {
                            VenueName = "Garden Pavilion",
                            Location = "789 Park Avenue",
                            Capacity = 150,
                            ImageUrl = "https://placehold.co/600x400/f59e0b/white?text=Garden+Pavilion",
                            IsAvailable = true,
                            CreatedAt = DateTime.Now
                        },
                        new Venue
                        {
                            VenueName = "Executive Boardroom",
                            Location = "45 Business Park",
                            Capacity = 30,
                            ImageUrl = "https://placehold.co/600x400/ef4444/white?text=Executive+Boardroom",
                            IsAvailable = true,
                            CreatedAt = DateTime.Now
                        }
                    };
                    context.Venue.AddRange(venues);
                    context.SaveChanges();
                    Console.WriteLine($"Added {venues.Length} venues");

                    // Retrieve saved IDs for relationships
                    var savedEventTypes = context.EventTypes.ToList();
                    var savedVenues = context.Venue.ToList();

                    // ============================================
                    // SEED EVENTS
                    // ============================================
                    var events = new Event[]
                    {
                        new Event
                        {
                            EventName = "Annual Tech Conference",
                            EventDate = new DateTime(2026, 6, 15, 9, 0, 0),
                            Description = "Technology innovation showcase with keynote speakers and workshops",
                            VenueId = savedVenues.First(v => v.VenueName == "Grand Ballroom").VenueId,
                            EventTypeId = savedEventTypes.First(et => et.TypeName == "Conference").EventTypeId,
                            ImageUrl = "https://placehold.co/600x400/8b5cf6/white?text=Tech+Conference",
                            CreatedAt = DateTime.Now
                        },
                        new Event
                        {
                            EventName = "Wedding Reception",
                            EventDate = new DateTime(2026, 7, 20, 18, 0, 0),
                            Description = "Smith-Johnson wedding reception with dinner and dancing",
                            VenueId = savedVenues.First(v => v.VenueName == "Garden Pavilion").VenueId,
                            EventTypeId = savedEventTypes.First(et => et.TypeName == "Wedding").EventTypeId,
                            ImageUrl = "https://placehold.co/600x400/ec4899/white?text=Wedding+Reception",
                            CreatedAt = DateTime.Now
                        },
                        new Event
                        {
                            EventName = "Product Launch",
                            EventDate = new DateTime(2026, 5, 10, 14, 0, 0),
                            Description = "New product reveal for the latest tech innovation",
                            VenueId = savedVenues.First(v => v.VenueName == "Conference Hall A").VenueId,
                            EventTypeId = savedEventTypes.First(et => et.TypeName == "Conference").EventTypeId,
                            ImageUrl = "https://placehold.co/600x400/14b8a6/white?text=Product+Launch",
                            CreatedAt = DateTime.Now
                        },
                        new Event
                        {
                            EventName = "Board Meeting",
                            EventDate = new DateTime(2026, 5, 5, 10, 0, 0),
                            Description = "Quarterly review and strategic planning session",
                            VenueId = savedVenues.First(v => v.VenueName == "Executive Boardroom").VenueId,
                            EventTypeId = savedEventTypes.First(et => et.TypeName == "Corporate").EventTypeId,
                            ImageUrl = "https://placehold.co/600x400/f97316/white?text=Board+Meeting",
                            CreatedAt = DateTime.Now
                        },
                        new Event
                        {
                            EventName = "Summer Music Festival",
                            EventDate = new DateTime(2026, 8, 5, 14, 0, 0),
                            Description = "Annual summer music festival with multiple artists",
                            VenueId = savedVenues.First(v => v.VenueName == "Grand Ballroom").VenueId,
                            EventTypeId = savedEventTypes.First(et => et.TypeName == "Concert").EventTypeId,
                            ImageUrl = "https://placehold.co/600x400/dc2626/white?text=Music+Festival",
                            CreatedAt = DateTime.Now
                        },
                        new Event
                        {
                            EventName = "Leadership Workshop",
                            EventDate = new DateTime(2026, 9, 10, 10, 0, 0),
                            Description = "Leadership development and team building workshop",
                            VenueId = savedVenues.First(v => v.VenueName == "Conference Hall A").VenueId,
                            EventTypeId = savedEventTypes.First(et => et.TypeName == "Workshop").EventTypeId,
                            ImageUrl = "https://placehold.co/600x400/8b5cf6/white?text=Workshop",
                            CreatedAt = DateTime.Now
                        }
                    };
                    context.Event.AddRange(events);
                    context.SaveChanges();
                    Console.WriteLine($"Added {events.Length} events");

                    // Retrieve saved Event IDs
                    var savedEvents = context.Event.ToList();

                    // ============================================
                    // SEED BOOKINGS
                    // ============================================
                    var bookings = new Booking[]
                    {
                        new Booking
                        {
                            EventId = savedEvents.First(e => e.EventName == "Annual Tech Conference").EventId,
                            VenueId = savedVenues.First(v => v.VenueName == "Grand Ballroom").VenueId,
                            BookingDate = new DateTime(2026, 6, 15),
                            CreatedAt = DateTime.Now
                        },
                        new Booking
                        {
                            EventId = savedEvents.First(e => e.EventName == "Wedding Reception").EventId,
                            VenueId = savedVenues.First(v => v.VenueName == "Garden Pavilion").VenueId,
                            BookingDate = new DateTime(2026, 7, 20),
                            CreatedAt = DateTime.Now
                        },
                        new Booking
                        {
                            EventId = savedEvents.First(e => e.EventName == "Product Launch").EventId,
                            VenueId = savedVenues.First(v => v.VenueName == "Conference Hall A").VenueId,
                            BookingDate = new DateTime(2026, 5, 10),
                            CreatedAt = DateTime.Now
                        },
                        new Booking
                        {
                            EventId = savedEvents.First(e => e.EventName == "Board Meeting").EventId,
                            VenueId = savedVenues.First(v => v.VenueName == "Executive Boardroom").VenueId,
                            BookingDate = new DateTime(2026, 5, 5),
                            CreatedAt = DateTime.Now
                        }
                    };
                    context.Booking.AddRange(bookings);
                    context.SaveChanges();
                    Console.WriteLine($"Added {bookings.Length} bookings");

                    Console.WriteLine("========================================");
                    Console.WriteLine("Database seeding completed successfully!");
                    Console.WriteLine($"Summary: {eventTypes.Length} EventTypes, {venues.Length} Venues, {events.Length} Events, {bookings.Length} Bookings");
                    Console.WriteLine("========================================");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding database: {ex.Message}");
                    Console.WriteLine(ex.StackTrace);
                    throw;
                }
            }
        }
    }
}