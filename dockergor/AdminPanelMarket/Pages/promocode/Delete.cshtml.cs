using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.promocode
{
    public class DeleteModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public DeleteModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Promocode Promocode { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.Promocodes == null)
            {
                return NotFound();
            }

            var promocode = await _context.Promocodes.FirstOrDefaultAsync(m => m.Id == id);

            if (promocode == null)
            {
                return NotFound();
            }
            else 
            {
                Promocode = promocode;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null || _context.Promocodes == null)
            {
                return NotFound();
            }
            var promocode = await _context.Promocodes.FindAsync(id);

            if (promocode != null)
            {
                Promocode = promocode;
                _context.Promocodes.Remove(Promocode);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
