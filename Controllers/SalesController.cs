using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Controllers
{
    [Authorize]
    public class SalesController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var sales = await context.SaleRecords
                .Include(s => s.Pig)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
            return View(sales);
        }

        public async Task<IActionResult> Create()
        {
            var activePigs = await context.Pigs
                .Where(p => p.Status == PigStatus.Active)
                .ToListAsync();
            return View(activePigs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int[] selectedPigIds, decimal pricePerKg, string? customerName, string? notes)
        {
            if (selectedPigIds == null || selectedPigIds.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một con heo để bán.");
                return View(await context.Pigs.Where(p => p.Status == PigStatus.Active).ToListAsync());
            }

            foreach (var id in selectedPigIds)
            {
                var pig = await context.Pigs.FindAsync(id);
                if (pig != null)
                {
                    decimal weight = pig.Weight ?? 0;
                    var sale = new SaleRecord
                    {
                        PigId = pig.Id,
                        SaleDate = DateTime.Now,
                        Weight = weight,
                        Price = pricePerKg * weight,
                        CustomerName = customerName,
                        Notes = notes
                    };

                    pig.Status = PigStatus.Sold;
                    context.SaleRecords.Add(sale);
                    context.Update(pig);
                }
            }

            await context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
