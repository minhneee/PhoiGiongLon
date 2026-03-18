using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using System.Security.Claims;

namespace SwineBreedingManager.Filters
{
    public class DynamicPermissionFilter(ApplicationDbContext _context) : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var user = context.HttpContext.User;

            // Only check permissions for authenticated users with CÔNG NHÂN role
            if (user.Identity?.IsAuthenticated == true && user.IsInRole("CÔNG NHÂN"))
            {
                var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
                var controllerName = context.RouteData.Values["controller"]?.ToString();
                
                if (!string.IsNullOrEmpty(controllerName) && !string.IsNullOrEmpty(userId))
                {
                    // Check if this specific user has restricted access to this controller
                    var permission = await _context.PagePermissions
                        .FirstOrDefaultAsync(p => p.ControllerName == controllerName && p.UserId == userId);

                    if (permission != null && !permission.IsAllowed)
                    {
                        // Access denied for this page, redirect to 404 as requested
                        context.Result = new RedirectToActionResult("NotFound", "Home", null);
                        return;
                    }
                }
            }

            await next();
        }
    }
}
