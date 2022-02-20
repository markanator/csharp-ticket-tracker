using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Extensions;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;
using TheBugTracker.ViewModels;

namespace TheBugTracker.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly IRoleService _roleService;
        private readonly ICompanyInfoService _companyInfoService;

        public UserRolesController(IRoleService role, ICompanyInfoService companyInfo)
        {
            this._roleService = role;
            this._companyInfoService = companyInfo;
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            // add instance of viewModel as a list
            List<ManageUserRolesViewModels> model = new();
            // get company Id
            int comanyId = User.Identity.GetCompanyId().Value;
            // get all company User
            List<BTUser> members = await _companyInfoService.GetAllMembersAsync(comanyId);

            //loop over users to populate VM
            foreach (var user in members)
            {
                // isntantiate VM
                ManageUserRolesViewModels vm = new();
                vm.User = user;
                // use roleService to 
                IEnumerable<string> selectedRole = await _roleService.GetUserRolesAsync(user);
                // create multiselectList
                vm.Roles = new MultiSelectList(await _roleService.GetRolesAsync(), "Name", "Name", selectedRole);

                // add userInfo, userRoles to
                model.Add(vm);
            }

            // return VM to view
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModels member)
        {
            // get companyId
            int companyId = User.Identity.GetCompanyId().Value;
            // init the user
            BTUser user = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(m => m.Id == member.User.Id);
            // get roles of the user
            IEnumerable<string> roles = await _roleService.GetUserRolesAsync(user);
            // grab the selected roles
            string userRole = member.SelectedRoles.FirstOrDefault();

            if (!string.IsNullOrEmpty(userRole))
            {
                // remove user from old roles
                if (await _roleService.RemoveUserFromRolesAsync(user, roles))
                {
                    // add user to the new role
                    await _roleService.AddUserToRoleAsync(user, userRole);
                }

            }
            // navigate back to the view
            return View();
        }
    }
}
