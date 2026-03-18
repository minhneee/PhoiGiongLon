using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwineBreedingManager.Data;
using SwineBreedingManager.Models;

namespace SwineBreedingManager.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class BreedingApiController(ApplicationDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BreedingRecord>>> GetRecords()
        {
            return await context.BreedingRecords
                .Include(b => b.Boar)
                .Include(b => b.Sow)
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BreedingRecord>> GetRecord(int id)
        {
            var record = await context.BreedingRecords.FindAsync(id);
            if (record == null) return NotFound();
            return record;
        }
    }
}
