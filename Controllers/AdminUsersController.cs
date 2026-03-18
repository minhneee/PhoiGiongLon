using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Models;
using SwineBreedingManager.Data;

namespace SwineBreedingManager.Controllers
{
    [Authorize(Roles = "CHỦ TRẠI")]
    public class AdminUsersController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var users = await userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "",
                    UserName = user.UserName ?? "",
                    Role = roles.FirstOrDefault() ?? ""
                });
            }

            return View(userViewModels);
        }

        public async Task<IActionResult> Permissions(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var permissions = await context.PagePermissions
                .Where(p => p.UserId == userId)
                .ToListAsync();

            // If no individual permissions exist, seed them from defaults
            if (!permissions.Any())
            {
                var defaults = new List<PagePermission>
                {
                    new PagePermission { PageName = "Danh Sách Đàn", DisplayName = "Quản lý heo", ControllerName = "Pigs", IsAllowed = true, UserId = userId },
                    new PagePermission { PageName = "Phối Giống & Đẻ", DisplayName = "Quản lý sinh sản", ControllerName = "Breeding", IsAllowed = true, UserId = userId },
                    new PagePermission { PageName = "Sơ Đồ Chuồng", DisplayName = "Quản lý chuồng trại", ControllerName = "Pen", IsAllowed = true, UserId = userId },
                    new PagePermission { PageName = "Kinh Doanh", DisplayName = "Quản lý bán hàng", ControllerName = "Sales", IsAllowed = true, UserId = userId },
                    new PagePermission { PageName = "Báo Cáo", DisplayName = "Báo cáo thống kê", ControllerName = "Report", IsAllowed = true, UserId = userId },
                    new PagePermission { PageName = "Gia Phả", DisplayName = "Xem gia phả", ControllerName = "Genealogy", IsAllowed = true, UserId = userId }
                };
                context.PagePermissions.AddRange(defaults);
                await context.SaveChangesAsync();
                permissions = defaults;
            }

            ViewBag.TargetUser = user.Email;
            ViewBag.TargetUserId = userId;
            return View(permissions);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePermission(int id, bool isAllowed, string userId)
        {
            var permission = await context.PagePermissions.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
            if (permission != null)
            {
                permission.IsAllowed = isAllowed;
                await context.SaveChangesAsync();
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await userManager.CreateAsync(user, model.Password!);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await userManager.AddToRoleAsync(user, model.Role);
                    }
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await userManager.GetRolesAsync(user);
            var model = new UserViewModel
            {
                Id = user.Id,
                Email = user.Email ?? "",
                UserName = user.UserName ?? "",
                Role = roles.FirstOrDefault() ?? ""
            };

            ViewBag.Roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UserViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(id);
                if (user == null) return NotFound();

                user.Email = model.Email;
                user.UserName = model.Email;

                var result = await userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Update Role
                    var currentRoles = await userManager.GetRolesAsync(user);
                    await userManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!string.IsNullOrEmpty(model.Role))
                    {
                        await userManager.AddToRoleAsync(user, model.Role);
                    }

                    // Update Password if provided
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await userManager.GeneratePasswordResetTokenAsync(user);
                        await userManager.ResetPasswordAsync(user, token, model.Password);
                    }

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Prevent deleting self
                if (user.UserName == User.Identity?.Name)
                {
                    return BadRequest("Bạn không thể tự xóa chính mình.");
                }

                await userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
