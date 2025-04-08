using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Models;
using TelegramBot.Postgree;
using static System.Net.WebRequestMethods;

[Route("api/[controller]")]
[ApiController]
public class TelegramBotController : ControllerBase
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConfiguration _configuration;
    private readonly TelegramAPPContext _context;

    public static string helloText = $@"Хозяин Горбушки приветствует тебя🤝 

Ты попал сюда не случайно, именно здесь водится техника Apple, Dyson, игровые ноутбуки и много интересных находок⚡️

Покупка всегда доставляет приятные эмоции, мы решили раскрасить их еще больше🔥

В приложении ты сможешь: 
 ⁃ Получить баллы от покупок и тратить в свое удовольствие
 ⁃ Отслеживать статус покупки 
 ⁃ Приглашать друзей и получать от нас денежную благодарность💸
 ⁃ Переключать мозг от работы в игре, в которой можно выиграть iPhone📱 
 ⁃ Находить то, что ищешь по приятным ценам

И пусть каждая покупка в нашем пространстве дарит яркие эмоции и крутое состояние, которое повлияет на все сферы твоей жизни💫

Мы ждем тебя по кнопке
 «МАГАЗИН»⤵️";

    public TelegramBotController(ITelegramBotClient botClient, TelegramAPPContext context, IConfiguration configuration)
    {
        _botClient = botClient;
        _configuration = configuration;
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Update([FromBody] Update update)
    {
        if (update == null)
        {
            return Ok();
        }
        try
        {
            if (update.Type == UpdateType.ChatJoinRequest)
            {
                var chatId = update.ChatJoinRequest.Chat.Id;
                
                await _botClient.SendTextMessageAsync(update.ChatJoinRequest.From.Id, helloText);
                await _botClient.MakeRequestAsync(new ApproveChatJoinRequest(chatId, update.ChatJoinRequest.From.Id));

                var user = await _context.Users.Where(x => x.Telegramid == update.ChatJoinRequest.From.Id.ToString()).FirstOrDefaultAsync();
                if (user == null)
                {
                    var _user = new TelegramBot.Models.User()
                    {
                        Id = Guid.NewGuid(),
                        Username = update.ChatJoinRequest.From.Username,
                        Telegramid = update.ChatJoinRequest.From.Id.ToString(),
                        IsActive = true,
                        Score = 0,
                        Referal = null,
                        DateRegister = DateTime.Now,

                    };
                    _context.Users.Add(_user);
                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
        }
        catch (Exception ex)
        {
            var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "Bot_ChatJoinRequest", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

            if (ex.InnerException != null)
            {
                error.InnerException = ex.InnerException.Message;
            }
            _context.ErrorLogs.Add(error);
            _context.SaveChanges();
            return Ok();
        }
        try
        {
            if (update.Type == UpdateType.Message)
            {
                if (update.Message.Type != MessageType.Text)
                {
                    return Ok();
                }

                if (update.Message.Chat.Id != update.Message.From.Id)
                {
                    return Ok();
                }

                var manager = await _context.Admins.Where(x => x.Telegramid == update.Message.From.Id).FirstOrDefaultAsync();
                if (manager != null)
                {
                    await ManagerMessageEvents(update, manager);
                    return Ok();
                }
                else
                {
                    await UserTextEvents(update);
                    return Ok();
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                if (update.CallbackQuery.GameShortName == "karategame")
                {
                    await SendGameUser(update);
                    return Ok();
                }
                var admin = await _context.Admins.Where(x => x.Telegramid == update.CallbackQuery.From.Id).FirstOrDefaultAsync();
                if (admin != null)
                {
                    if (update.CallbackQuery.Data == "admin_statuses"
                        || update.CallbackQuery.Data == "admin_day_money"
                        || update.CallbackQuery.Data == "admin_week_money"
                        || update.CallbackQuery.Data == "admin_month_money"
                        || update.CallbackQuery.Data == "admin_all_money"
                        || update.CallbackQuery.Data == "admin_status_manager"
                        || update.CallbackQuery.Data == "admin_status_pay"
                        || update.CallbackQuery.Data == "admin_status_send"
                        || update.CallbackQuery.Data == "admin_status_unsuccess"
                        || update.CallbackQuery.Data == "admin_status_return"
                        || update.CallbackQuery.Data == "admin_status_success"
                        )
                    {
                        await AdminCallback(update, admin);
                        return Ok();
                    }
                    await ManagerCallbackEvents(update, admin);
                    return Ok();
                }
                else
                {
                    await SendGameUser(update);
                }
            }

            return Ok();
        }
        catch (Exception ex)
        {

        }
        return Ok();
    }

    async Task<ActionResult> UserTextEvents(Update update)
    {
        try
        {
            
            var user = await _context.Users.Where(x => x.Telegramid == update.Message.From.Id.ToString())
                .Include(x => x.Orders.Where(y => y.Status != "Завершено" && y.Status != "Отменено" && y.UserDialogs.Any(ud => ud.IsActive.Value)))
                    .ThenInclude(x=>x.UserDialogs.Where(y=>y.IsActive == true))
                .FirstOrDefaultAsync();
            if (user == null)
            {
                return await UserRegister(update);
            }
            else
            {
                string[] messageParse = update.Message.Text.Split(' ');
                if (update.Message.Text == "/start" || messageParse[0] == "/start")
                {
                    var responseText = helloText;
                    await _botClient.SendTextMessageAsync(update.Message.From.Id, responseText);
                    return Ok();
                }
                if (user.Orders.Count != 0)
                {
                    var order = user.Orders.First();
                    var userdialog = order.UserDialogs.FirstOrDefault();
                    var messageOrder = update.Message.Text;
                    var orderDialog = await _context.OrderMessages.Where(x => x.Order == order.Id && x.Admin != null).OrderBy(x=>x.Minteger).Include(x=>x.AdminNavigation).LastOrDefaultAsync();
                    if (orderDialog != null && userdialog != null)
                    {
                        var countOrderMessages = _context.OrderMessages.Where(x => x.Order == order.Id).Count();
                        var orderMessage = new OrderMessage()
                        {
                            Id = Guid.NewGuid(),
                            Order = order.Id,
                            IsWrittenAdmin = true,
                            Date = DateTime.UtcNow,
                            Admin = null,
                            Minteger = countOrderMessages,
                            Message = messageOrder
                        };
                        _context.OrderMessages.Add(orderMessage);
                        await _context.SaveChangesAsync();
                        var responseText = $@"Вам сообщение от @{update.Message.From.Username}
{update.Message.Text}";
                        await _botClient.SendTextMessageAsync(orderDialog.AdminNavigation.Telegramid, responseText);
                    }
                }
                else
                {

                }
            }
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex);
        }
    }

    async Task<ActionResult> UserRegister(Update update)
    {
        try
        {
            string[] messageParse = update.Message.Text.Split(' ');
            Guid? referalUserGuid = null;
            if (messageParse[0] == "/start")
            {
                if (messageParse.Length > 1)
                {
                    var referalChecker = await _context.ReferalStartups.Where(x => x.Referalid == messageParse[1] && x.Userid == update.Message.From.Id.ToString()).FirstOrDefaultAsync();
                    if (referalChecker == null)
                    {
                        var referalUser = await _context.Users.Where(x => x.Telegramid == messageParse[1]).Include(x => x.ReferalStatistics).FirstOrDefaultAsync();
                        if (referalUser != null)
                        {
                            var statistic = referalUser.ReferalStatistics.FirstOrDefault();
                            if (statistic != null)
                            {
                                statistic.ClickOnLink++;
                                _context.ReferalStatistics.Update(statistic);
                            }
                            referalUserGuid = referalUser.Id;
                        }
                        var referal = new ReferalStartup()
                        {
                            Id = Guid.NewGuid(),
                            Referalid = messageParse[1],
                            Userid = update.Message.From.Id.ToString(),
                            IsBonus = true
                        };
                        _context.ReferalStartups.Add(referal);
                    }
                }
            }
            var user = new TelegramBot.Models.User()
            {
                Id = Guid.NewGuid(),
                Username = update.Message.From.Username,
                Telegramid = update.Message.From.Id.ToString(),
                IsActive = true,
                Score = 0,
                Referal = referalUserGuid,
                DateRegister = DateTime.Now,

            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            var responseText = helloText;
            await _botClient.SendTextMessageAsync(update.Message.From.Id, responseText);
            return Ok();
        }
        catch (Exception ex)
        {
            var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "TelegramBot_UserRegister", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

            if (ex.InnerException != null)
            {
                error.InnerException = ex.InnerException.Message;
            }
            _context.ErrorLogs.Add(error);
            _context.SaveChanges();
            return Ok();
        }
        
    }

    async Task ManagerMessageEvents(Update update, Admin admin)
    {
        if ((update.Message.Text == "админ" || update.Message.Text == "Админ") && admin.IsManager != true)
        {
            await AdminDialog(update, admin);
        }
        else
        {
            try
            {
                var managerDialog = await _context.ManagerDialogs
                .Where(x => x.IdTelegramManager == admin.Telegramid)
                .Include(x => x.OrderNavigation)
                .ThenInclude(y => y.UserNavigation)
                .FirstOrDefaultAsync();
                if (managerDialog == null || managerDialog.Order == null)
                {
                    // Если диалог пустой, то никому нечего не доставлено
                    var responseText = @"Сообщение никому не было доставлено";
                    await _botClient.SendTextMessageAsync(admin.Telegramid, responseText);
                }
                else
                {
                    string messageOrder = update.Message.Text;
                    long idUser = Convert.ToInt64(managerDialog.OrderNavigation.UserNavigation.Telegramid);
                    // Проверка на отмену заказа
                    if (update.Message.Text == "отмена" || update.Message.Text == "Отмена" || update.Message.Text == "Jnvtyf" || update.Message.Text == "jnvtyf")
                    {
                        var responseText = $@"Отправка сообщения пользователю @{managerDialog.OrderNavigation.UserNavigation.Username} отменено";
                        managerDialog.Order = null;
                        _context.ManagerDialogs.Update(managerDialog);
                        await _context.SaveChangesAsync();
                        await _botClient.SendTextMessageAsync(admin.Telegramid, responseText);
                    }
                    else
                    {
                        var userDialog = await _context.UserDialogs.Where(x => x.Order == managerDialog.Order).FirstOrDefaultAsync();
                        if (userDialog == null)
                        {
                            userDialog = new UserDialog()
                            {
                                Id = Guid.NewGuid(),
                                Order = managerDialog.Order,
                                IsActive = true
                            };
                            _context.UserDialogs.Add(userDialog);
                            await _botClient.SendTextMessageAsync(idUser, $@"С вами начал диалог менеджер по вашему заказу. Пишите сюда в бота, чтобы ответить ему.");
                        }

                        var countOrderMessages = _context.OrderMessages.Where(x => x.Order == managerDialog.Order).Count();
                        var orderMessage = new OrderMessage()
                        {
                            Id = Guid.NewGuid(),
                            Order = managerDialog.Order,
                            IsWrittenAdmin = true,
                            Date = DateTime.UtcNow,
                            Admin = admin.Id,
                            Minteger = countOrderMessages,
                            Message = messageOrder
                        };
                        _context.OrderMessages.Add(orderMessage);

                        var responseText = $@"Сообщение пользователю @{managerDialog.OrderNavigation.UserNavigation.Username} отправлено";
                        managerDialog.Order = null;
                        _context.ManagerDialogs.Update(managerDialog);
                        await _context.SaveChangesAsync();
                        await _botClient.SendTextMessageAsync(admin.Telegramid, responseText);
                        await _botClient.SendTextMessageAsync(idUser, messageOrder);
                    }
                }
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "TelegramBot_Order_ManagerMessageEvents", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
            }
        }
    }

    async Task<ActionResult> ManagerCallbackEvents(Update update, Admin admin)
    {
        try
        {
            var callbackQueryData = update.CallbackQuery.Data;
            int underscoreIndex = callbackQueryData.IndexOf('_');
            string? textGuid = null;
            string? firstPart = null;
            if (underscoreIndex >= 0)
            {
                textGuid = callbackQueryData.Substring(underscoreIndex + 1);
                firstPart = callbackQueryData.Substring(0, underscoreIndex);
            }

            if (textGuid == null) 
            {
                await _botClient.SendTextMessageAsync(admin.Telegramid, "Я не нашёл такого заказа");
            }
            else
            {
                Guid guid = Guid.Parse(textGuid);
                var order = await _context.Orders
                    .Where(x => x.Id == guid)
                    .Include(x=>x.MessageTelegrams)
                    .ThenInclude(y=>y.AdminNavigation)
                    .Include(x => x.UserNavigation)
                    .FirstOrDefaultAsync();

                InlineKeyboardMarkup? replyMarkup = null;
                string headerMessage = "";
                switch (firstPart)
                {
                    case "pay":
                        headerMessage = "Статус: Оплачен";
                        order.Status = "Оплачен";
                        replyMarkup =  new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Доставлен", $@"send_{order.Id}"),
                                InlineKeyboardButton.WithCallbackData("Написать сообщение", $@"msg_{order.Id}"),
                            },
                        }); 
                        break;
                    case "send":
                        order.Status = "Отправлен";
                        headerMessage = "Статус: Отправлен";
                        replyMarkup = new InlineKeyboardMarkup(new[]
                        {
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData("Получен", $@"issen_{order.Id}"),
                                InlineKeyboardButton.WithCallbackData("Написать сообщение", $@"msg_{order.Id}"),
                            },
                        });
                        break;
                    case "issen":
                        order.Status = "Завершено";
                        headerMessage = "Статус: Завершено";
                        var userDialog = await _context.UserDialogs.Where(x => x.Order == order.Id).FirstOrDefaultAsync();
                        if (userDialog != null)
                        {
                            userDialog.IsActive = false;
                            _context.UserDialogs.Update(userDialog);
                            await _context.SaveChangesAsync();
                        }
                        if (order.IsScoreAdd.Value)
                        {
                            var user = order.UserNavigation;
                            user.Score = user.Score + order.Score;
                            var history = new ScoreHistory()
                            {
                                Id = Guid.NewGuid(),
                                Date = DateTime.Now,
                                GivenOrWrittenOff = order.IsScoreAdd,
                                IsActive = true,
                                Score = order.Score,
                                User = order.User,
                                Type = "Зачисление"
                            };
                            _context.ScoreHistories.Add(history);
                            _context.Users.Update(user);
                        }
                        try
                        {
                            var referalUser = await _context.Users.Where(x => x.Id == order.UserNavigation.Referal).FirstOrDefaultAsync();
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
                            var referals = await _context.ReferalStartups.Where(x => x.Userid == order.UserNavigation.Telegramid.ToString() && x.IsBonus == true).ToListAsync();
                            if (referals.Count != 0)
                            {
                                var user = await _context.Users.Where(x => x.Id == order.UserNavigation.Id).FirstOrDefaultAsync();
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
                            var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "TelegramBot_Order_ReferallError", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                            if (ex.InnerException != null)
                            {
                                error.InnerException = ex.InnerException.Message;
                            }
                            _context.ErrorLogs.Add(error);
                            _context.SaveChanges();
                        }

                        long idUser = Convert.ToInt64(order.UserNavigation.Telegramid);
                        await _botClient.SendTextMessageAsync(idUser, $@"Ваш заказ успешно исполнен.
Спасибо, что вы с нами!");
                        break;
                    case "cancel":
                        headerMessage = "Статус: Отменен";
                        var _userDialog = await _context.UserDialogs.Where(x => x.Order == order.Id).FirstOrDefaultAsync();
                        if (_userDialog != null)
                        {
                            _userDialog.IsActive = false;
                            _context.UserDialogs.Update(_userDialog);
                        }
                        if (!order.IsScoreAdd.Value)
                        {
                            var user = order.UserNavigation;
                            user.Score = user.Score + order.Score;
                            _context.Users.Update(user);
                        }
                        await _context.SaveChangesAsync();
                        long _idUser = Convert.ToInt64(order.UserNavigation.Telegramid);
                        await _botClient.SendTextMessageAsync(_idUser, "Ваш заказ был отменен");
                        order.Status = "Отменен";
                        break;
                    case "msg":
                        await ManagerDialogEvents(update, admin, order);
                        return Ok();
                        break;
                }
                order.LastUpdateDate = DateTime.UtcNow;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                foreach (MessageTelegram messageTelegram in order.MessageTelegrams)
                {
                    var message = update.CallbackQuery.Message.Text;
                    string[] lines = message.Split('\n');
                    string resultText = string.Join("\n", lines.Skip(1));

                    await _botClient.EditMessageTextAsync(
                        messageTelegram.AdminNavigation.Telegramid,
                        (int)messageTelegram.Messageid,
                        $@"{headerMessage}
{resultText.Replace("+", "\\+")}",
                        ParseMode.MarkdownV2,
                        replyMarkup: replyMarkup);
                }
                
            }
            return Ok();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    async Task ManagerDialogEvents(Update update, Admin admin, Order order)
    {
        var managerDialog = await _context.ManagerDialogs.Where(x => x.IdTelegramManager == admin.Telegramid).FirstOrDefaultAsync();
        if (managerDialog == null)
        {
            managerDialog = new ManagerDialog()
            {
                Id = Guid.NewGuid(),
                Order = order.Id,
                IdTelegramManager = admin.Telegramid
            };
            _context.ManagerDialogs.Add(managerDialog);
        }
        else
        {
            managerDialog.Order = order.Id;
            _context.ManagerDialogs.Update(managerDialog);
        }
        var responseText = $@"Следующее сообщение будет отправлено пользователю @{order.UserNavigation.Username}
Напиши текст 'отмена', чтобы отменить отправку сообщения";
        await _context.SaveChangesAsync();
        await _botClient.SendTextMessageAsync(admin.Telegramid, responseText);
    }

    async Task<GameUser> GetUserFromGame(Update update)
    {
        try
        {
            var gameuser = await _context.GameUsers.Where(x => x.Telegramid == update.CallbackQuery.From.Id).FirstOrDefaultAsync();
            return gameuser;
        }
        catch (Exception ex)
        {
            var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "Bot_GetUserFromGame", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

            if (ex.InnerException != null)
            {
                error.InnerException = ex.InnerException.Message;
            }
            _context.ErrorLogs.Add(error);
            _context.SaveChanges();
            throw ex;
        }
    }

    async Task<ActionResult> SendGameUser(Update update)
    {
        String connectionPostgree = _configuration["ConnectionStringPostgree"];
        var user = await GetUserFromGame(update);
        if (user != null)
        {
            try
            {
                await _botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, url: $@"https://hunterfacts.github.io/karate-games/index.html?tkn={user.Token}");
                return Ok();
            }
            catch (Exception ex)
            {
                try
                {
                    await _botClient.DeleteMessageAsync(update.CallbackQuery.From.Id, update.CallbackQuery.Message.MessageId);
                    await SendGame(update);
                    return Ok();
                }
                catch (Exception _ex)
                {
                    return BadRequest(_ex);
                }
            }
        }   
        else
        {
            try
            {
                const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                Random random = new Random();
                string token = new string(Enumerable.Repeat(characters, 16)
                  .Select(s => s[random.Next(s.Length)]).ToArray());
                var newUser = new GameUser()
                {
                    Id = Guid.NewGuid(),
                    Score = 0,
                    Woods = 0,
                    Belt = "white",
                    Login = update.CallbackQuery.From.Username,
                    Username = update.CallbackQuery.From.FirstName,
                    Token = token,
                    Telegramid = update.CallbackQuery.From.Id
                };
                _context.GameUsers.Add(newUser);
                await _context.SaveChangesAsync();
                await _botClient.AnswerCallbackQueryAsync(update.CallbackQuery.Id, url: $@"https://hunterfacts.github.io/karate-games/index.html?tkn={token}");
                return Ok();
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "Bot_SendGameUser", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return BadRequest(ex);
            }
        }
    }

    public async Task<ActionResult> SendGame(Update update)
    {
        try
        {
            string gameShortName = _configuration["GameShortName"];
            string gameTextBtn = _configuration["GameTextBtn"];

            // Отправьте игру
            await _botClient.SendGameAsync(
                update.CallbackQuery.From.Id,
                gameShortName,
                replyMarkup: new[]
                {
                        InlineKeyboardButton.WithCallBackGame(text: gameTextBtn),
                });
            return Ok();
        }
        catch (Exception ex)
        {
            var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "Bot_SendGame", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

            if (ex.InnerException != null)
            {
                error.InnerException = ex.InnerException.Message;
            }
            _context.ErrorLogs.Add(error);
            _context.SaveChanges();
            return BadRequest(ex);
        }
    }

    async Task AdminDialog(Update update, Admin admin)
    {
        string responseText = $@"Команды для взаимодействия:
- Кнопка 'Заказ по статусам' - выгрузить количество заказов по определенному статусу
- Кнопка 'Доход за день' - доход за текущий день
- Кнопка 'Доход за неделю' - доход за текущую неделю с понедельника по воскресенье включительно
- Кнопка 'Доход за месяц' - доход за текущий месяц с 1 по последний день месяца включительно
- Кнопка 'Доход за все время' - доход за всё время с начала работы системы (Осторожно! Выгрузка может быть длительная)
";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Заказ по статусам", $@"admin_statuses"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Доход за день", $@"admin_day_money"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Доход за неделю", $@"admin_week_money"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Доход за месяц", $@"admin_month_money"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Доход за все время", $@"admin_all_money"),
            },
        });
        await _botClient.SendTextMessageAsync(admin.Telegramid, responseText, replyMarkup: keyboard);
    }
    async Task AdminCallback(Update update, Admin admin)
    {
        if (update.CallbackQuery.Data == "admin_day_money")
        {
            DateTime today = DateTime.UtcNow.Date;
            DateTime tomorrow = today.AddDays(1);

            var orders = await _context.Orders
                .Where(o => o.LastUpdateDate >= today && o.LastUpdateDate < tomorrow && o.Status == "Завершено")
                .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
                .ToListAsync();

            long? totalCost = orders.Sum(order =>
            {
                var isProduct = order.ProductTypeNavigation == null;
                var isProductCost = isProduct
                    ? order.ProductNavigation.Cost
                    : order.ProductTypeNavigation.Cost;

                return order.IsScoreAdd == true
                    ? isProductCost
                    : isProductCost - order.Score;
            });
            StringBuilder reportBuilder = new StringBuilder();

            reportBuilder.AppendLine("————————————");
            reportBuilder.AppendLine("Отчет по текущему дню:");
            reportBuilder.AppendLine("————————————");
            reportBuilder.AppendLine($"Заработано: {totalCost} руб.");
            reportBuilder.AppendLine("————————————");

            var takeOrders = orders.OrderByDescending(x=>x.ProductNavigation.Cost).Take(10).ToList();

            for (int i = 0; i < takeOrders.Count; i++)
            {
                var isProduct = takeOrders[i].ProductTypeNavigation == null;
                var isProductCost = isProduct
                    ? takeOrders[i].ProductNavigation.Cost
                    : takeOrders[i].ProductTypeNavigation.Cost;

                var realCost = takeOrders[i].IsScoreAdd == true
                    ? isProductCost
                    : isProductCost - takeOrders[i].Score;
                reportBuilder.AppendLine($"{i + 1}. {takeOrders[i].ProductNavigation.Name} | {realCost} руб.");
            }

            reportBuilder.AppendLine("————————————");
            reportBuilder.AppendLine($"Заказов всего: {orders.Count}");

            string reportMessage = reportBuilder.ToString();
            await _botClient.SendTextMessageAsync(admin.Telegramid, reportMessage);
        }
        else if (update.CallbackQuery.Data == "admin_week_money")
        {
            await GenerateWeeklyReportForAdmin(admin.Telegramid);
        }
        else if (update.CallbackQuery.Data == "admin_month_money")
        {
            await GenerateMonthlyReportForAdmin(admin.Telegramid);
        }
        else if (update.CallbackQuery.Data == "admin_all_money")
        {
            await GenerateAllReportForAdmin(admin.Telegramid);
        }
        else if (update.CallbackQuery.Data == "admin_statuses")
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Менеджер скоро свяжется", $@"admin_status_manager"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Оплачено", $@"admin_status_pay"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Отправлено", $@"admin_status_send"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Отменено", $@"admin_status_unsuccess"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Завершено", $@"admin_status_success"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("<- Назад", $@"admin_status_return"),
                },
            });
            await _botClient.EditMessageTextAsync(admin.Telegramid, update.CallbackQuery.Message.MessageId,  update.CallbackQuery.Message.Text, replyMarkup: keyboard);
        }
        else if (update.CallbackQuery.Data == "admin_status_manager" 
            || update.CallbackQuery.Data == "admin_status_pay"
            || update.CallbackQuery.Data == "admin_status_send"
            || update.CallbackQuery.Data == "admin_status_unsuccess"
            || update.CallbackQuery.Data == "admin_status_success")
        {
            string searchText = "";
            int countResult = 30;
            if (update.CallbackQuery.Data == "admin_status_manager")
            {
                searchText = "Менеджер скоро свяжется";
            }
            else if (update.CallbackQuery.Data == "admin_status_pay")
            {
                searchText = "Оплачено";
            }
            else if (update.CallbackQuery.Data == "admin_status_send")
            {
                searchText = "Отправлен";
            }
            else if (update.CallbackQuery.Data == "admin_status_success")
            {
                searchText = "Завершено";
                countResult = 50;
            }
            else if (update.CallbackQuery.Data == "admin_status_unsuccess")
            {
                searchText = "Отменено";
                countResult = 50;
            }

            int totalOrdersCount = await _context.Orders
            .Where(o => o.Status == searchText)
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
            .CountAsync();

            var lastOrders = await _context.Orders
            .Where(o => o.Status == searchText)
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
            .OrderByDescending(o => o.Date)
            .Take(countResult)
            .ToListAsync();

            StringBuilder reportBuilder = new StringBuilder();

            reportBuilder.AppendLine("————————————");
            reportBuilder.AppendLine($@"Последние {countResult} заказов с тегом {searchText}");
            reportBuilder.AppendLine("————————————");

            for (int i = 0; i < lastOrders.Count; i++)
            {
                var isProduct = lastOrders[i].ProductTypeNavigation == null;
                var isProductCost = isProduct
                    ? lastOrders[i].ProductNavigation.Cost
                    : lastOrders[i].ProductTypeNavigation.Cost;

                var realCost = lastOrders[i].IsScoreAdd == true
                    ? isProductCost
                    : isProductCost - lastOrders[i].Score;
                reportBuilder.AppendLine($"{i + 1}. {lastOrders[i].ProductNavigation.Name} | {realCost} руб.");
            }

            reportBuilder.AppendLine("————————————");
            reportBuilder.AppendLine($"Заказов всего: {totalOrdersCount}");

            string reportMessage = reportBuilder.ToString();
            await _botClient.SendTextMessageAsync(admin.Telegramid, reportMessage);
        }
        else if (update.CallbackQuery.Data == "admin_status_return")
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Заказ по статусам", $@"admin_statuses"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Доход за день", $@"admin_day_money"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Доход за неделю", $@"admin_week_money"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Доход за месяц", $@"admin_month_money"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("Доход за все время", $@"admin_all_money"),
                },
            });
            await _botClient.EditMessageTextAsync(admin.Telegramid, update.CallbackQuery.Message.MessageId, update.CallbackQuery.Message.Text, replyMarkup: keyboard);
        }
    }

    public async Task GenerateWeeklyReportForAdmin(long? telegramId)
    {
        DateTime today = DateTime.UtcNow.Date;
        DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        DateTime endOfWeek = startOfWeek.AddDays(7);

        var _totalCost = await _context.Orders
            .Where(o => o.LastUpdateDate >= startOfWeek && o.LastUpdateDate < endOfWeek && o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation).ToListAsync();
        var totalCost = _totalCost.Sum(order =>
            {
                var isProduct = order.ProductTypeNavigation == null;
                var isProductCost = isProduct
                    ? order.ProductNavigation.Cost
                    : order.ProductTypeNavigation.Cost;

                return order.IsScoreAdd == true
                    ? isProductCost
                    : isProductCost - order.Score;
            });

        int totalOrdersCount = await _context.Orders
            .Where(o => o.LastUpdateDate >= startOfWeek && o.LastUpdateDate < endOfWeek && o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
            .CountAsync();

        var lastTenOrders = await _context.Orders
            .Where(o => o.LastUpdateDate >= startOfWeek && o.LastUpdateDate < endOfWeek && o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
            .OrderByDescending(o => o.LastUpdateDate)
            .Take(10)
            .ToListAsync();

        StringBuilder reportBuilder = new StringBuilder();

        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine("Отчет за текущую неделю:");
        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine($"Заработано: {totalCost} руб.");
        reportBuilder.AppendLine("————————————");

        for (int i = 0; i < lastTenOrders.Count; i++)
        {
            var isProduct = lastTenOrders[i].ProductTypeNavigation == null;
            var isProductCost = isProduct
                ? lastTenOrders[i].ProductNavigation.Cost
                : lastTenOrders[i].ProductTypeNavigation.Cost;

            var realCost = lastTenOrders[i].IsScoreAdd == true
                ? isProductCost
                : isProductCost - lastTenOrders[i].Score;
            reportBuilder.AppendLine($"{i + 1}. {lastTenOrders[i].ProductNavigation.Name} | {realCost} руб.");
        }

        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine($"Заказов всего: {totalOrdersCount}");

        string reportMessage = reportBuilder.ToString();
        await _botClient.SendTextMessageAsync(telegramId, reportMessage);
    }

    public async Task GenerateMonthlyReportForAdmin(long? telegramId)
    {
        DateTime today = DateTime.UtcNow.Date;
        DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
        DateTime endOfMonth = startOfMonth.AddMonths(1);

        var _totalCost = await _context.Orders
            .Where(o => o.LastUpdateDate >= startOfMonth && o.LastUpdateDate < endOfMonth && o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation).ToListAsync();

        var totalCost = _totalCost.Sum(order =>
        {
            var isProduct = order.ProductTypeNavigation == null;
            var isProductCost = isProduct
                ? order.ProductNavigation.Cost
                : order.ProductTypeNavigation.Cost;

            return order.IsScoreAdd == true
                ? isProductCost
                : isProductCost - order.Score;
        });

        int totalOrdersCount = await _context.Orders
            .Where(o => o.LastUpdateDate >= startOfMonth && o.LastUpdateDate < endOfMonth && o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
            .CountAsync();

        var lastTenOrders = await _context.Orders
            .Where(o => o.LastUpdateDate >= startOfMonth && o.LastUpdateDate < endOfMonth && o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
            .OrderByDescending(o => o.ProductNavigation.Cost)
            .Take(10)
            .ToListAsync();

        StringBuilder reportBuilder = new StringBuilder();

        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine("Отчет за текущий месяц:");
        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine($"Заработано: {totalCost} руб.");
        reportBuilder.AppendLine("————————————");

        for (int i = 0; i < lastTenOrders.Count; i++)
        {
            var isProduct = lastTenOrders[i].ProductTypeNavigation == null;
            var isProductCost = isProduct
                ? lastTenOrders[i].ProductNavigation.Cost
                : lastTenOrders[i].ProductTypeNavigation.Cost;

            var realCost = lastTenOrders[i].IsScoreAdd == true
                ? isProductCost
                : isProductCost - lastTenOrders[i].Score;
            reportBuilder.AppendLine($"{i + 1}. {lastTenOrders[i].ProductNavigation.Name} | {realCost} руб.");
        }

        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine($"Заказов всего: {totalOrdersCount}");

        string reportMessage = reportBuilder.ToString();
        await _botClient.SendTextMessageAsync(telegramId, reportMessage);
    }

    public async Task GenerateAllReportForAdmin(long? telegramId)
    {

        var _totalCost = await _context.Orders
            .Where(o => o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation).ToListAsync();

        var totalCost = _totalCost.Sum(order =>
        {
            var isProduct = order.ProductTypeNavigation == null;
            var isProductCost = isProduct
                ? order.ProductNavigation.Cost
                : order.ProductTypeNavigation.Cost;

            return order.IsScoreAdd == true
                ? isProductCost
                : isProductCost - order.Score;
        });

        int totalOrdersCount = await _context.Orders
            .Where(o => o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
            .CountAsync();

        var lastTenOrders = await _context.Orders
            .Where(o => o.Status == "Завершено")
            .Include(x => x.ProductNavigation).Include(x => x.ProductTypeNavigation)
            .OrderByDescending(o => o.ProductNavigation.Cost)
            .Take(10)
            .ToListAsync();

        StringBuilder reportBuilder = new StringBuilder();

        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine("Отчет за всё время:");
        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine($"Заработано: {totalCost} руб.");
        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine("Самые дорогие заказы:");

        for (int i = 0; i < lastTenOrders.Count; i++)
        {
            var isProduct = lastTenOrders[i].ProductTypeNavigation == null;
            var isProductCost = isProduct
                ? lastTenOrders[i].ProductNavigation.Cost
                : lastTenOrders[i].ProductTypeNavigation.Cost;

            var realCost = lastTenOrders[i].IsScoreAdd == true
                ? isProductCost
                : isProductCost - lastTenOrders[i].Score;
            reportBuilder.AppendLine($"{i + 1}. {lastTenOrders[i].ProductNavigation.Name} | {realCost} руб.");
        }

        reportBuilder.AppendLine("————————————");
        reportBuilder.AppendLine($"Заказов всего: {totalOrdersCount}");

        string reportMessage = reportBuilder.ToString();
        await _botClient.SendTextMessageAsync(telegramId, reportMessage);
    }
}