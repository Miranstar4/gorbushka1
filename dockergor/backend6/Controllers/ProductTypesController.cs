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
    public class ProductTypesController : ControllerBase
    {
        private readonly TelegramAPPContext _context;

        public ProductTypesController(TelegramAPPContext context)
        {
            _context = context;
        }

        // GET: api/ProductTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetProductTypes(string product)
        {
          if (_context.ProductTypes == null)
          {
              return NotFound();
          }
            return await _context.ProductTypes.Where(x=>x.Product == new Guid(product) && x.IsActive == true).ToListAsync();
        }

        // GET: api/ProductTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductType>> GetProductType(Guid id)
        {
          if (_context.ProductTypes == null)
          {
              return NotFound();
          }
            var productType = await _context.ProductTypes.FindAsync(id);

            if (productType == null)
            {
                return NotFound();
            }

            return productType;
        }
    }
}
