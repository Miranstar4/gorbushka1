using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.stories
{
    public class DeleteModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public DeleteModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        [BindProperty]
      public Story _Story { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.Stories == null)
            {
                return NotFound();
            }

            var story = await _context.Stories.FirstOrDefaultAsync(m => m.Id == id);

            if (story == null)
            {
                return NotFound();
            }
            else 
            {
                _Story = story;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null || _context.Stories == null)
            {
                return NotFound();
            }
            var story = await _context.Stories.FindAsync(id);

            if (story != null)
            {
                _Story = story;
                _Story.IsActive = false;
                _context.Stories.Update(_Story);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
