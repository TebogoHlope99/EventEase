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
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Event name must be between 2 and 100 characters")]
        public string EventName { get; set; }

        [Required(ErrorMessage = "Event date is required")]
        [Display(Name = "Event Date")]
        [DataType(DataType.DateTime)]
        [CustomValidation(typeof(Event), nameof(ValidateEventDate))]
        public DateTime EventDate { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Venue")]
        public int? VenueId { get; set; }

        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }

        [Display(Name = "Image")]
        public string? ImageUrl { get; set; }

        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("VenueId")]
        public virtual Venue? Venue { get; set; }

        [ForeignKey("EventTypeId")]
        public virtual EventType? EventType { get; set; }
        public virtual ICollection<Booking>? Bookings { get; set; }

        // Custom validation
        public static ValidationResult? ValidateEventDate(DateTime eventDate, ValidationContext context)
        {
            if (eventDate < DateTime.Now)
            {
                return new ValidationResult("Event date cannot be in the past");
            }
            return ValidationResult.Success;
        }
    }
}