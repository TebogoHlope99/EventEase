using EventEase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using EventEase.Data;

namespace EventEase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Venue> Venue { get; set; }
        public DbSet<Event> Event { get; set; }
        public DbSet<Booking> Booking { get; set; }
        public DbSet<EventType> EventTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table names
            modelBuilder.Entity<Venue>().ToTable("Venues");
            modelBuilder.Entity<Event>().ToTable("Events");
            modelBuilder.Entity<Booking>().ToTable("Bookings");
            modelBuilder.Entity<EventType>().ToTable("EventTypes");

            // Configure relationships and constraints
            modelBuilder.Entity<Event>()
                .HasOne(e => e.EventType)
                .WithMany(et => et.Events)
                .HasForeignKey(e => e.EventTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Event)
                .WithMany(e => e.Bookings)
                .HasForeignKey(b => b.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.Venue)
                .WithMany(v => v.Bookings)
                .HasForeignKey(b => b.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.SetNull);

            // Add unique constraint to prevent double booking
            modelBuilder.Entity<Booking>()
                .HasIndex(b => new { b.VenueId, b.BookingDate })
                .IsUnique()
                .HasName("UQ_Booking_Venue_Date");

            // Seed EventTypes
            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, TypeName = "Conference", Description = "Professional conferences and seminars", IconClass = "bi-people-fill", ColorClass = "primary", IsActive = true, CreatedAt = DateTime.Now },
                new EventType { EventTypeId = 2, TypeName = "Wedding", Description = "Wedding ceremonies and receptions", IconClass = "bi-heart-fill", ColorClass = "danger", IsActive = true, CreatedAt = DateTime.Now },
                new EventType { EventTypeId = 3, TypeName = "Concert", Description = "Live music and performances", IconClass = "bi-music-note-beamed", ColorClass = "success", IsActive = true, CreatedAt = DateTime.Now },
                new EventType { EventTypeId = 4, TypeName = "Corporate", Description = "Business meetings and corporate events", IconClass = "bi-briefcase-fill", ColorClass = "info", IsActive = true, CreatedAt = DateTime.Now },
                new EventType { EventTypeId = 5, TypeName = "Private Party", Description = "Birthdays, anniversaries, private celebrations", IconClass = "bi-gift-fill", ColorClass = "warning", IsActive = true, CreatedAt = DateTime.Now },
                new EventType { EventTypeId = 6, TypeName = "Workshop", Description = "Educational workshops and training", IconClass = "bi-mortarboard-fill", ColorClass = "secondary", IsActive = true, CreatedAt = DateTime.Now }
            );

            // Seed Venues
            modelBuilder.Entity<Venue>().HasData(
                new Venue { VenueId = 1, VenueName = "Grand Ballroom", Location = "123 Main St, City Center", Capacity = 500, ImageUrl = "https://placehold.co/600x400/3b82f6/white?text=Grand+Ballroom", IsAvailable = true, CreatedAt = DateTime.Now },
                new Venue { VenueId = 2, VenueName = "Conference Hall A", Location = "45 Business Park", Capacity = 200, ImageUrl = "https://placehold.co/600x400/10b981/white?text=Conference+Hall+A", IsAvailable = true, CreatedAt = DateTime.Now },
                new Venue { VenueId = 3, VenueName = "Garden Pavilion", Location = "789 Park Avenue", Capacity = 150, ImageUrl = "https://placehold.co/600x400/f59e0b/white?text=Garden+Pavilion", IsAvailable = true, CreatedAt = DateTime.Now },
                new Venue { VenueId = 4, VenueName = "Executive Boardroom", Location = "45 Business Park", Capacity = 30, ImageUrl = "https://placehold.co/600x400/ef4444/white?text=Executive+Boardroom", IsAvailable = true, CreatedAt = DateTime.Now }
            );

            // Seed Events
            modelBuilder.Entity<Event>().HasData(
                new Event { EventId = 1, EventName = "Annual Tech Conference", EventDate = new DateTime(2026, 6, 15, 9, 0, 0), Description = "Technology innovation showcase with keynote speakers and workshops", VenueId = 1, EventTypeId = 1, ImageUrl = "https://placehold.co/600x400/8b5cf6/white?text=Tech+Conference", CreatedAt = DateTime.Now },
                new Event { EventId = 2, EventName = "Wedding Reception", EventDate = new DateTime(2026, 7, 20, 18, 0, 0), Description = "Smith-Johnson wedding reception with dinner and dancing", VenueId = 3, EventTypeId = 2, ImageUrl = "https://placehold.co/600x400/ec4899/white?text=Wedding+Reception", CreatedAt = DateTime.Now },
                new Event { EventId = 3, EventName = "Product Launch", EventDate = new DateTime(2026, 5, 10, 14, 0, 0), Description = "New product reveal for the latest tech innovation", VenueId = 2, EventTypeId = 1, ImageUrl = "https://placehold.co/600x400/14b8a6/white?text=Product+Launch", CreatedAt = DateTime.Now },
                new Event { EventId = 4, EventName = "Board Meeting", EventDate = new DateTime(2026, 5, 5, 10, 0, 0), Description = "Quarterly review and strategic planning session", VenueId = 4, EventTypeId = 4, ImageUrl = "https://placehold.co/600x400/f97316/white?text=Board+Meeting", CreatedAt = DateTime.Now }
            );

            // Seed Bookings
            modelBuilder.Entity<Booking>().HasData(
                new Booking { BookingId = 1, EventId = 1, VenueId = 1, BookingDate = new DateTime(2026, 6, 15), CreatedAt = DateTime.Now },
                new Booking { BookingId = 2, EventId = 2, VenueId = 3, BookingDate = new DateTime(2026, 7, 20), CreatedAt = DateTime.Now },
                new Booking { BookingId = 3, EventId = 3, VenueId = 2, BookingDate = new DateTime(2026, 5, 10), CreatedAt = DateTime.Now },
                new Booking { BookingId = 4, EventId = 4, VenueId = 4, BookingDate = new DateTime(2026, 5, 5), CreatedAt = DateTime.Now }
            );
        }
    }
}

