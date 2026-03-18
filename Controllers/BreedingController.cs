using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;
using SwineBreedingManager.Services;

namespace SwineBreedingManager.Controllers
{
    [Authorize]
    public class BreedingController(ApplicationDbContext context, IBreedingService breedingService, IGenealogyService genealogyService) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var records = await context.BreedingRecords
                .Include(b => b.Boar)
                .Include(b => b.Sow)
                .OrderByDescending(b => b.BreedingDate)
                .ToListAsync();
            return View(records);
        }

        public async Task<IActionResult> Expected(int? penId, string? searchString)
        {
            var query = context.BreedingRecords
                .Include(b => b.Boar)
                .Include(b => b.Sow)
                    .ThenInclude(s => s!.Pen)
                .Where(b => b.ActualBirthDate == null) // Only upcoming
                .AsQueryable();

            if (penId.HasValue)
            {
                query = query.Where(b => b.Sow != null && b.Sow.PenId == penId.Value);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(b => (b.Sow != null && b.Sow.TagNumber.Contains(searchString)) || 
                                         (b.Boar != null && b.Boar.TagNumber.Contains(searchString)));
            }

            ViewBag.CurrentPenId = penId;
            ViewBag.CurrentSearch = searchString;
            ViewBag.Pens = await context.Pens.ToListAsync();

            var records = await query
                .OrderBy(b => b.EstimatedBirthDate)
                .ToListAsync();

            return View(records);
        }

        public IActionResult Create()
        {
            var boars = context.Pigs.Where(p => p.Gender == PigGender.Boar && p.Status == PigStatus.Active).ToList();
            var sows = context.Pigs.Where(p => p.Gender == PigGender.Sow && p.Status == PigStatus.Active).ToList();

            ViewBag.BoarId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(boars, "Id", "FullDisplayInfo");
            ViewBag.SowId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(sows, "Id", "FullDisplayInfo");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BreedingRecord record)
        {
            var (isInbred, commonAncestors) = await genealogyService.ValidateInbreeding(record.BoarId, record.SowId);
            
            if (isInbred)
            {
                ModelState.AddModelError("", "Cảnh báo trùng huyết! Tổ tiên chung: " + string.Join(", ", commonAncestors));
            }

            if (ModelState.IsValid)
            {
                record.EstimatedBirthDate = breedingService.CalculateExpectedBirthDate(record.BreedingDate);
                context.Add(record);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.BoarId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                context.Pigs.Where(p => p.Gender == PigGender.Boar && p.Status == PigStatus.Active), "Id", "FullDisplayInfo", record.BoarId);
            ViewBag.SowId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(
                context.Pigs.Where(p => p.Gender == PigGender.Sow && p.Status == PigStatus.Active), "Id", "FullDisplayInfo", record.SowId);
            return View(record);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var record = await context.BreedingRecords.FindAsync(id);
            if (record == null) return NotFound();

            ViewBag.BoarId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(context.Pigs.Where(p => p.Gender == PigGender.Boar), "Id", "FullDisplayInfo", record.BoarId);
            ViewBag.SowId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(context.Pigs.Where(p => p.Gender == PigGender.Sow), "Id", "FullDisplayInfo", record.SowId);
            return View(record);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BreedingRecord record)
        {
            if (id != record.Id) return NotFound();

            if (ModelState.IsValid)
            {
                if (record.ActualBirthDate.HasValue && !record.WeaningDate.HasValue)
                {
                    record.WeaningDate = breedingService.CalculateWeaningDate(record.ActualBirthDate.Value);
                }

                context.Update(record);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(record);
        }

        public async Task<IActionResult> Farrow(int? id)
        {
            if (id == null) return NotFound();
            var record = await context.BreedingRecords
                .Include(b => b.Boar)
                .Include(b => b.Sow)
                .FirstOrDefaultAsync(b => b.Id == id);
            if (record == null) return NotFound();

            var viewModel = new FarrowingViewModel
            {
                BreedingRecordId = record.Id,
                BreedingRecord = record,
                ActualBirthDate = record.ActualBirthDate ?? DateTime.Now,
                LitterSizeAlive = record.LitterSizeAlive ?? 0,
                LitterSizeDead = record.LitterSizeDead ?? 0,
                Notes = record.Notes
            };

            ViewBag.Pens = await context.Pens.ToListAsync();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Farrow(int id, FarrowingViewModel viewModel)
        {
            if (id != viewModel.BreedingRecordId) return NotFound();

            var existing = await context.BreedingRecords
                .Include(b => b.Boar)
                .Include(b => b.Sow)
                .FirstOrDefaultAsync(b => b.Id == id);
                
            if (existing == null) return NotFound();

            if (ModelState.IsValid)
            {
                existing.ActualBirthDate = viewModel.ActualBirthDate;
                existing.LitterSizeAlive = viewModel.LitterSizeAlive;
                existing.LitterSizeDead = viewModel.LitterSizeDead;
                existing.Notes = viewModel.Notes;
                existing.WeaningDate = breedingService.CalculateWeaningDate(viewModel.ActualBirthDate);

                // Auto-create piglets
                if (viewModel.Piglets != null && viewModel.Piglets.Count > 0)
                {
                    // 1. Check for duplicates within the current list
                    var duplicateNewTags = viewModel.Piglets
                        .GroupBy(p => p.TagNumber)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key)
                        .ToList();

                    if (duplicateNewTags.Any())
                    {
                        ModelState.AddModelError("", $"Danh sách heo con có số tai bị trùng lặp: {string.Join(", ", duplicateNewTags)}");
                    }

                    // 2. Check for duplicates in the database
                    foreach (var pigletData in viewModel.Piglets)
                    {
                        if (context.Pigs.Any(p => p.TagNumber == pigletData.TagNumber))
                        {
                            ModelState.AddModelError("", $"Số tai '{pigletData.TagNumber}' đã tồn tại trong hệ thống.");
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        foreach (var pigletData in viewModel.Piglets)
                        {
                            var newPig = new Pig
                            {
                                TagNumber = pigletData.TagNumber,
                                Gender = pigletData.Gender,
                                Breed = string.IsNullOrEmpty(pigletData.Breed) ? existing.Sow?.Breed : pigletData.Breed,
                                BirthDate = viewModel.ActualBirthDate,
                                FatherId = existing.BoarId,
                                MotherId = existing.SowId,
                                PenId = pigletData.PenId > 0 ? pigletData.PenId : null,
                                Weight = pigletData.Weight,
                                Status = PigStatus.Active
                            };
                            context.Pigs.Add(newPig);
                        }
                    }
                    else
                    {
                        viewModel.BreedingRecord = existing;
                        ViewBag.Pens = await context.Pens.ToListAsync();
                        return View(viewModel);
                    }
                }

                context.Update(existing);
                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            viewModel.BreedingRecord = existing;
            ViewBag.Pens = await context.Pens.ToListAsync();
            return View(viewModel);
        }
        [HttpGet]
        public async Task<JsonResult> CheckInbreeding(int boarId, int sowId)
        {
            var (isInbred, commonAncestors) = await genealogyService.ValidateInbreeding(boarId, sowId);
            return Json(new { isInbred, commonAncestors });
        }
    }
}
