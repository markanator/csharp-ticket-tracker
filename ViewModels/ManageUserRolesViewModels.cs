using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using TheBugTracker.Models;

namespace TheBugTracker.ViewModels
{
    public class ManageUserRolesViewModels
    {
        public BTUser User { get; set; }
        public MultiSelectList Roles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}
