using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacteristicProductsController : ControllerBase
    {
        private readonly TelegramAPPContext _context;

        public CharacteristicProductsController(TelegramAPPContext context)
        {
            _context = context;
        }

        // GET: api/CharacteristicProducts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CharacteristicProduct>>> GetCharacteristicProducts()
        {
          if (_context.CharacteristicProducts == null)
          {
              return NotFound();
          }
            return await _context.CharacteristicProducts.ToListAsync();
        }

        // GET: api/CharacteristicProducts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CharacteristicProduct>> GetCharacteristicProduct(Guid id)
        {
          if (_context.CharacteristicProducts == null)
          {
              return NotFound();
          }
            var characteristicProduct = await _context.CharacteristicProducts.FindAsync(id);

            if (characteristicProduct == null)
            {
                return NotFound();
            }

            return characteristicProduct;
        }

        // PUT: api/CharacteristicProducts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCharacteristicProduct(Guid id, CharacteristicProduct characteristicProduct)
        {
            if (id != characteristicProduct.Id)
            {
                return BadRequest();
            }

            _context.Entry(characteristicProduct).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CharacteristicProductExists(id))
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

        // POST: api/CharacteristicProducts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CharacteristicProduct>> PostCharacteristicProduct(CharacteristicProduct characteristicProduct)
        {
          if (_context.CharacteristicProducts == null)
          {
              return Problem("Entity set 'TelegramAPPContext.CharacteristicProducts'  is null.");
          }
            _context.CharacteristicProducts.Add(characteristicProduct);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CharacteristicProductExists(characteristicProduct.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCharacteristicProduct", new { id = characteristicProduct.Id }, characteristicProduct);
        }

        // DELETE: api/CharacteristicProducts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCharacteristicProduct(Guid id)
        {
            if (_context.CharacteristicProducts == null)
            {
                return NotFound();
            }
            var characteristicProduct = await _context.CharacteristicProducts.FindAsync(id);
            if (characteristicProduct == null)
            {
                return NotFound();
            }

            _context.CharacteristicProducts.Remove(characteristicProduct);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CharacteristicProductExists(Guid id)
        {
            return (_context.CharacteristicProducts?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
