using Microsoft.AspNetCore.Mvc;

namespace EventEase.Models
{
    public class BookingViewModel
    {
        public int BookingId { get; set; }
        public string? EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string? VenueName { get; set; }
        public string? Location { get; set; }
        public int Capacity { get; set; }
        public DateTime BookingDate { get; set; }
        public string? EventDescription { get; set; }
        public string? VenueImageUrl { get; set; }
        public string? EventImageUrl { get; set; }
        public string Status => BookingDate >= DateTime.Now.Date ? "Upcoming" : "Past";
        public bool IsUpcoming => BookingDate >= DateTime.Now.Date;
    }
}
