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
    public class IndexModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public IndexModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        public IList<Subcategory> Subcategory { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Subcategories != null)
            {
                Subcategory = await _context.Subcategories.Where(x=>x.IsActive == true)
                .Include(s => s.CategoryNavigation).ToListAsync();
            }
        }
    }
}
