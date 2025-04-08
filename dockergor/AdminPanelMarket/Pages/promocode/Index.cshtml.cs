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
    public class IndexModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public IndexModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        public IList<Promocode> Promocode { get;set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Promocodes != null)
            {
                Promocode = await _context.Promocodes
                .Include(p => p.UserActivePromocodeNavigation).ToListAsync();
            }
        }
    }
}
