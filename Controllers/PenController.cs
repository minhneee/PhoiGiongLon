using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Controllers
{
    [Authorize]
    public class PenController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var pens = await context.Pens
                .Include(p => p.Pigs)
                .ToListAsync();
            return View(pens);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Capacity,Description")] Pen pen)
        {
            if (ModelState.IsValid)
            {
                context.Add(pen);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pen);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pen = await context.Pens.FindAsync(id);
            if (pen == null) return NotFound();
            return View(pen);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Capacity,Description")] Pen pen)
        {
            if (id != pen.Id) return NotFound();

            if (ModelState.IsValid)
            {
                context.Update(pen);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pen);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pen = await context.Pens
                .Include(p => p.Pigs)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (pen == null) return NotFound();

            return View(pen);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var pen = await context.Pens
                .Include(p => p.Pigs)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (pen == null) return NotFound();

            return View(pen);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pen = await context.Pens.Include(p => p.Pigs).FirstOrDefaultAsync(p => p.Id == id);
            if (pen != null)
            {
                // Disconnect pigs before deleting pen
                if (pen.Pigs != null)
                {
                    foreach(var pig in pen.Pigs)
                    {
                        pig.PenId = null;
                    }
                }
                context.Pens.Remove(pen);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Organize(int? penId, string? status)
        {
            var pens = await context.Pens.Include(p => p.Pigs).ToListAsync();
            var pigsQuery = context.Pigs.Where(p => p.Status == PigStatus.Active).AsQueryable();

            if (status == "unassigned")
            {
                pigsQuery = pigsQuery.Where(p => p.PenId == null);
            }
            else if (status == "assigned")
            {
                pigsQuery = pigsQuery.Where(p => p.PenId != null);
            }

            if (penId.HasValue)
            {
                pigsQuery = pigsQuery.Where(p => p.PenId == penId.Value);
            }

            var pigs = await pigsQuery.OrderBy(p => p.TagNumber).ToListAsync();

            ViewBag.CurrentPenId = penId;
            ViewBag.CurrentStatus = status;
            ViewBag.Pens = pens;

            return View(pigs);
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePigPen(int pigId, int? penId)
        {
            var pig = await context.Pigs.FindAsync(pigId);
            if (pig == null) return NotFound();

            pig.PenId = penId == 0 ? null : penId;
            await context.SaveChangesAsync();

            return Ok();
        }
    }
}
