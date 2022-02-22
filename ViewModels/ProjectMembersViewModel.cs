using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using TheBugTracker.Models;

namespace TheBugTracker.ViewModels
{
	public class ProjectMembersViewModel
	{
		public Project Project { get; set; }
		public SelectList Members { get; set; }
		public List<string> SelectedMembers { get; set; }
	}
}
