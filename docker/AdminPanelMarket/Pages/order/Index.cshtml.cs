using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;

namespace AdminPanelMarket.Pages.order
{
    public class IndexModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;

        public IndexModel(AdminPanelMarket.Models.TelegramAPPContext context)
        {
            _context = context;
        }

        public IList<Order> Order { get;set; } = default!;

        public IList<Order> OrderDelete { get; set; } = default!;

        public IList<Order> OrderClear { get; set; } = default!;

        public async Task OnGetAsync()
        {
            if (_context.Orders != null)
            {
                var orderStatusOrder = new List<string>
                {
                    "Оплачено",
                    "Отправлен",
                    "Менеджер скоро свяжется",
                    "Завершено",
                    "Отменено"
                };
                var dirtOrders = await _context.Orders
                    .Where(x=>x.Status != "Завершено")
                .Include(o => o.ProductNavigation)
                .Include(o => o.ProductTypeNavigation)
                .Include(o => o.UserNavigation).Take(200).ToListAsync();

                Order = dirtOrders.OrderBy(x => orderStatusOrder.IndexOf(x.Status)).ToList();

                OrderClear = Order.Where(x => x.Status != "Отменено").ToList();

                OrderDelete = Order.Where(x => x.Status == "Отменено").ToList();
            }
        }
    }
}
