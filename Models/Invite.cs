using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TheBugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; } // PK

        [DisplayName("Date Sent")]
        public DateTimeOffset InviteDate { get; set; } // sent date
        
        [DisplayName("Join Date")]
        public DateTimeOffset JoinDate { get; set; } // sent date
        
        [DisplayName("Code")]
        public Guid CompanyToken { get; set; }

        [DisplayName("Company")]
        public int CompanyId { get; set; } // FK

        [DisplayName("Project")]
        public int ProjectId { get; set; } // FK

        [DisplayName("Invitor")]
        public string InvitorId { get; set; } // FK

        [DisplayName("Invitee")]
        public string InviteeId { get; set; } // FK

        [DisplayName("Invitee Email")]
        public string InviteeEmail { get; set; }

        [DisplayName("Invitee First Name")]
        public string InviteeFirstName { get; set; }

        [DisplayName("Invitee Last Name")]
        public string InviteeLastName { get; set; }

        public bool IsValid { get; set; } // flag

        // navigation props
        public virtual Company Company { get; set; }
        public virtual Project Project { get; set; }
        public virtual BTUser Invitor { get; set; }
        public virtual BTUser Invitee { get; set; }
    }
}
