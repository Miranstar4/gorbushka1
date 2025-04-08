using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using AdminPanelMarket.Models;
using AdminPanelMarket.Helpers;

namespace AdminPanelMarket.Pages.stories
{
    public class CreateModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;
        private readonly IConfiguration _configuration;
        public CreateModel(AdminPanelMarket.Models.TelegramAPPContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public List<string> files;
        public async Task<IActionResult> OnGetAsync()
        {
            String searchFolder = _configuration["UrlImages"];
            var filters = new String[] { "png" };
            files = await Helper.GetFilesFrom();
            return Page();
        }

        [BindProperty]
        public Story _Story { get; set; } = default!;
        

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || _context.Products == null || _Story == null)
            {
                return Page();
            }
            _Story.Id = Guid.NewGuid();
            _Story.IsActive = true;
            _Story.Date = DateTime.Now;
            _context.Stories.Add(_Story);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Edit", new { id = _Story.Id });
        }
    }
}
