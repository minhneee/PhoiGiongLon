using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;
using SwineBreedingManager.Services;

namespace SwineBreedingManager.Controllers
{
    [Authorize]
    public class GenealogyController(ApplicationDbContext context, IGenealogyService genealogyService) : Controller
    {
        public async Task<IActionResult> Index(string? searchString)
        {
            var query = context.Pigs.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.TagNumber.Contains(searchString) || 
                                         (p.Name != null && p.Name.Contains(searchString)) ||
                                         (p.Breed != null && p.Breed.Contains(searchString)));
            }

            var pigs = await query.OrderBy(p => p.TagNumber).ToListAsync();
            ViewBag.CurrentSearch = searchString;
            return View(pigs);
        }

        public async Task<IActionResult> Tree(int id)
        {
            var pig = await context.Pigs
                .Include(p => p.Father)
                .Include(p => p.Mother)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pig == null) return NotFound();

            var siblings = await genealogyService.GetSiblings(id);
            var children = await genealogyService.GetChildren(id);

            ViewBag.Siblings = siblings;
            ViewBag.Children = children;

            return View(pig);
        }
    }
}
