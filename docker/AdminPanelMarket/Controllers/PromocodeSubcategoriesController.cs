using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;
using Microsoft.AspNetCore.Authorization;

namespace AdminPanelMarket.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class PromocodeSubcategoriesController : ControllerBase
    {
        private readonly TelegramAPPContext _context;

        public PromocodeSubcategoriesController(TelegramAPPContext context)
        {
            _context = context;
        }

        // GET: api/PromocodeSubcategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PromocodeSubcategory>>> GetPromocodeSubcategories()
        {
          if (_context.PromocodeSubcategories == null)
          {
              return NotFound();
          }
            return await _context.PromocodeSubcategories.ToListAsync();
        }

        // GET: api/PromocodeSubcategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PromocodeSubcategory>> GetPromocodeSubcategory(Guid id)
        {
          if (_context.PromocodeSubcategories == null)
          {
              return NotFound();
          }
            var promocodeSubcategory = await _context.PromocodeSubcategories.FindAsync(id);

            if (promocodeSubcategory == null)
            {
                return NotFound();
            }

            return promocodeSubcategory;
        }

        // PUT: api/PromocodeSubcategories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPromocodeSubcategory(Guid id, PromocodeSubcategory promocodeSubcategory)
        {
            if (id != promocodeSubcategory.Id)
            {
                return BadRequest();
            }

            _context.Entry(promocodeSubcategory).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PromocodeSubcategoryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/PromocodeSubcategories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PromocodeSubcategory>> PostPromocodeSubcategory(PromocodeSubcategory promocodeSubcategory)
        {
          if (_context.PromocodeSubcategories == null)
          {
              return Problem("Entity set 'TelegramAPPContext.PromocodeSubcategories'  is null.");
          }
            promocodeSubcategory.Id = Guid.NewGuid();
            _context.PromocodeSubcategories.Add(promocodeSubcategory);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PromocodeSubcategoryExists(promocodeSubcategory.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPromocodeSubcategory", new { id = promocodeSubcategory.Id }, promocodeSubcategory);
        }

        // DELETE: api/PromocodeSubcategories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePromocodeSubcategory(Guid id)
        {
            if (_context.PromocodeSubcategories == null)
            {
                return NotFound();
            }
            var promocodeSubcategory = await _context.PromocodeSubcategories.FindAsync(id);
            if (promocodeSubcategory == null)
            {
                return NotFound();
            }

            _context.PromocodeSubcategories.Remove(promocodeSubcategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PromocodeSubcategoryExists(Guid id)
        {
            return (_context.PromocodeSubcategories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
