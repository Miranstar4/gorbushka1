using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend6.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend6.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
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
        public async Task<ActionResult<IEnumerable<CharacteristicProduct>>> GetCharacteristicProducts(string product)
        {
          if (_context.CharacteristicProducts == null)
          {
              return NotFound();
          }
            return await _context.CharacteristicProducts.Where(x=>x.Product == new Guid(product)).Include(x=>x.CharacteristicNavigation).ToListAsync();
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
    }
}
