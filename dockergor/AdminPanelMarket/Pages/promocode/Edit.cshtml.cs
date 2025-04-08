using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.promocode
{
    public class EditModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public EditModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Promocode Promocode { get; set; } = default!;

        public List<PromocodeSubcategory> promocodeSubcategories { get; set; } = default!;

        public List<Subcategory> subcategories { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.Promocodes == null)
            {
                return NotFound();
            }

            var promocode =  await _context.Promocodes.Include(x=>x.PromocodeSubcategories)
                .ThenInclude(x=>x.SubcategoryNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (promocode == null)
            {
                return NotFound();
            }
            promocodeSubcategories = promocode.PromocodeSubcategories.ToList();
            subcategories = await _context.Subcategories.Include(x=>x.CategoryNavigation).ToListAsync();
            Promocode = promocode;
           ViewData["UserActivePromocode"] = new SelectList(_context.Users, "Id", "Id");
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
            Promocode.IsActive = true;
            _context.Attach(Promocode).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PromocodeExists(Promocode.Id))
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

        private bool PromocodeExists(Guid id)
        {
          return (_context.Promocodes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
