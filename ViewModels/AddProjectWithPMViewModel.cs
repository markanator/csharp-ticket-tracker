using Microsoft.AspNetCore.Mvc.Rendering;
using TheBugTracker.Models;

namespace TheBugTracker.ViewModels
{
    public class AddProjectWithPMViewModel
    {
        public Project Project { get; set; }
        public SelectList PMList { get; set; }
        public string PmId { get; set; }
        public SelectList PriorityList { get; set; }
    }
}
