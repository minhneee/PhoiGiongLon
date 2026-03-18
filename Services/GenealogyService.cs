using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Services
{
    public interface IGenealogyService
    {
        Task<(bool isInbred, List<string> commonAncestors)> ValidateInbreeding(int boarId, int sowId, int depth = 3);
        Task<List<Pig>> GetAncestors(int pigId, int depth = 3);
        Task<List<Pig>> GetSiblings(int pigId);
        Task<List<Pig>> GetChildren(int pigId);
    }

    public class GenealogyService(ApplicationDbContext context) : IGenealogyService
    {
        public async Task<List<Pig>> GetAncestors(int pigId, int depth = 3)
        {
            var ancestors = new List<Pig>();
            await FetchAncestorsRecursive(pigId, depth, ancestors);
            return ancestors;
        }

        private async Task FetchAncestorsRecursive(int pigId, int depth, List<Pig> result)
        {
            if (depth <= 0) return;

            var pig = await context.Pigs
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == pigId);

            if (pig == null) return;

            if (pig.FatherId.HasValue)
            {
                var father = await context.Pigs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pig.FatherId);
                if (father != null)
                {
                    result.Add(father);
                    await FetchAncestorsRecursive(father.Id, depth - 1, result);
                }
            }

            if (pig.MotherId.HasValue)
            {
                var mother = await context.Pigs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == pig.MotherId);
                if (mother != null)
                {
                    result.Add(mother);
                    await FetchAncestorsRecursive(mother.Id, depth - 1, result);
                }
            }
        }

        public async Task<List<Pig>> GetSiblings(int pigId)
        {
            var pig = await context.Pigs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == pigId);
            if (pig == null || (!pig.FatherId.HasValue && !pig.MotherId.HasValue)) return new List<Pig>();

            return await context.Pigs
                .AsNoTracking()
                .Where(p => p.Id != pigId && 
                           ((pig.FatherId.HasValue && p.FatherId == pig.FatherId) || 
                            (pig.MotherId.HasValue && p.MotherId == pig.MotherId)))
                .ToListAsync();
        }

        public async Task<List<Pig>> GetChildren(int pigId)
        {
            return await context.Pigs
                .AsNoTracking()
                .Where(p => p.FatherId == pigId || p.MotherId == pigId)
                .ToListAsync();
        }

        public async Task<(bool isInbred, List<string> commonAncestors)> ValidateInbreeding(int boarId, int sowId, int depth = 3)
        {
            var boarAncestors = await GetAncestors(boarId, depth);
            var sowAncestors = await GetAncestors(sowId, depth);

            var boarAncestorIds = boarAncestors.Select(a => a.Id).ToHashSet();
            var sowAncestorIds = sowAncestors.Select(a => a.Id).ToHashSet();

            var commonIds = boarAncestorIds.Intersect(sowAncestorIds).ToList();

            if (commonIds.Any())
            {
                var commonNames = boarAncestors
                    .Where(a => commonIds.Contains(a.Id))
                    .Select(a => $"{a.TagNumber} ({(a.Name ?? "Không tên")})")
                    .Distinct()
                    .ToList();
                return (true, commonNames);
            }

            // Check if they are siblings or parent-child
            if (boarId == sowId) return (true, new List<string> { "Cùng là một con heo!" });

            return (false, new List<string>());
        }
    }
}
