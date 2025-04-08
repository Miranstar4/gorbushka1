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

namespace AdminPanelMarket.Pages.stories
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
        public Story _Story { get; set; } = default!;

        public List<string> files;
        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.Stories == null)
            {
                return NotFound();
            }

            var story =  await _context.Stories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (story == null)
            {
                return NotFound();
            }
            _Story = story;
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
            _Story.IsActive = true;

            _context.Attach(_Story).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(_Story.Id))
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
