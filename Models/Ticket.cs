using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TheBugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; } // PK

        [Required]
        [StringLength(75)]
        [DisplayName("Title")]
        public string Title { get; set; }

        [Required]
        [DisplayName("Description")] // description of file to upload
        public string Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Created")]
        public DateTimeOffset Created { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Created")]
        public DateTimeOffset? Updated { get; set; }

        [DisplayName("Archived")]
        public bool Archived { get; set; }

        // add ability to archive a project's tickets and restore them
        [DisplayName("Archived By Project")]
        public bool ArchivedByProject { get; set; }

        [DisplayName("Project")]
        public int ProjectId { get; set; } // FK

        [DisplayName("Ticket Type")]
        public int TicketTypeId { get; set; } // FK

        [DisplayName("Ticket Priority")]
        public int TicketPriorityId { get; set; } // FK

        [DisplayName("Ticket Status")]
        public int TicketStatusId { get; set; } // FK

        [DisplayName("Ticket Owner")]
        public string OwnerUserId { get; set; } // FK

        [DisplayName("Ticket Developer")]
        public string DeveloperUserId { get; set; } // FK



        // many tickets to one X
        public virtual Project Project { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual BTUser OwnerUser { get; set; }
        public virtual BTUser DeveloperUser { get; set; }

        // one ticket to many X
        // virtual = lazy loaded (not required anymore), will be null if not .Included(x => x.Thing)
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}
