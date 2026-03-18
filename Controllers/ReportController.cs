using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Controllers
{
    [Authorize]
    public class ReportController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var totalPigs = await context.Pigs.CountAsync();
            var activePigs = await context.Pigs.CountAsync(p => p.Status == PigStatus.Active);
            var soldPigs = await context.Pigs.CountAsync(p => p.Status == PigStatus.Sold);

            // Sales by Month (Last 6 months)
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var salesData = new List<decimal>();
            var labels = new List<string>();

            foreach (var month in last6Months)
            {
                var revenue = await context.SaleRecords
                    .Where(s => s.SaleDate.Month == month.Month && s.SaleDate.Year == month.Year)
                    .SumAsync(s => s.Price);
                salesData.Add(revenue);
                labels.Add(month.ToString("MM/yyyy"));
            }

            // Farrowing Stats
            var farrowingData = await context.BreedingRecords
                .Where(b => b.ActualBirthDate != null)
                .Select(b => new { b.LitterSizeAlive, b.LitterSizeDead })
                .ToListAsync();

            ViewBag.TotalPigs = totalPigs;
            ViewBag.ActivePigs = activePigs;
            ViewBag.SoldPigs = soldPigs;
            ViewBag.SalesLabels = labels;
            ViewBag.SalesData = salesData;
            ViewBag.FarrowingAlive = farrowingData.Sum(f => f.LitterSizeAlive ?? 0);
            ViewBag.FarrowingDead = farrowingData.Sum(f => f.LitterSizeDead ?? 0);

            // Top Heaviest Pigs (Currently Active)
            var topPigs = await context.Pigs
                .Where(p => p.Status == PigStatus.Active && p.Weight != null)
                .OrderByDescending(p => p.Weight)
                .Take(5)
                .ToListAsync();
            ViewBag.TopPigs = topPigs;

            // Top Farrowing Sows (Most live piglets total)
            var topSows = await context.Pigs
                .Where(p => p.Gender == PigGender.Sow)
                .Select(p => new
                {
                    Pig = p,
                    TotalPiglets = p.BreedingRecordsAsSow!.Sum(b => b.LitterSizeAlive ?? 0)
                })
                .OrderByDescending(x => x.TotalPiglets)
                .Take(5)
                .ToListAsync();
            
            ViewBag.TopSows = topSows.Select(x => new { x.Pig.TagNumber, x.Pig.Breed, x.TotalPiglets }).ToList();

            // Oldest Pigs (Active)
            var oldestPigs = await context.Pigs
                .Where(p => p.Status == PigStatus.Active)
                .OrderBy(p => p.BirthDate)
                .Take(5)
                .ToListAsync();
            ViewBag.OldestPigs = oldestPigs;

            return View();
        }
    }
}
