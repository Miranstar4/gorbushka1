using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.product
{
    public class CreateModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public CreateModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["Subcategory"] = new SelectList(_context.Subcategories.Where(x => x.IsActive == true), "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Product Product { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.Products == null || Product == null)
            {
                return Page();
            }
            Product.Id = Guid.NewGuid();
            Product.IsActive = true;
            Product.IsDiscount = false;
            _context.Products.Add(Product);
            var characteristics = _context.Characteristics.Where(x => x.Subcategory == Product.Subcategory).ToList();
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
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = Product.Id });
        }
    }
}
