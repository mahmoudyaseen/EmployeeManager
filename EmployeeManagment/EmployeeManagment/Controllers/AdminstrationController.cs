using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EmployeeManagment
{
    //[Authorize(Policy = "AdminRolePolicy")]
    public class AdminstrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdminstrationController> logger;

        public AdminstrationController(RoleManager<IdentityRole> roleManager,
                                       UserManager<ApplicationUser> userManager,
                                       ILogger<AdminstrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        #region Manage User

        #region List Users

        [HttpGet]
        public IActionResult ListUsers()
        {
            return View(userManager.Users);
        }

        #endregion

        #region Edit User

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} is not fount";
                return View("NotFound");
            }

            var rolesForUser = await userManager.GetRolesAsync(user);
            var claimsForUser = await userManager.GetClaimsAsync(user);

            var model = new EditUserViewModel()
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                City = user.City,
                Roles = rolesForUser.ToList(),
                Claims = claimsForUser.Select(c => c.Type + ":" + c.Value).ToList()
            };

            return View(model);
        }     

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(model.Id);

                if (user == null)
                {
                    ViewBag.ErrorMessage = $"User with Id = {model.Id} is not fount";
                    return View("NotFound");
                }

                user.UserName = model.UserName;
                user.Email = model.Email;
                user.City = model.City;

                var res = await userManager.UpdateAsync(user);

                if (res.Succeeded)
                    return RedirectToAction("ListUsers", "Adminstration");

                foreach (var error in res.Errors)
                    ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        #endregion

        #region Delete User

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {id} is not fount";
                return View("NotFound");
            }

            var res = await userManager.DeleteAsync(user);

            foreach (var error in res.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return RedirectToAction("ListUsers");
        }

        #endregion

        #region Edit User Roles

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId)
        {
            ViewBag.userId = userId;

            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} is not fount";
                return View("NotFound");
            }

            var model = new List<UserRolesViewModel>();

            foreach (var role in roleManager.Roles.ToList())
            {
                model.Add(new UserRolesViewModel()
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsSelected = await userManager.IsInRoleAsync(user, role.Name)
                });
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRoles(string userId, List<UserRolesViewModel> model)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} is not fount";
                return View("NotFound");
            }

            var roles = await userManager.GetRolesAsync(user);
            var result = await userManager.RemoveFromRolesAsync(user, roles);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can't remove user existing roles");
                return View(model);
            }

            result = await userManager.AddToRolesAsync(user, model.Where(r => r.IsSelected).Select(r => r.RoleName));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can't add selected roles to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { id = userId });
        }

        #endregion

        #region Edit User Claims

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {userId} is not fount";
                return View("NotFound");
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new UserClaimsViewModel()
            {
                UserId = userId
            };

            foreach (var claim in ClaimsStore.AllClaims)
            {
                model.Claims.Add(new UserClaim()
                {
                    ClaimType = claim.Type,
                    IsSelected = existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true"),
                });
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserClaims(UserClaimsViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ViewBag.ErrorMessage = $"User with Id = {model.UserId} is not fount";
                return View("NotFound");
            }

            var claims = await userManager.GetClaimsAsync(user);
            var result = await userManager.RemoveClaimsAsync(user, claims);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can't remove user existing Claims");
                return View(model);
            }

            result = await userManager.AddClaimsAsync(user, model.Claims.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true" : "false")));

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Can't add selected claims to user");
                return View(model);
            }

            return RedirectToAction("EditUser", new { id = model.UserId });
        }

        #endregion

        #endregion

        #region Manage Roles

        #region List Roles

        [HttpGet]
        public IActionResult ListRoles()
        {
            return View(roleManager.Roles);
        }

        #endregion

        #region Create Role

        [HttpGet]
        [Authorize(Policy = "CreateRolePolicy")]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CreateRolePolicy")]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = new IdentityRole()
                {
                    Name = model.RoleName
                };

                var res = await roleManager.CreateAsync(role);

                if (res.Succeeded)
                    return RedirectToAction("ListRoles", "Adminstration");

                foreach (var error in res.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        #endregion

        #region Edit role

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} is not fount";
                return View("NotFound");
            }

            var users = await userManager.GetUsersInRoleAsync(role.Name);

            var model = new EditRoleViewModel()
            {
                Id = role.Id,
                RoleName = role.Name,
                Users = users.Select(a => a.UserName).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(EditRoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await roleManager.FindByIdAsync(model.Id);

                if (role == null)
                {
                    ViewBag.ErrorMessage = $"Role with Id = {model.Id} is not fount";
                    return View("NotFound");
                }

                role.Name = model.RoleName;

                var res = await roleManager.UpdateAsync(role);

                if (res.Succeeded)
                    return RedirectToAction("ListRoles", "Adminstration");

                foreach (var error in res.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        #endregion

        #region Delete Role

        [HttpPost]
        [Authorize(Policy = "DeleteRolePolicy")]
        public async Task<IActionResult> DeleteRole(string id)
        {
            var role = await roleManager.FindByIdAsync(id);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} is not fount";
                return View("NotFound");
            }

            try
            {
                // if user delete role that has users assign to it EFCore will throw exception
                var res = await roleManager.DeleteAsync(role);

                foreach (var error in res.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return RedirectToAction("Listroles");
            }
            catch (DbUpdateException ex)
            {
                logger.LogError($"Error deleting role {ex}");

                ViewBag.ErrorTitle = $"{role.Name} role is in use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users in this role. " +
                    $"If you want to delete this role, please remove the users from the role and then try to delete";
                return View("Error");
            }
        }

        #endregion

        #region Edit Users In Role

        [HttpGet]
        public async Task<IActionResult> EditUsersInRole(string roleId)
        {
            ViewBag.roleId = roleId;

            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} is not fount";
                return View("NotFound");
            }

            var model = new List<UserRoleViewModel>();

            foreach (var user in userManager.Users.ToList())
            {
                model.Add(new UserRoleViewModel()
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    IsSelected = await userManager.IsInRoleAsync(user, role.Name)
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUsersInRole(string roleId, List<UserRoleViewModel> model)
        {
            var role = await roleManager.FindByIdAsync(roleId);

            if (role == null)
            {
                ViewBag.ErrorMessage = $"Role with Id = {roleId} is not fount";
                return View("NotFound");
            }

            foreach (var userRole in model)
            {
                var user = await userManager.FindByIdAsync(userRole.UserId);
                IdentityResult result = null;

                if (userRole.IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
                    result = await userManager.AddToRoleAsync(user, role.Name);

                if (!userRole.IsSelected && await userManager.IsInRoleAsync(user, role.Name))
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);

                if (result != null && !result.Succeeded)
                {
                    foreach (var error in result.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);
                    return View(model);
                }
            }
            return RedirectToAction("EditRole", new { id = roleId });
        }

        #endregion

        #endregion

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
