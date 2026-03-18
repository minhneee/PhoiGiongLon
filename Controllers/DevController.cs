using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SwineBreedingManager.Controllers
{
    public class DevController(ApplicationDbContext context) : Controller
    {
        public async Task<IActionResult> SeedData()
        {
            // 1. Cleanup existing records to avoid duplicates and FK issues
            // Order of deletion matters for FK constraints
            context.SaleRecords.RemoveRange(context.SaleRecords);
            context.BreedingRecords.RemoveRange(context.BreedingRecords);
            context.Pigs.RemoveRange(context.Pigs);
            await context.SaveChangesAsync();

            var random = new Random();
            var breeds = new[] { "Yorkshire", "Landrace", "Duroc", "Pietrain" };
            
            // Ensure we have some pens
            var pens = await context.Pens.ToListAsync();
            if (!pens.Any())
            {
                for (int i = 1; i <= 8; i++)
                {
                    var pen = new Pen { Name = $"Chuồng Số {i}", Capacity = 100, Description = "Khu vực nuôi tiêu chuẩn" };
                    context.Pens.Add(pen);
                }
                await context.SaveChangesAsync();
                pens = await context.Pens.ToListAsync();
            }

            // 2. Create 50 Pigs
            var pigs = new List<Pig>();
            
            // Create some Boars and Sows for breeding history
            for (int i = 1; i <= 50; i++)
            {
                var gender = i <= 5 ? PigGender.Boar : (i <= 15 ? PigGender.Sow : (random.Next(2) == 0 ? PigGender.Boar : PigGender.Sow));
                var birthDate = DateTime.Now.AddDays(-random.Next(30, 400));
                var status = i > 40 ? PigStatus.Sold : PigStatus.Active;
                
                var pig = new Pig
                {
                    TagNumber = $"P{(i).ToString("D3")}",
                    Name = i % 10 == 0 ? $"Heo {i}" : null,
                    Gender = gender,
                    BirthDate = birthDate,
                    Breed = breeds[random.Next(breeds.Length)],
                    Status = status,
                    Weight = random.Next(1, 120),
                    PenId = status == PigStatus.Active ? pens[random.Next(pens.Count)].Id : null
                };
                pigs.Add(pig);
            }
            context.Pigs.AddRange(pigs);
            await context.SaveChangesAsync();

            // 3. Create Breeding & Farrowing History
            var boars = pigs.Where(p => p.Gender == PigGender.Boar).ToList();
            var sows = pigs.Where(p => p.Gender == PigGender.Sow).ToList();

            for (int i = 0; i < 15; i++)
            {
                var sow = sows[random.Next(sows.Count)];
                var boar = boars[random.Next(boars.Count)];
                var breedDate = DateTime.Now.AddDays(-random.Next(120, 180));
                
                var record = new BreedingRecord
                {
                    BoarId = boar.Id,
                    SowId = sow.Id,
                    BreedingDate = breedDate,
                    EstimatedBirthDate = breedDate.AddDays(114),
                    ActualBirthDate = breedDate.AddDays(114 + random.Next(-2, 3)),
                    LitterSizeAlive = random.Next(8, 14),
                    LitterSizeDead = random.Next(0, 2),
                    PenId = sow.PenId
                };
                context.BreedingRecords.Add(record);
            }

            // Upcoming births
            for (int i = 0; i < 5; i++)
            {
                var sow = sows[random.Next(sows.Count)];
                var boar = boars[random.Next(boars.Count)];
                var breedDate = DateTime.Now.AddDays(-random.Next(10, 100));
                
                var record = new BreedingRecord
                {
                    BoarId = boar.Id,
                    SowId = sow.Id,
                    BreedingDate = breedDate,
                    EstimatedBirthDate = breedDate.AddDays(114)
                };
                context.BreedingRecords.Add(record);
            }

            // 4. Create Sale History
            var soldPigs = pigs.Where(p => p.Status == PigStatus.Sold).ToList();
            foreach (var pig in soldPigs)
            {
                var sale = new SaleRecord
                {
                    PigId = pig.Id,
                    SaleDate = DateTime.Now.AddDays(-random.Next(1, 30)),
                    Weight = pig.Weight ?? 100,
                    Price = (pig.Weight ?? 100) * random.Next(50000, 65000),
                    CustomerName = "Thương lái Chợ Gạo",
                    Notes = "Bán cả đàn"
                };
                context.SaleRecords.Add(sale);
            }

            await context.SaveChangesAsync();
            return Content("Success: Seeded 50 pigs, breeding records, and sales history. Visit /Dashboard to see result.");
        }
    }
}
