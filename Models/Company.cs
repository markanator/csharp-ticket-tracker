using System.Collections.Generic;
using System.ComponentModel;

namespace TheBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; } // PK

        [DisplayName("Company Name")]
        public string Name { get; set; }

        [DisplayName("Company Description")]
        public string Description { get; set; }

        // navigation props
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

        // TODO: INVITES
    }
}
