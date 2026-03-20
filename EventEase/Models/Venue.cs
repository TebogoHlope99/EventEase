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
        [StringLength(100)]
        public string VenueName { get; set; }

        [Required(ErrorMessage = "Location is required")]
        [StringLength(200)]
        public string Location { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Range(1, 10000, ErrorMessage = "Capacity must be between 1 and 10000")]
        public int Capacity { get; set; }

        [Display(Name = "Image URL")]
        [DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public virtual ICollection<Event>? Events { get; set; }
        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}