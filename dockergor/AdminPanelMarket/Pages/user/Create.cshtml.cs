using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.user
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
        ViewData["Referal"] = new SelectList(_context.Users, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public User User { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
          if (!ModelState.IsValid || _context.Users == null || User == null)
            {
                return Page();
            }

            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
