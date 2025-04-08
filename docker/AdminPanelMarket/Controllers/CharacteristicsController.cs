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
    public class CharacteristicsController : ControllerBase
    {
        private readonly TelegramAPPContext _context;

        public CharacteristicsController(TelegramAPPContext context)
        {
            _context = context;
        }

        // GET: api/Characteristics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Characteristic>>> GetCharacteristics()
        {
          if (_context.Characteristics == null)
          {
              return NotFound();
          }
            return await _context.Characteristics.ToListAsync();
        }

        // GET: api/Characteristics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Characteristic>> GetCharacteristic(Guid id)
        {
          if (_context.Characteristics == null)
          {
              return NotFound();
          }
            var characteristic = await _context.Characteristics.FindAsync(id);

            if (characteristic == null)
            {
                return NotFound();
            }

            return characteristic;
        }

        // PUT: api/Characteristics/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCharacteristic(Guid id, Characteristic characteristic)
        {
            if (id != characteristic.Id)
            {
                return BadRequest();
            }

            _context.Entry(characteristic).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CharacteristicExists(id))
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

        // POST: api/Characteristics
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Characteristic>> PostCharacteristic(Characteristic characteristic)
        {
            if (_context.Characteristics == null)
            {
                return Problem("Entity set 'TelegramAPPContext.Characteristics'  is null.");
            }
            characteristic.Id = Guid.NewGuid();
            characteristic.Order = _context.Characteristics.Where(x => x.Subcategory == characteristic.Subcategory && x.IsActive == true).Count() + 1;
            characteristic.IsActive = true;

            _context.Characteristics.Add(characteristic);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CharacteristicExists(characteristic.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCharacteristic", new { id = characteristic.Id }, characteristic);
        }

        // DELETE: api/Characteristics/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharacteristic(Guid id)
        {
            if (_context.Characteristics == null)
            {
                return NotFound();
            }
            var characteristic = await _context.Characteristics.FindAsync(id);
            if (characteristic == null)
            {
                return NotFound();
            }
            characteristic.IsActive = false;
            _context.Characteristics.Update(characteristic);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CharacteristicExists(Guid id)
        {
            return (_context.Characteristics?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
