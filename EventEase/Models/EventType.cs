using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventEase.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required(ErrorMessage = "Event type name is required")]
        [Display(Name = "Event Type")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Event type must be between 2 and 50 characters")]
        public string TypeName { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Display(Name = "Icon Class")]
        public string? IconClass { get; set; }

        [Display(Name = "Color Class")]
        public string? ColorClass { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Event>? Events { get; set; }
    }
}