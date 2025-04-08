using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;
using AdminPanelMarket.Helpers;

namespace AdminPanelMarket.Pages.product
{
    public class EditModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        private readonly IConfiguration _configuration;
        public EditModel(AdminPanelMarket.Models.TelegramAPPContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [BindProperty]
        public Guid? LastSubcategory { get; set; } = default!;

        [BindProperty]
        public Product Product { get; set; } = default!;

        [BindProperty]
        public List<ProductImage> ProductImages { get; set; } = default!;

        [BindProperty]
        public List<CharacteristicProduct> CharacteristicProducts { get; set; } = default!;

        [BindProperty]
        public List<ProductType> ProductTypes { get; set; } = default!;

        public List<string> files;
        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product =  await _context.Products
                .Include(x=>x.CharacteristicProducts)
                    .ThenInclude(y=>y.CharacteristicNavigation)
                .Include(x=>x.ProductImages)
                .Include(x => x.ProductTypes.Where(p => p.IsActive == true))
                    .ThenInclude(y => y.ProductImages)
                .Include(x=>x.ProductTops.Take(1))
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            Product = product;
            LastSubcategory = product.Subcategory;
            ProductImages = product.ProductImages.OrderBy(x=>x.Order).ToList();
            CharacteristicProducts = product.CharacteristicProducts.ToList();
            ProductTypes = product.ProductTypes.ToList();
            ViewData["Subcategory"] = new SelectList(_context.Subcategories.Where(x=>x.IsActive == true), "Id", "Name");
            String searchFolder = _configuration["UrlImages"];
            var filters = new String[] { "png" };
            files = await Helper.GetFilesFrom();
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            Product.IsActive = true;

            var currentValue = Product.Subcategory;
            if (LastSubcategory != currentValue)
            {
                var characteristic = _context.CharacteristicProducts.Where(x => x.Product == Product.Id);
                _context.CharacteristicProducts.RemoveRange(characteristic);
                await _context.SaveChangesAsync();

                var characteristics = _context.Characteristics.Where(x=>x.Subcategory == Product.Subcategory && x.IsActive == true).ToList();
                foreach (var _characteristic in characteristics)
                {
                    var characteristicProduct = new CharacteristicProduct
                    {
                        Characteristic = _characteristic.Id,
                        Product = Product.Id,
                        Id = Guid.NewGuid()
                    };
                    _context.CharacteristicProducts.Add(characteristicProduct);
                }
                await _context.SaveChangesAsync();
            }

            _context.Attach(Product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(Product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ProductExists(Guid id)
        {
          return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
