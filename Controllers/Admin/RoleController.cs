using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using mvc.Models.Authorize;
using mvc.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace mvc.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/[controller]/[action]")]
    public class RoleController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var rolesViewModel = new List<RoleViewModel>();
            
            foreach (var role in roles)
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                rolesViewModel.Add(new RoleViewModel
                {
                    Id = role.Id,
                    RoleName = role.Name,
                    UsersCount = usersInRole.Count
                });
            }
            
            return View(rolesViewModel);
        }

        public IActionResult Create()
        {
            return View(new RoleFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RoleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if role exists
            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (roleExists)
            {
                ModelState.AddModelError("RoleName", "Role with this name already exists");
                return View(model);
            }

            // Create role
            var role = new IdentityRole(model.RoleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Role '{model.RoleName}' created successfully";
                return RedirectToAction(nameof(Index));
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Role not found";
                return RedirectToAction(nameof(Index));
            }

            var model = new RoleFormViewModel
            {
                Id = role.Id,
                RoleName = role.Name,
                // Check if it's a system role (Admin, User)
                IsSystemRole = (role.Name == "Admin" || role.Name == "User") 
            };
            
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RoleFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.IsSystemRole)
            {
                TempData["Error"] = "System roles cannot be modified";
                return RedirectToAction(nameof(Index));
            }

            var role = await _roleManager.FindByIdAsync(model.Id);
            if (role == null)
            {
                TempData["Error"] = "Role not found";
                return RedirectToAction(nameof(Index));
            }

            // Check if trying to update to an existing role name
            if (role.Name != model.RoleName)
            {
                var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
                if (roleExists)
                {
                    ModelState.AddModelError("RoleName", "Role with this name already exists");
                    return View(model);
                }
            }

            role.Name = model.RoleName;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Role '{model.RoleName}' updated successfully";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Role not found";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deletion of system roles
            if (role.Name == "Admin" || role.Name == "User")
            {
                TempData["Error"] = "System roles cannot be deleted";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);
            
            if (result.Succeeded)
            {
                TempData["Success"] = $"Role '{role.Name}' deleted successfully";
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    TempData["Error"] = error.Description;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ManageUsers(string roleId)
        {
            if (string.IsNullOrEmpty(roleId))
            {
                return NotFound();
            }

            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                TempData["Error"] = "Role not found";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.RoleId = roleId;
            ViewBag.RoleName = role.Name;

            var users = await _userManager.Users.ToListAsync();
            var userRoleViewModels = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    IsSelected = await _userManager.IsInRoleAsync(user, role.Name)
                };

                userRoleViewModels.Add(userRoleViewModel);
            }

            return View(userRoleViewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUsers(List<UserRoleViewModel> model, string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                TempData["Error"] = "Role not found";
                return RedirectToAction(nameof(Index));
            }

            int addedCount = 0;
            int removedCount = 0;

            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);
                
                if (user == null)
                {
                    continue;
                }

                var userIsInRole = await _userManager.IsInRoleAsync(user, role.Name);
                
                if (model[i].IsSelected && !userIsInRole)
                {
                    await _userManager.AddToRoleAsync(user, role.Name);
                    addedCount++;
                }
                else if (!model[i].IsSelected && userIsInRole)
                {
                    // Don't allow removing Admin role from last admin
                    if (role.Name == "Admin")
                    {
                        var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                        if (adminUsers.Count == 1 && adminUsers.First().Id == user.Id)
                        {
                            TempData["Error"] = "Cannot remove the last admin user from Admin role";
                            return RedirectToAction("ManageUsers", new { roleId });
                        }
                    }
                    
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                    removedCount++;
                }
            }

            TempData["Success"] = $"Updated users in role '{role.Name}': {addedCount} users added, {removedCount} users removed";
            return RedirectToAction("ManageUsers", new { roleId });
        }
    }
}
