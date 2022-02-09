using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TheBugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; } // PK

        [Required]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        public string Message { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date")]
        public DateTimeOffset Created { get; set; } // created

        [DisplayName("Has been viewed")]
        public bool Viewed { get; set; }

        [DisplayName("Ticket")]
        public int TicketId { get; set; } // FK

        [Required]
        [DisplayName("Recipient")]
        public string RecipientId { get; set; } // FK

        [Required]
        [DisplayName("Sender")]
        public string SenderId { get; set; } // FK

        // navigation props
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser Recipient { get; set; }
        public virtual BTUser Sender { get; set; }
    }
}
