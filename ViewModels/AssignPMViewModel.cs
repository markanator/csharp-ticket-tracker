using Microsoft.AspNetCore.Mvc.Rendering;
using TheBugTracker.Models;

namespace TheBugTracker.ViewModels
{
    public class AssignPMViewModel
    {
        public Project Project { get; set; }
        public SelectList ProjectManagerList { get; set; }
        public string PMID { get; set; }
    }
}
