using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.user
{
    public class IndexModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public IndexModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        public IList<User> User { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Users != null)
            {
                User = await _context.Users
                .Include(u => u.ReferalNavigation).ToListAsync();
            }
        }
    }
}
