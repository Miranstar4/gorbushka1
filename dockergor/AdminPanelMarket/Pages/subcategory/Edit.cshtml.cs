using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.subcategory
{
    public class EditModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public EditModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Subcategory Subcategory { get; set; } = default!;

        [BindProperty]
        public List<Characteristic> Characteristics { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.Subcategories == null)
            {
                return NotFound();
            }

            var subcategory =  await _context.Subcategories.Include(x=> x.Characteristics).FirstOrDefaultAsync(m => m.Id == id);
            if (subcategory == null)
            {
                return NotFound();
            }
            Characteristics = subcategory.Characteristics.Where(x=>x.IsActive == true).OrderBy(x=>x.Order).ToList();
            Subcategory = subcategory;
            ViewData["Category"] = new SelectList(_context.Categories.Where(x=>x.IsActive == true), "Id", "Name");
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
            Subcategory.IsActive = true;
            _context.Attach(Subcategory).State = EntityState.Modified;
            if (Characteristics.Count != 0)
            {
                _context.Update(Characteristics);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubcategoryExists(Subcategory.Id))
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

        private bool SubcategoryExists(Guid id)
        {
          return (_context.Subcategories?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
