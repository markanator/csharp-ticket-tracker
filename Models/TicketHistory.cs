using System;
using System.ComponentModel;

namespace TheBugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; }

        [DisplayName("Updated Item")] // name of property that was changed
        public string Property { get; set; }
        
        [DisplayName("Previous")] // prev val of property
        public string OldValue { get; set; }
        
        [DisplayName("Current")] // current val of property
        public string NewValue { get; set; }

        [DisplayName("Date Modified")] // created
        public DateTimeOffset Created { get; set; }

        [DisplayName("Description of Change")] // description of change made
        public string Description { get; set; }

        [DisplayName("Team Member")] // person making the change
        public string UserId { get; set; }

        // navigation properties
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}
