using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Controllers
{
    [Authorize]
    public class PigsController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> Index(PigStatus? status, int? penId, string? searchString)
        {
            var query = context.Pigs
                .Include(p => p.Father)
                .Include(p => p.Mother)
                .Include(p => p.Pen)
                .Include(p => p.SaleRecord)
                .AsQueryable();

            // Default behavior: hides Sold pigs if no status filter is specified
            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }
            else if (string.IsNullOrEmpty(searchString) && !penId.HasValue)
            {
                // Only default to Active if no other filters are present
                query = query.Where(p => p.Status == PigStatus.Active);
            }

            if (penId.HasValue)
            {
                query = query.Where(p => p.PenId == penId.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.TagNumber.Contains(searchString) || (p.Name != null && p.Name.Contains(searchString)));
            }

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPenId = penId;
            ViewBag.CurrentSearch = searchString;
            ViewBag.Pens = await context.Pens.ToListAsync();

            var pigs = await query
                .OrderBy(p => p.PenId == null) // Show pigs with pens first
                .ThenBy(p => p.Pen!.Name)
                .ThenBy(p => p.TagNumber)
                .ToListAsync();
            return View(pigs);
        }

        public IActionResult Create()
        {
            var boars = context.Pigs.Where(p => p.Gender == PigGender.Boar && p.Status == PigStatus.Active).ToList();
            var sows = context.Pigs.Where(p => p.Gender == PigGender.Sow && p.Status == PigStatus.Active).ToList();

            ViewBag.FatherId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(boars, "Id", "FullDisplayInfo");
            ViewBag.MotherId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(sows, "Id", "FullDisplayInfo");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pig pig)
        {
            if (context.Pigs.Any(p => p.TagNumber == pig.TagNumber))
            {
                ModelState.AddModelError("TagNumber", "Số tai này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                context.Add(pig);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.FatherId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                context.Pigs.Where(p => p.Gender == PigGender.Boar && p.Status == PigStatus.Active), "Id", "FullDisplayInfo", pig.FatherId);
            ViewBag.MotherId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                context.Pigs.Where(p => p.Gender == PigGender.Sow && p.Status == PigStatus.Active), "Id", "FullDisplayInfo", pig.MotherId);
            return View(pig);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var pig = await context.Pigs
                .Include(p => p.Father)
                .Include(p => p.Mother)
                .Include(p => p.BreedingRecordsAsBoar!)
                    .ThenInclude(b => b.Sow)
                .Include(p => p.BreedingRecordsAsSow!)
                    .ThenInclude(b => b.Boar)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (pig == null) return NotFound();

            return View(pig);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var pig = await context.Pigs.FindAsync(id);
            if (pig == null) return NotFound();

            ViewBag.FatherId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(context.Pigs.Where(p => p.Gender == PigGender.Boar && p.Id != id), "Id", "FullDisplayInfo", pig.FatherId);
            ViewBag.MotherId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(context.Pigs.Where(p => p.Gender == PigGender.Sow && p.Id != id), "Id", "FullDisplayInfo", pig.MotherId);
            return View(pig);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Pig pig)
        {
            if (id != pig.Id) return NotFound();

            if (context.Pigs.Any(p => p.TagNumber == pig.TagNumber && p.Id != id))
            {
                ModelState.AddModelError("TagNumber", "Số tai này đã tồn tại trong hệ thống.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    context.Update(pig);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PigExists(pig.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(pig);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pig = await context.Pigs.FindAsync(id);
            if (pig != null)
            {
                // 1. Remove associated SaleRecords
                var saleRecords = await context.SaleRecords.Where(s => s.PigId == id).ToListAsync();
                context.SaleRecords.RemoveRange(saleRecords);

                // 2. Remove associated health records
                var healthRecords = await context.PigHealthRecords.Where(h => h.PigId == id).ToListAsync();
                context.PigHealthRecords.RemoveRange(healthRecords);

                // 3. Remove associated BreedingRecords where this pig is Boar or Sow
                var breedingRecords = await context.BreedingRecords.Where(b => b.BoarId == id || b.SowId == id).ToListAsync();
                context.BreedingRecords.RemoveRange(breedingRecords);

                // 4. Update children (set parent IDs to null)
                var children = await context.Pigs.Where(p => p.FatherId == id || p.MotherId == id).ToListAsync();
                foreach (var child in children)
                {
                    if (child.FatherId == id) child.FatherId = null;
                    if (child.MotherId == id) child.MotherId = null;
                }

                // 5. Remove the pig itself
                context.Pigs.Remove(pig);
                await context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PigExists(int id)
        {
            return context.Pigs.Any(e => e.Id == id);
        }
    }
}
