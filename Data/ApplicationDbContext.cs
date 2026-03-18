using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Pig> Pigs { get; set; }
    public DbSet<PigHealth> PigHealthRecords { get; set; }
    public DbSet<BreedingRecord> BreedingRecords { get; set; }
    public DbSet<Pen> Pens { get; set; }
    public DbSet<SaleRecord> SaleRecords { get; set; }
    public DbSet<PagePermission> PagePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Pig>()
            .HasOne(p => p.Father)
            .WithMany()
            .HasForeignKey(p => p.FatherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Pig>()
            .HasOne(p => p.Mother)
            .WithMany()
            .HasForeignKey(p => p.MotherId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Pig>()
            .HasIndex(p => p.TagNumber)
            .IsUnique();

        builder.Entity<BreedingRecord>()
            .HasOne(b => b.Boar)
            .WithMany(p => p.BreedingRecordsAsBoar)
            .HasForeignKey(b => b.BoarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BreedingRecord>()
            .HasOne(b => b.Sow)
            .WithMany(p => p.BreedingRecordsAsSow)
            .HasForeignKey(b => b.SowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
