using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using TheBugTracker.Models;

namespace TheBugTracker.Services.Factories
{
    public class BTUserClaimsPrincipleFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
    {
        public BTUserClaimsPrincipleFactory(UserManager<BTUser> userManager,
                                            RoleManager<IdentityRole> roleManager,
                                            IOptions<IdentityOptions> options)
                                            : base(userManager, roleManager, options) { }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
        {
            // using base class method
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            // add new claim to BtUser
            // give us access to read a logged in user's companyId
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            return identity;
        }
    }
}
