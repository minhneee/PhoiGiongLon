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
    public class PigsApiController(ApplicationDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pig>>> GetPigs()
        {
            return await context.Pigs.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pig>> GetPig(int id)
        {
            var pig = await context.Pigs.FindAsync(id);
            if (pig == null) return NotFound();
            return pig;
        }

        [HttpPost]
        public async Task<ActionResult<Pig>> PostPig(Pig pig)
        {
            context.Pigs.Add(pig);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPig), new { id = pig.Id }, pig);
        }
    }
}
