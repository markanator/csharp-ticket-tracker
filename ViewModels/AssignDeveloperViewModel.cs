using Microsoft.AspNetCore.Mvc.Rendering;
using TheBugTracker.Models;

namespace TheBugTracker.ViewModels
{
	public class AssignDeveloperViewModel
	{
		public Ticket Ticket { get; set; }
		public SelectList Developers { get; set; }
		public string DeveloperId { get; set; }
	}
}
