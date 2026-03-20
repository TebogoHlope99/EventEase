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
                // Check if data already exists
                if (context.Venue.Any())
                {
                    Console.WriteLine("Database already has data. Skipping seed.");
                    return;
                }

                Console.WriteLine("Seeding database...");

                try
                {
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
                            CreatedAt = DateTime.Now
                        },
                        new Venue
                        {
                            VenueName = "Conference Hall A",
                            Location = "45 Business Park",
                            Capacity = 200,
                            ImageUrl = "https://placehold.co/600x400/10b981/white?text=Conference+Hall+A",
                            CreatedAt = DateTime.Now
                        },
                        new Venue
                        {
                            VenueName = "Garden Pavilion",
                            Location = "789 Park Avenue",
                            Capacity = 150,
                            ImageUrl = "https://placehold.co/600x400/f59e0b/white?text=Garden+Pavilion",
                            CreatedAt = DateTime.Now
                        },
                        new Venue
                        {
                            VenueName = "Executive Boardroom",
                            Location = "45 Business Park",
                            Capacity = 30,
                            ImageUrl = "https://placehold.co/600x400/ef4444/white?text=Executive+Boardroom",
                            CreatedAt = DateTime.Now
                        }
                    };
                    context.Venue.AddRange(venues);
                    context.SaveChanges();
                    Console.WriteLine($"Added {venues.Length} venues");

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
                            VenueId = venues[0].VenueId,
                            ImageUrl = "https://placehold.co/600x400/8b5cf6/white?text=Tech+Conference",
                            CreatedAt = DateTime.Now
                        },
                        new Event
                        {
                            EventName = "Wedding Reception",
                            EventDate = new DateTime(2026, 7, 20, 18, 0, 0),
                            Description = "Smith-Johnson wedding reception with dinner and dancing",
                            VenueId = venues[2].VenueId,
                            ImageUrl = "https://placehold.co/600x400/ec4899/white?text=Wedding+Reception",
                            CreatedAt = DateTime.Now
                        },
                        new Event
                        {
                            EventName = "Product Launch",
                            EventDate = new DateTime(2026, 5, 10, 14, 0, 0),
                            Description = "New product reveal for the latest tech innovation",
                            VenueId = venues[1].VenueId,
                            ImageUrl = "https://placehold.co/600x400/14b8a6/white?text=Product+Launch",
                            CreatedAt = DateTime.Now
                        },
                        new Event
                        {
                            EventName = "Board Meeting",
                            EventDate = new DateTime(2026, 5, 5, 10, 0, 0),
                            Description = "Quarterly review and strategic planning session",
                            VenueId = venues[3].VenueId,
                            ImageUrl = "https://placehold.co/600x400/f97316/white?text=Board+Meeting",
                            CreatedAt = DateTime.Now
                        }
                    };
                    context.Event.AddRange(events);
                    context.SaveChanges();
                    Console.WriteLine($"Added {events.Length} events");

                    // ============================================
                    // SEED BOOKINGS
                    // ============================================
                    var bookings = new Booking[]
                    {
                        new Booking
                        {
                            EventId = events[0].EventId,
                            VenueId = venues[0].VenueId,
                            BookingDate = new DateTime(2026, 6, 15),
                            CreatedAt = DateTime.Now
                        },
                        new Booking
                        {
                            EventId = events[1].EventId,
                            VenueId = venues[2].VenueId,
                            BookingDate = new DateTime(2026, 7, 20),
                            CreatedAt = DateTime.Now
                        },
                        new Booking
                        {
                            EventId = events[2].EventId,
                            VenueId = venues[1].VenueId,
                            BookingDate = new DateTime(2026, 5, 10),
                            CreatedAt = DateTime.Now
                        },
                        new Booking
                        {
                            EventId = events[3].EventId,
                            VenueId = venues[3].VenueId,
                            BookingDate = new DateTime(2026, 5, 5),
                            CreatedAt = DateTime.Now
                        }
                    };
                    context.Booking.AddRange(bookings);
                    context.SaveChanges();
                    Console.WriteLine($"Added {bookings.Length} bookings");

                    Console.WriteLine("Database seeding completed successfully!");
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