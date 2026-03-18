using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndUserAsync(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roleNames = { "CHỦ TRẠI", "CÔNG NHÂN" };
            
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Remove other roles to clarify the system
            var existingRoles = await roleManager.Roles.ToListAsync();
            foreach (var role in existingRoles)
            {
                if (!roleNames.Contains(role.Name!))
                {
                    await roleManager.DeleteAsync(role);
                }
            }

            // Create admin user
            var adminEmail = "admin@swinebreeding.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "CHỦ TRẠI");
            }

            // Seed sample pigs
            if (!context.Pigs.Any())
            {
                // Level 0: Great-grandparents
                var ggFather = new Pig { TagNumber = "GG-001", Name = "Cụ Hùng", Gender = PigGender.Boar, BirthDate = DateTime.Now.AddYears(-6), Breed = "Yorkshire Pure", Status = PigStatus.Active, AvatarUrl = "https://images.unsplash.com/photo-1544225580-3ef372242826?w=200&h=200&fit=crop" };
                var ggMother = new Pig { TagNumber = "GG-002", Name = "Cụ Mơ", Gender = PigGender.Sow, BirthDate = DateTime.Now.AddYears(-6), Breed = "Yorkshire Pure", Status = PigStatus.Active, AvatarUrl = "https://images.unsplash.com/photo-1596733430284-f7437764b1a9?w=200&h=200&fit=crop" };
                context.Pigs.AddRange(ggFather, ggMother);
                await context.SaveChangesAsync();

                // Level 1: Grandparents
                var gFather = new Pig { TagNumber = "G-001", Name = "Ông Nội", Gender = PigGender.Boar, BirthDate = DateTime.Now.AddYears(-4), Breed = "Yorkshire", Status = PigStatus.Active, FatherId = ggFather.Id, MotherId = ggMother.Id, AvatarUrl = "https://images.unsplash.com/photo-1516467508483-a7212febe31a?w=200&h=200&fit=crop" };
                context.Pigs.Add(gFather);
                await context.SaveChangesAsync();

                // Level 2: Parents
                var father = new Pig { TagNumber = "B001", Name = "Đại Hùng", Gender = PigGender.Boar, BirthDate = DateTime.Now.AddYears(-2), Breed = "Yorkshire", Status = PigStatus.Active, FatherId = gFather.Id, AvatarUrl = "https://images.unsplash.com/photo-1544225580-3ef372242826?w=200&h=200&fit=crop" };
                var mother = new Pig { TagNumber = "S001", Name = "Hoa Đào", Gender = PigGender.Sow, BirthDate = DateTime.Now.AddYears(-1).AddMonths(-6), Breed = "Landrace", Status = PigStatus.Active, AvatarUrl = "https://images.unsplash.com/photo-1596733430284-f7437764b1a9?w=200&h=200&fit=crop" };
                context.Pigs.AddRange(father, mother);
                await context.SaveChangesAsync();

                // Level 3: Children (Current Generation)
                var child1 = new Pig { TagNumber = "C001", Name = "Heo Con 1", Gender = PigGender.Boar, BirthDate = DateTime.Now.AddMonths(-3), Breed = "Hybrid", Status = PigStatus.Active, FatherId = father.Id, MotherId = mother.Id, AvatarUrl = "https://images.unsplash.com/photo-1516467508483-a7212febe31a?w=200&h=200&fit=crop" };
                var child2 = new Pig { TagNumber = "C002", Name = "Heo Con 2", Gender = PigGender.Sow, BirthDate = DateTime.Now.AddMonths(-3), Breed = "Hybrid", Status = PigStatus.Active, FatherId = father.Id, MotherId = mother.Id, AvatarUrl = "https://images.unsplash.com/photo-1516467508483-a7212febe31a?w=200&h=200&fit=crop" };
                context.Pigs.AddRange(child1, child2);
                await context.SaveChangesAsync();

                // Seed some other unrelated pigs
                context.Pigs.Add(new Pig { TagNumber = "S002", Name = "Mận Trắng", Gender = PigGender.Sow, BirthDate = DateTime.Now.AddYears(-1).AddMonths(-4), Breed = "Landrace", Status = PigStatus.Active, AvatarUrl = "https://images.unsplash.com/photo-1570129477492-45c003edd2be?w=200&h=200&fit=crop" });
                await context.SaveChangesAsync();

                // Seed a breeding record
                var record = new BreedingRecord
                {
                    BoarId = father.Id,
                    SowId = mother.Id,
                    BreedingDate = DateTime.Now.AddMonths(-5),
                    EstimatedBirthDate = DateTime.Now.AddMonths(-2),
                    ActualBirthDate = DateTime.Now.AddMonths(-2),
                    LitterSizeAlive = 10,
                    Notes = "Lứa phối thành công sinh con C001, C002"
                };
                context.BreedingRecords.Add(record);
                await context.SaveChangesAsync();
            }

            // Seed initial PagePermissions
            if (!context.PagePermissions.Any())
            {
                var initialPermissions = new List<PagePermission>
                {
                    new PagePermission { PageName = "Danh Sách Đàn", DisplayName = "Quản lý heo", ControllerName = "Pigs", IsAllowed = true },
                    new PagePermission { PageName = "Phối Giống & Đẻ", DisplayName = "Quản lý sinh sản", ControllerName = "Breeding", IsAllowed = true },
                    new PagePermission { PageName = "Sơ Đồ Chuồng", DisplayName = "Quản lý chuồng trại", ControllerName = "Pen", IsAllowed = true },
                    new PagePermission { PageName = "Kinh Doanh", DisplayName = "Quản lý bán hàng", ControllerName = "Sales", IsAllowed = true },
                    new PagePermission { PageName = "Báo Cáo", DisplayName = "Báo cáo thống kê", ControllerName = "Report", IsAllowed = true },
                    new PagePermission { PageName = "Gia Phả", DisplayName = "Xem gia phả", ControllerName = "Genealogy", IsAllowed = true }
                };
                context.PagePermissions.AddRange(initialPermissions);
                await context.SaveChangesAsync();
            }
        }
    }
}
