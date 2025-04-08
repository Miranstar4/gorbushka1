using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;
using Telegram.Bot;

namespace AdminPanelMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductTypesController : ControllerBase
    {
        private readonly TelegramAPPContext _context;
        private readonly IConfiguration _configuration;
        public ProductTypesController(TelegramAPPContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/ProductTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductType>>> GetProductTypes()
        {
          if (_context.ProductTypes == null)
          {
              return NotFound();
          }
            return await _context.ProductTypes.ToListAsync();
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

        // PUT: api/ProductTypes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProductType(Guid id, ProductType productType)
        {
            if (id != productType.Id)
            {
                return BadRequest();
            }
            productType.IsActive = true;
            _context.Entry(productType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductTypeExists(id))
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

        // POST: api/ProductTypes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProductType>> PostProductType(ProductType productType)
        {
          if (_context.ProductTypes == null)
          {
              return Problem("Entity set 'TelegramAPPContext.ProductTypes'  is null.");
          }
            productType.Id = Guid.NewGuid();
            productType.IsActive = true;
            _context.ProductTypes.Add(productType);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ProductTypeExists(productType.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetProductType", new { id = productType.Id }, productType);
        }

        // DELETE: api/ProductTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProductType(Guid id)
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

            productType.IsActive = false;
            _context.ProductTypes.Update(productType);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductTypeExists(Guid id)
        {
            return (_context.ProductTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage(string message, Guid orderid)
        {
            var user = User;

            var admin = await _context.Admins.Where(x => x.Username == user.Identity.Name).FirstOrDefaultAsync();
            var order = await _context.Orders.Where(x => x.Id == orderid).Include(x => x.UserNavigation).FirstOrDefaultAsync();
            if (order != null)
            {
                String botToken = _configuration["BotToken"];
                var botClient = new TelegramBotClient(botToken);
                long idUser = Convert.ToInt64(order.UserNavigation.Telegramid);
                var userDialog = await _context.UserDialogs.Where(x => x.Order == order.Id).FirstOrDefaultAsync();
                if (userDialog == null)
                {
                    userDialog = new UserDialog()
                    {
                        Id = Guid.NewGuid(),
                        Order = order.Id,
                        IsActive = true
                    };
                    _context.UserDialogs.Add(userDialog);
                    await botClient.SendTextMessageAsync(idUser, $@"С вами начал диалог менеджер по вашему заказу. Пишите сюда в бота, чтобы ответить ему.");
                }
                
                var countOrderMessages = _context.OrderMessages.Where(x => x.Order == order.Id).Count();
                var orderMessage = new OrderMessage()
                {
                    Id = Guid.NewGuid(),
                    Order = order.Id,
                    IsWrittenAdmin = true,
                    Date = DateTime.UtcNow,
                    Admin = admin.Id,
                    Minteger = countOrderMessages,
                    Message = message
                };
                _context.OrderMessages.Add(orderMessage);
                await _context.SaveChangesAsync();
                await botClient.SendTextMessageAsync(idUser, message);
            }
            
            return Ok();
        }

        [HttpGet]
        [Route("CheckPokupatel")]
        public async Task<IActionResult> CheckPokupatel(bool check, Guid productid)
        {
            var product = await _context.Products.Where(x => x.Id == productid).FirstOrDefaultAsync();
            if (check)
            {
                if (product != null)
                {
                    var productTop = new ProductTop()
                    {
                        Id = Guid.NewGuid(),
                        Date = DateTime.Now.ToString(),
                        IsActive = true,
                        Product = product.Id
                    };
                    _context.ProductTops.Add(productTop);
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
            else
            {
                if (product != null)
                {
                    var productTops = await _context.ProductTops.Where(x=>x.Product == product.Id).ToListAsync();
                    _context.ProductTops.RemoveRange(productTops);
                    await _context.SaveChangesAsync();
                }
                return Ok();
            }
        }

        [HttpGet]
        [Route("CheckDiscount")]
        public async Task<IActionResult> CheckDiscount(bool check, Guid productid)
        {
            var product = await _context.Products.Where(x => x.Id == productid).FirstOrDefaultAsync();
            if (check)
            {
                if (product != null)
                {
                    product.IsDiscount = true;
                }
            }
            else
            {
                if (product != null)
                {
                    product.IsDiscount = false;
                }
            }
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet]
        [Route("UpdateCharacteristic")]
        public async Task<IActionResult> UpdateCharacteristic(Guid productid)
        {
            var product = await _context.Products.Where(x => x.Id == productid).Include(x=>x.CharacteristicProducts).FirstOrDefaultAsync();

            var characteristic = product.CharacteristicProducts;
            _context.CharacteristicProducts.RemoveRange(characteristic);
            await _context.SaveChangesAsync();

            var characteristics = _context.Characteristics.Where(x => x.Subcategory == product.Subcategory && x.IsActive == true).ToList();
            foreach (var _characteristic in characteristics)
            {
                var characteristicProduct = new CharacteristicProduct
                {
                    Characteristic = _characteristic.Id,
                    Product = product.Id,
                    Id = Guid.NewGuid()
                };
                _context.CharacteristicProducts.Add(characteristicProduct);
            }
            await _context.SaveChangesAsync();

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
