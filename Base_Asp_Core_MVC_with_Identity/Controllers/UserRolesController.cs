using Base_Asp_Core_MVC_with_Identity.CommonFile.Enum;
using Base_Asp_Core_MVC_with_Identity.Models.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class UserRolesController : Controller
    {
        private readonly UserManager<UserSystemIdentity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public UserRolesController(UserManager<UserSystemIdentity> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRolesViewModel = new List<UserRolesViewModel>();
            foreach (UserSystemIdentity user in users)
            {
                var thisViewModel = new UserRolesViewModel();
                thisViewModel.UserId = user.Id;
                thisViewModel.Email = user.Email;
                thisViewModel.FirstName = user.FirstName;
                thisViewModel.LastName = user.LastName;
                thisViewModel.Roles = await GetUserRoles(user);
                userRolesViewModel.Add(thisViewModel);
            }
            return View(userRolesViewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(string userId)
        {
                ViewBag.userId = userId;
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    ViewBag.ErrorMessage = $"User with Id = {userId} cannot be found";
                    return View("NotFound");
                }
                ViewBag.UserName = user.UserName;
                var model = new List<ManageUserRolesViewModel>();
                foreach (var role in _roleManager.Roles)
                {

                    var userRolesViewModel = new ManageUserRolesViewModel
                    {
                        RoleId = role.Id,
                        RoleName = role.Name
                    };
                    //if (await _userManager.IsInRoleAsync(user, role.Name))
                    //{
                    //    userRolesViewModel.Selected = true;
                    //}
                    //else
                    //{
                    //    userRolesViewModel.Selected = false;
                    //}
                    model.Add(userRolesViewModel);
                }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Manage(List<ManageUserRolesViewModel> model, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return View();
            }
            var roles = await _userManager.GetRolesAsync(user);
            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot remove user existing roles");
                return View(model);
            }
            result = await _userManager.AddToRolesAsync(user, model.Where(x => x.Selected).Select(y => y.RoleName));
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Cannot add selected roles to user");
                return View(model);
            }
            return RedirectToAction("Index");
        }
        private async Task<List<string>> GetUserRoles(UserSystemIdentity user)
        {
            return new List<string>(await _userManager.GetRolesAsync(user));
        }
    }
}
