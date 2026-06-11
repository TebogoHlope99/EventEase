using EventEase.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required(ErrorMessage = "Venue name is required")]
        [Display(Name = "Venue Name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Venue name must be between 2 and 100 characters")]
        public string VenueName { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [Display(Name = "Location")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Location must be between 5 and 200 characters")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10,000")]
        public int Capacity { get; set; }

        [Display(Name = "Image")]
        //[DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Is Available?")]
        public bool IsAvailable { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<Event>? Events { get; set; }
        public virtual ICollection<Booking>? Bookings { get; set; }


        // Method to update availability based on bookings
        public async Task UpdateAvailabilityAsync(ApplicationDbContext context)
        {
            var hasActiveBookings = await context.Booking
                .AnyAsync(b => b.VenueId == VenueId && b.BookingDate >= DateTime.Now.Date);

            IsAvailable = !hasActiveBookings;
        }
    }
}