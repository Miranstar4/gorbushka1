using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AdminPanelMarket.Models;
using Telegram.Bot;

namespace AdminPanelMarket.Pages.order
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
        public Order Order { get; set; } = default!;

        [BindProperty]
        public string status { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order =  await _context.Orders
                .Include(x=>x.UserNavigation)
                .Include(x => x.PromocodeNavigation)
                .Include(x=>x.OrderMessages.OrderBy(y=>y.Minteger))
                .ThenInclude(y=>y.AdminNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }
            Order = order;
            status = order.Status;
           ViewData["Product"] = new SelectList(_context.Products, "Id", "Name");
           ViewData["ProductType"] = new SelectList(_context.ProductTypes, "Id", "Name");
           ViewData["User"] = new SelectList(_context.Users, "Id", "Username");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var _order = await _context.Orders.FindAsync(Order.Id);
            _order.Status = status;
            _order.LastUpdateDate = DateTime.UtcNow;
            _context.Update(_order);
            try
            {
                String botToken = _configuration["BotToken"];
                var botClient = new TelegramBotClient(botToken);
                var user = await _context.Users.Where(x => x.Id == _order.User).FirstOrDefaultAsync();
                if (user != null) 
                {
                    if (_order.Status == "Отменено")
                    {
                        await botClient.SendTextMessageAsync(chatId: user.Telegramid, $@"Ваш заказ был отменен");
                        var _userDialog = await _context.UserDialogs.Where(x => x.Order == _order.Id).FirstOrDefaultAsync();
                        if (_userDialog != null)
                        {
                            _userDialog.IsActive = false;
                            _context.UserDialogs.Update(_userDialog);
                        }
                        if (!_order.IsScoreAdd)
                        {
                            user.Score = user.Score + _order.Score;
                            _context.Users.Update(user);
                        }
                        await _context.SaveChangesAsync();
                    }
                    else if (_order.Status == "Завершено")
                    {
                        await botClient.SendTextMessageAsync(chatId: user.Telegramid, $@"Ваш заказ успешно исполнен.
Спасибо, что вы с нами!");
                        var userDialog = await _context.UserDialogs.Where(x => x.Order == _order.Id).FirstOrDefaultAsync();
                        if (userDialog != null)
                        {
                            userDialog.IsActive = false;
                            _context.UserDialogs.Update(userDialog);
                            await _context.SaveChangesAsync();
                        }

                        try
                        {
                            var referalUser = await _context.Users.Where(x => x.Id == user.Referal).FirstOrDefaultAsync();
                            if (referalUser != null)
                            {
                                var _referalRefStatistic = await _context.ReferalStatistics.Where(x => x.User == referalUser.Id).FirstOrDefaultAsync();
                                if (_referalRefStatistic != null)
                                {
                                    _referalRefStatistic.ShippedOrders = _referalRefStatistic.ShippedOrders + 1;
                                    _context.ReferalStatistics.Update(_referalRefStatistic);
                                }
                                else
                                {
                                    var _referalStatistic = new ReferalStatistic()
                                    {
                                        Id = Guid.NewGuid(),
                                        User = referalUser.Id,
                                        ClickOnLink = 0,
                                        MadeOrder = 0,
                                        Paid = 0,
                                        ShippedOrders = 1,
                                        AllScore = 0,
                                    };
                                    _context.ReferalStatistics.Add(_referalStatistic);
                                }
                            }

                            var referals = await _context.ReferalStartups.Where(x => x.Userid == user.Telegramid.ToString() && x.IsBonus == true).ToListAsync();
                            if (referals.Count != 0)
                            {
                                foreach (ReferalStartup rf in referals)
                                {
                                    var userRef = await _context.Users.Where(x => x.Telegramid == rf.Referalid).FirstAsync();
                                    userRef.Score = userRef.Score + 500;
                                    user.Score = user.Score + 500;
                                    rf.IsBonus = false;
                                    _context.ReferalStartups.Update(rf);
                                    _context.Users.Update(userRef);
                                    _context.Users.Update(user);

                                    var history = new ScoreHistory()
                                    {
                                        Id = Guid.NewGuid(),
                                        Date = DateTime.Now,
                                        GivenOrWrittenOff = true,
                                        IsActive = true,
                                        Score = 500,
                                        User = user.Id,
                                        Type = "За друга"
                                    };
                                    _context.ScoreHistories.Add(history);

                                    var historyRef = new ScoreHistory()
                                    {
                                        Id = Guid.NewGuid(),
                                        Date = DateTime.Now,
                                        GivenOrWrittenOff = true,
                                        IsActive = true,
                                        Score = 500,
                                        User = userRef.Id,
                                        Type = "За друга"
                                    };
                                    _context.ScoreHistories.Add(historyRef);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "Admin_Edit_Order_ReferallError", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                            if (ex.InnerException != null)
                            {
                                error.InnerException = ex.InnerException.Message;
                            }
                            _context.ErrorLogs.Add(error);
                            _context.SaveChanges();
                        }

                        if (_order.IsScoreAdd)
                        {
                            user.Score = user.Score + _order.Score;
                            _context.Users.Update(user);
                            var history = new ScoreHistory()
                            {
                                Id = Guid.NewGuid(),
                                Date = DateTime.Now,
                                GivenOrWrittenOff = _order.IsScoreAdd,
                                IsActive = true,
                                Score = _order.Score,
                                User = _order.User,
                                Type = "Зачисление"
                            };
                            _context.ScoreHistories.Add(history);
                        }
                    }
                }
                var msqTg = await _context.MessageTelegrams.Where(x => x.Order == _order.Id).Include(x=>x.AdminNavigation).ToListAsync();
                if (msqTg.Count != 0)
                {
                    foreach (MessageTelegram message in msqTg)
                    {
                        try
                        {
                            await botClient.EditMessageTextAsync(message.AdminNavigation.Telegramid, (int)message.Messageid, "Статус заказа отредактирован в админ панели", replyMarkup: null);
                            message.IsActive = false;
                            _context.MessageTelegrams.Update(message);
                        }
                        catch (Exception ex)
                        {
                            var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "Admin_Changestatus" + message.AdminNavigation.Username, ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                            if (ex.InnerException != null)
                            {
                                error.InnerException = ex.InnerException.Message;
                            }
                            _context.ErrorLogs.Add(error);
                            _context.SaveChanges();
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(Order.Id))
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

        private bool OrderExists(Guid id)
        {
          return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
