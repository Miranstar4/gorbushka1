using AdminPanelMarket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AdminPanelMarket.Pages.tablegame
{
    public class IndexModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public IndexModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        public IList<GameUser> gameUser { get; set; } = default!;

        public async Task OnGetAsync()
        {
            gameUser = await _context.GameUsers.OrderByDescending(x => x.Score).Take(100).ToListAsync();
        }
    }
}
