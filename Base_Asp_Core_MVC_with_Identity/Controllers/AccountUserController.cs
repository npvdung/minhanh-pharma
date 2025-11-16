using Base_Asp_Core_MVC_with_Identity.CommonFile.Enum;
using Base_Asp_Core_MVC_with_Identity.Models;
using Base_Asp_Core_MVC_with_Identity.Models.View;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace Base_Asp_Core_MVC_with_Identity.Controllers
{
    public class AccountUserController : Controller
    {
        private UserManager<UserSystemIdentity> userManager;
        private IPasswordHasher<UserSystemIdentity> passwordHasher;
        public AccountUserController(UserManager<UserSystemIdentity> usrMgr, IPasswordHasher<UserSystemIdentity> passwordHash)
        {
            userManager = usrMgr;
            passwordHasher = passwordHash;
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View(userManager.Users);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            AccountUserView category = new AccountUserView();
            return View(category);
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(AccountUserView user)
        {
            if (ModelState.IsValid)
            {
                var defaultUser = CreateUser();

                defaultUser.Email = user.Email;
                defaultUser.LastName = user.FirstName;
                defaultUser.FirstName = user.LastName;
                defaultUser.PhoneNumber = user.PhoneNumber;
                defaultUser.EmailConfirmed = true;
                defaultUser.PhoneNumberConfirmed = true;
                defaultUser.ProfilePicture = new byte[0];

                if (userManager.Users.All(u => u.Id != defaultUser.Id))
                {
                    var userEntity = await userManager.FindByEmailAsync(defaultUser.Email);
                    if (userEntity == null)
                    {

                        await userManager.SetUserNameAsync(defaultUser, defaultUser.Email);
                        await userManager.SetEmailAsync(defaultUser, defaultUser.Email);

                        var result = await userManager.CreateAsync(defaultUser, user.Password);
                        if (result.Succeeded)
                        {
                            TempData["ResultOk"] = "Thêm mới thông tin tài khoản thành công !";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            foreach (IdentityError error in result.Errors)
                                ModelState.AddModelError("", error.Description);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email đã được đăng ký");
                    }
                }
            }
            return View(user);

        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(string id)
        {
            UserSystemIdentity user = await userManager.FindByIdAsync(id);
            if (user != null)
                return View(user);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(UserSystemIdentity userEdit, string password)
        {
            UserSystemIdentity user = await userManager.FindByIdAsync(userEdit.Id);
            if (user != null)
            {
                if (!string.IsNullOrEmpty(user.Email))
                    user.Email = user.Email;
                else
                    ModelState.AddModelError("", "Email không được để trống");

                if (!string.IsNullOrEmpty(password))
                    user.PasswordHash = passwordHasher.HashPassword(user, password);
                else
                    ModelState.AddModelError("", "Password không được để trống");

                if (!string.IsNullOrEmpty(user.Email) && !string.IsNullOrEmpty(password))
                {
                    user.PhoneNumber = userEdit.PhoneNumber;
                    user.FirstName = userEdit.FirstName;
                    user.LastName = userEdit.LastName;
                    user.ProfilePicture = new byte[0];
                    IdentityResult result = await userManager.UpdateAsync(user);
                    if (result.Succeeded)
                    {
                        TempData["ResultOk"] = "Cập nhật thông tin tài khoản thành công !";
                        return RedirectToAction("Index");
                    }
                    else
                        Errors(result);
                }
            }
            else
                ModelState.AddModelError("", "Không tìm thấy tài khoản");
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            UserSystemIdentity user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                IdentityResult result = await userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    TempData["ResultOk"] = "Xoá tài khoản thành công !";
                    return RedirectToAction("Index");
                }
                else
                    Errors(result);
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View("Index", userManager.Users);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }
        private static UserSystemIdentity CreateUser()
        {
            try
            {
                return Activator.CreateInstance<UserSystemIdentity>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(UserSystemIdentity)}'. " +
                    $"Ensure that '{nameof(UserSystemIdentity)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }
    }
}
