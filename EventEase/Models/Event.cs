using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required(ErrorMessage = "Event name is required")]
        [Display(Name = "Event Name")]
        [StringLength(100)]
        public string EventName { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        [Display(Name = "Event Date")]
        [DataType(DataType.DateTime)]
        public DateTime EventDate { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Venue")]
        public int? VenueId { get; set; }

        [Display(Name = "Image URL")]
        [DataType(DataType.ImageUrl)]
        public string? ImageUrl { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }
        public virtual ICollection<Booking>? Bookings { get; set; }
    }
}