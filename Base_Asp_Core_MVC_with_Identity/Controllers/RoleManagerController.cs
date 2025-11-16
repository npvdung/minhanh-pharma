using Base_Asp_Core_MVC_with_Identity.CommonFile.Enum;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class RoleManagerController : Controller
    {
        private readonly UserManager<UserSystemIdentity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleManagerController(UserManager<UserSystemIdentity> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddRole(string roleName)
        {
            if (roleName != null)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName.Trim()));
            }
            return RedirectToAction("Index");
        }
    }
}
