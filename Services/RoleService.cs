using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManger;

        public RoleService(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<BTUser> userManger)
        {
            this._context = context;
            this._roleManager = roleManager;
            this._userManger = userManger;
        }

        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            // wrap paranthesis to return success property
            return (await _userManger.AddToRoleAsync(user, roleName)).Succeeded;
        }

        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            // find the role in the database by ID
            IdentityRole role = _context.Roles.Find(roleId);
            // return string name of role
            return await _roleManager.GetRoleNameAsync(role);
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            try
            {
                // fetch and return the list of current Roles from db
                return await _context.Roles.ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            // fetch all roles for a given user
            IEnumerable<string> result = await _userManger.GetRolesAsync(user);
            return result;
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            // fetch and convert to List
            List<BTUser> users = (await _userManger.GetUsersInRoleAsync(roleName)).ToList();
            // multi-tenancy
            List<BTUser> results = users.Where(u => u.CompanyId == companyId).ToList();
            return results;
        }

        public async Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            // fetch list of users, of given role
            List<string> userIds = (await _userManger.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();
            // If list above does not contain this userId, keep it
            List<BTUser> roleUsers = _context.Users.Where(u => !userIds.Contains(u.Id)).ToList();
            // Multitenancy check
            List<BTUser> results = _context.Users.Where(u => u.CompanyId == companyId).ToList();

            return results;
        }

        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            return await _userManger.IsInRoleAsync(user, roleName);
        }

        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            return (await _userManger.RemoveFromRoleAsync(user, roleName)).Succeeded;
        }

        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles)
        {
            return (await _userManger.RemoveFromRolesAsync(user, roles)).Succeeded;
        }
    }
}
