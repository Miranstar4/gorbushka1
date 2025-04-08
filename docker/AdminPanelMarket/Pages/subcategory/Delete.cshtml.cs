using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.subcategory
{
    public class DeleteModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public DeleteModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Subcategory Subcategory { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.Subcategories == null)
            {
                return NotFound();
            }

            var subcategory = await _context.Subcategories.FirstOrDefaultAsync(m => m.Id == id);

            if (subcategory == null)
            {
                return NotFound();
            }
            else 
            {
                Subcategory = subcategory;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null || _context.Subcategories == null)
            {
                return NotFound();
            }
            var subcategory = await _context.Subcategories.FindAsync(id);

            if (subcategory != null)
            {
                Subcategory = subcategory;
                _context.Subcategories.Remove(Subcategory);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
