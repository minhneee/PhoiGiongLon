using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Controllers
{
    [Authorize]
    public class DashboardController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var totalPigs = await context.Pigs.CountAsync();
            var boars = await context.Pigs.CountAsync(p => p.Gender == PigGender.Boar);
            var sows = await context.Pigs.CountAsync(p => p.Gender == PigGender.Sow);
            var activePregnancies = await context.BreedingRecords.CountAsync(b => !b.ActualBirthDate.HasValue);
            
            var upcomingBirths = await context.BreedingRecords
                .Include(b => b.Sow)
                .Where(b => !b.ActualBirthDate.HasValue)
                .OrderBy(b => b.EstimatedBirthDate)
                .Take(5)
                .ToListAsync();

            var recentLitterSizes = await context.BreedingRecords
                .Where(b => b.ActualBirthDate.HasValue)
                .OrderByDescending(b => b.ActualBirthDate)
                .Take(12)
                .Select(b => (double)(b.LitterSizeAlive ?? 0))
                .ToListAsync();

            var pens = await context.Pens.Include(p => p.Pigs).OrderBy(p => p.Name).ToListAsync();
            // Tự động tạo 8 chuồng nếu hệ thống chưa có đủ 8 chuồng cơ bản (mỗi chuồng 100 con)
            if (pens.Count < 8 && !pens.Any(p => p.Name.StartsWith("Chuồng Số")))
            {
                for (int i = 1; i <= 8; i++)
                {
                    context.Pens.Add(new Pen { Name = $"Chuồng Số {i}", Capacity = 100, Description = "Khu vực nuôi chuẩn" });
                }
                await context.SaveChangesAsync();
                pens = await context.Pens.Include(p => p.Pigs).OrderBy(p => p.Name).ToListAsync();
            }

            ViewBag.Pens = pens;

            ViewBag.TotalPigs = totalPigs;
            ViewBag.Boars = boars;
            ViewBag.Sows = sows;
            ViewBag.ActivePregnancies = activePregnancies;
            ViewBag.UpcomingBirths = upcomingBirths;
            ViewBag.AvgLitterSize = recentLitterSizes.Any() ? Math.Round(recentLitterSizes.Average(), 1) : 0;

            return View();
        }
    }
}
