using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.subcategory
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
            ViewData["Category"] = new SelectList(_context.Categories.Where(x => x.IsActive == true), "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Subcategory Subcategory { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || _context.Subcategories == null || Subcategory == null)
            {
                return Page();
            }
            Subcategory.Id = Guid.NewGuid();
            _context.Subcategories.Add(Subcategory);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = Subcategory.Id } );
        }
    }
}
