using backend6.Helpers;
using backend6.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace backend6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly TelegramAPPContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(TelegramAPPContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        // GET: api/Users/5
        [HttpGet("{token}")]
        public async Task<ActionResult<Models.User>> GetUser(string token)
        {
          if (_context.Users == null)
          {
              return NotFound();
          }
            var user = await _context.Users.Where(x=>x.Token == token).FirstAsync();

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // Post: api/Users
        [HttpPost]
        public async Task<ActionResult> PostUser(UpdateUserData updateUserData)
        {
            var user = await _context.Users.Where(x => x.Token == updateUserData.token).FirstOrDefaultAsync();
            if (user != null)
            {
                user.City = updateUserData.city;
                user.Street = updateUserData.street;
                user.Fio = updateUserData.fio;
                user.Phone = updateUserData.phone;
                _context.Update(user);
                _context.SaveChanges();
            }
            else
            {
                return BadRequest();
            }

            return Ok();
        }

        [Authorize]
        [HttpGet("get-score")]
        public async Task<long> GetScore(string token)
        {
            try
            {
                var user = await _context.Users.Where(x => x.Token == token).FirstOrDefaultAsync();
                return user.Score.Value;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetScore", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                throw ex;
            }
        }

        [Authorize]
        [HttpGet]
        [Route ("GetMyOrders")]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders(string token)
        {
            try
            {
                var orderStatusOrder = new List<string>
                {
                    "Оплачено",
                    "Отправлен",
                    "Менеджер скоро свяжется",
                    "Завершено",
                    "Отменено"
                };
                var user = await _context.Users.Where(x => x.Token == token).FirstOrDefaultAsync();
                var orders = await _context.Orders
                    .Where(x => (x.UserNavigation.Id == user.Id) 
                        && ((x.Status == "Отменено" || x.Status == "Завершено") && (x.Date.Value.Date == DateTime.Now.Date || x.Date.Value.Date >= DateTime.Now.AddDays(-7)) 
                        || (x.Status != "Отменено" && x.Status != "Завершено")))
                    .Include(x => x.ProductNavigation)
                        .ThenInclude(y => y.ProductImages)
                    .ToListAsync();

                var newOrder = orders.OrderBy(x => orderStatusOrder.IndexOf(x.Status)).ToList();

                /*foreach (var _ord in newOrder)
                {
                    if (_ord.Promocode != null)
                    {
                        var promocodeObj = await _context.Promocodes.Where(x => x.Id == _ord.Promocode).FirstOrDefaultAsync();
                        switch (promocodeObj.Type)
                        {
                            case "cena":
                                _ord.ProductNavigation.Cost = _ord.ProductNavigation.Cost - promocodeObj.BonusMoney;
                                break;
                            case "cenaprocent":
                                _ord.ProductNavigation.Cost = _ord.ProductNavigation.Cost - (_ord.ProductNavigation.Cost / 100 * promocodeObj.BonusProcent);
                                break;
                            case "category":
                                _ord.ProductNavigation.Cost = _ord.ProductNavigation.Cost - promocodeObj.BonusMoney;
                                break;
                            case "categoryprocent":
                                _ord.ProductNavigation.Cost = _ord.ProductNavigation.Cost - (_ord.ProductNavigation.Cost / 100 * promocodeObj.BonusProcent);
                                break;
                            case "onlyprocent":
                                _ord.ProductNavigation.Cost = _ord.ProductNavigation.Cost - (_ord.ProductNavigation.Cost / 100 * promocodeObj.BonusProcent);
                                break;
                            case "onlysumma":
                                _ord.ProductNavigation.Cost = _ord.ProductNavigation.Cost - promocodeObj.BonusMoney;
                                break;
                        }
                    }
                }*/

                return newOrder;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetMyOrders", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetMyStatistic")]
        public async Task<ActionResult<ReferalStatistic>> GetMyStatistic(string token)
        {
            try
            {
                var user = await _context.Users.Where(x => x.Token == token).FirstOrDefaultAsync();
                var stat = await _context.ReferalStatistics
                    .Where(x => x.User == user.Id)
                    .FirstOrDefaultAsync();

                return stat;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetMyStatistic", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetMyOrder")]
        public async Task<ActionResult<Order>> GetMyOrder(string token, string id)
        {
            try
            {
                var guidId = new Guid(id);
                var user = await _context.Users.Where(x => x.Token == token).FirstOrDefaultAsync();
                var orders = await _context.Orders.Where(x => x.UserNavigation.Id == user.Id && x.Id == guidId)
                    .Include(x => x.ProductNavigation)
                        .ThenInclude(y => y.ProductImages)
                    .FirstOrDefaultAsync();

                /*if (orders.Promocode != null)
                {
                    var promocodeObj = await _context.Promocodes.Where(x => x.Id == orders.Promocode).FirstOrDefaultAsync();
                    switch (promocodeObj.Type)
                    {
                        case "cena":
                            orders.ProductNavigation.Cost = orders.ProductNavigation.Cost - promocodeObj.BonusMoney;
                            break;
                        case "cenaprocent":
                            orders.ProductNavigation.Cost = orders.ProductNavigation.Cost - (orders.ProductNavigation.Cost / 100 * promocodeObj.BonusProcent);
                            break;
                        case "category":
                            orders.ProductNavigation.Cost = orders.ProductNavigation.Cost - promocodeObj.BonusMoney;
                            break;
                        case "categoryprocent":
                            orders.ProductNavigation.Cost = orders.ProductNavigation.Cost - (orders.ProductNavigation.Cost / 100 * promocodeObj.BonusProcent);
                            break;
                        case "onlyprocent":
                            orders.ProductNavigation.Cost = orders.ProductNavigation.Cost - (orders.ProductNavigation.Cost / 100 * promocodeObj.BonusProcent);
                            break;
                        case "onlysumma":
                            orders.ProductNavigation.Cost = orders.ProductNavigation.Cost - promocodeObj.BonusMoney;
                            break;
                    }
                }*/

                return orders;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetMyOrder", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("CancelOrder")]
        public async Task<ActionResult> CancelOrder(string token, string id)
        {
            try
            {
                var guidId = new Guid(id);
                var user = await _context.Users.Where(x => x.Token == token).FirstOrDefaultAsync();
                var order = await _context.Orders
                    .Where(x => x.UserNavigation.Id == user.Id && x.Id == guidId)
                    .Include(x=>x.MessageTelegrams)
                    .ThenInclude(y=>y.AdminNavigation)
                    .FirstOrDefaultAsync();
                String botToken = _configuration["BotToken"];
                var botClient = new TelegramBotClient(botToken);

                if (order != null)
                {
                    order.Status = "Отменено";
                    _context.Orders.Update(order);
                    if (order.MessageTelegrams.Count != 0)
                    {
                        foreach (MessageTelegram message in order.MessageTelegrams)
                        {
                            try
                            {
                                botClient.EditMessageTextAsync(message.AdminNavigation.Telegramid, (int)message.Messageid, "Заказ отменен пользователем", replyMarkup: null);
                                message.IsActive = false;
                                _context.MessageTelegrams.Update(message);
                            }
                            catch (Exception ex)
                            {
                                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "CancelOrder_" + message.AdminNavigation.Username, ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

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
                return Ok();
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "CancelOrder", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        private bool UserExists(Guid id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpPost("token-auth")]
        public async Task<ActionResult> Token([FromBody] TokenAuthRequestData requestData)
        {
            try
            {
                TokenAuthRequestData Usercredentials = requestData;

                var hash = Usercredentials.Hash;
                var telegramUser = Usercredentials.Hashuser;

                if (ValidateBotHash.Validate(requestData.InitData))
                {
                    var usr = await _context.Users.Where(x => x.Telegramid == telegramUser.Id.ToString()).FirstOrDefaultAsync();
                    if (usr == null)
                    {
                        Guid? userReferal = null;
                        var referalStartup = await _context.ReferalStartups.Where(x => x.Userid == telegramUser.Id.ToString()).FirstOrDefaultAsync();
                        if (referalStartup != null)
                        {
                            var referalUser = await _context.Users.Where(x => x.Telegramid == referalStartup.Referalid).FirstOrDefaultAsync();
                            if (referalUser != null)
                            {
                                userReferal = referalUser.Id;
                            }
                        }
                        usr = new Models.User()
                        {
                            Id = Guid.NewGuid(),
                            Username = telegramUser.Username,
                            Telegramid = telegramUser.Id.ToString(),
                            DateRegister = DateTime.UtcNow,
                            Score = 0,
                            Token = Helper.GenerateToken(128),
                            IsActive = true,
                            Referal = userReferal,
                        };

                        var game = new Models.Game()
                        {
                            Id = Guid.NewGuid(),
                            User = usr.Id,
                            Belt = "white",
                            Score = 0,
                            Woods = 0,
                            IsActive = true,
                        };
                        var referalStatistic = new ReferalStatistic()
                        {
                            Id = Guid.NewGuid(),
                            User = usr.Id,
                            ClickOnLink = 0,
                            MadeOrder = 0,
                            Paid = 0,
                            ShippedOrders = 0,
                            AllScore = 0,
                        };
                        _context.Users.Add(usr);
                        _context.ReferalStatistics.Add(referalStatistic);
                        _context.Games.Add(game);
                        _context.SaveChanges();
                    }
                    else
                    {
                        usr.Token = Helper.GenerateToken(128);
                        _context.Users.Update(usr);
                        _context.SaveChanges();
                    }
                    var now = DateTime.UtcNow;
                    // создаем JWT-токен
                    var jwt = new JwtSecurityToken(
                            issuer: AuthOptions.ISSUER,
                            audience: AuthOptions.AUDIENCE,
                            notBefore: now,
                            //claims: identity.Claims,
                            expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
                    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                    var response = new
                    {
                        access_token = encodedJwt,
                        name = usr.Username,
                        specialtoken = usr.Token
                    };
                    _context.SaveChanges();

                    return Content(JsonConvert.SerializeObject(response));
                }
                else
                {
                    throw new Exception("Неверные данные");
                }
            }
            catch(Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "TokenAuth", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        [HttpGet("userphoto/{userId}")]
        public async Task<IActionResult> GetUserPhoto(long userId)
        {
            string imageDirectory = "photos";
            string directoryPath = Directory.GetCurrentDirectory() + $@"//" + imageDirectory;
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            String botToken = _configuration["BotToken"];
            var botClient = new TelegramBotClient(botToken);
            try
            {
                // Задайте имя файла и путь в папку
                string fileName = $"{userId}_photo.jpg";
                string savePath = Path.Combine(imageDirectory, fileName);

                if (!System.IO.File.Exists(savePath))
                {
                    var user = await botClient.GetChatAsync(userId);

                    if (user.Photo != null)
                    {
                        string photo = user.Photo.BigFileId;
                        var file = await botClient.GetFileAsync(photo);
                        FileStream fs = new FileStream($@"{imageDirectory}/{fileName}", FileMode.Create);
                        // Загрузите изображение с использованием WebClient
                        await botClient.DownloadFileAsync(file.FilePath, fs);
                        fs.Close();
                        fs.Dispose();
                    }
                    else
                    {
                        fileName = "Profile_userpic.png";
                        return File(System.IO.File.ReadAllBytes($@"{imageDirectory}/{fileName}"), "image/jpeg");
                    }
                }

                // Отправьте изображение как файл клиенту
                return File(System.IO.File.ReadAllBytes($@"{imageDirectory}/{fileName}"), "image/jpeg");
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetUserPhoto", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return BadRequest("Произошла ошибка: " + ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetMyToken")]
        public async Task<ActionResult<string>> GetMyToken(string telegramid)
        {
            try
            {
                var user = await _context.Users.Where(x => x.Telegramid == telegramid).FirstOrDefaultAsync();
                return user.Token;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetMyToken", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetMyHistory")]
        public async Task<ActionResult<List<ScoreHistory>>> GetMyHistory(string token)
        {
            try
            {
                var user = await _context.Users.Where(x => x.Token == token).FirstOrDefaultAsync();
                var history = await _context.ScoreHistories
                    .Where(x => x.User == user.Id)
                    .OrderByDescending(x => x.Date)
                    .Take(20)
                    .ToListAsync();

                return history;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetMyHistory", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("GetMyVersion")]
        public async Task<ActionResult<string>> GetMyVersion()
        {
            try
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json").Build();

                var version = config.GetSection("AppVersion").Value;
                return version;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetMyVersion", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("SendGame")]
        public async Task<ActionResult> SendGame(string telegramid)
        {
            String botToken = _configuration["BotToken"];
            var botClient = new TelegramBotClient(botToken);

            try
            {
                string gameShortName = _configuration["GameShortName"];
                string gameTextBtn = _configuration["GameTextBtn"];

                // Отправьте игру
                await botClient.SendGameAsync(
                    Convert.ToInt64(telegramid),
                    gameShortName,
                    replyMarkup: new[]
                    {
                        InlineKeyboardButton.WithCallBackGame(text: gameTextBtn),
                    });
                return Ok();
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "SendGame", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                await _context.SaveChangesAsync();
                return BadRequest(ex);
            }
        }

        [Route("GetGameDataTg")]
        public async Task<ActionResult<GameData>> GetGameDataTg(string telegramid)
        {
            try
            {
                long? tgid = Convert.ToInt64(telegramid);
                var user = await _context.GameUsers.Where(x => x.Telegramid == tgid).FirstOrDefaultAsync();
                GameData gameData = new GameData();
                var usrData = await _context.Users.Where(x => x.Telegramid == telegramid).FirstOrDefaultAsync();
                if (user != null)
                {
                    if (user.DateLastGame == null || user.DateLastGame.Value.AddDays(1) < DateTime.Now)
                    {
                        user.Life = 6;
                        user.DateLastGame = DateTime.Now;
                        _context.GameUsers.Update(user);
                        await _context.SaveChangesAsync();
                    }
                    gameData.gameUser = user;
                }
                else
                {
                    if (usrData != null)
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
                            Login = usrData.Username,
                            Username = usrData.Fio,
                            Token = token,
                            Telegramid = Convert.ToInt64(usrData.Telegramid),
                            Life = 6,
                            DateLastGame = DateTime.Now
                        };
                        _context.GameUsers.Add(newUser);
                        await _context.SaveChangesAsync();
                        gameData.gameUser = newUser;
                    }
                    
                }

                var referals = await _context.ReferalStatistics.Where(x => x.User == usrData.Id).FirstOrDefaultAsync();
                if (referals != null)
                {
                    if (referals.ClickOnLink > 0)
                    {
                        gameData.isReferal = true;
                    }
                    else
                    {
                        gameData.isReferal = false;
                    }
                }
                else
                {
                    gameData.isReferal = false;
                }
                if (user != null)
                {
                    if (user.Life == null) user.Life = 6;
                }
                
                return gameData;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetGameDataTg", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        [Route("GetGameData")]
        public async Task<ActionResult<GameUser>> GetGameData(string tkn)
        {
            try
            {
                var user = await _context.GameUsers.Where(x => x.Token == tkn).FirstOrDefaultAsync();
                if (user.DateLastGame == null || user.DateLastGame.Value.AddDays(1) < DateTime.Now)
                {
                    user.Life = 6;
                    user.DateLastGame = DateTime.Now;
                    _context.GameUsers.Update(user);
                    await _context.SaveChangesAsync();
                }
                return user;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetGameData", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return StatusCode(400);
            }
        }

        
        [Route("SaveGameData")]
        public async Task<ActionResult<GameUser>> SaveGameData(long tg, int woods, string belt, int score)
        {
            try
            {
                var userdb = await _context.GameUsers.Where(x => x.Telegramid == tg).FirstOrDefaultAsync();
                if (userdb != null && userdb.Life > 0)
                {
                    userdb.Woods = woods;
                    userdb.Belt = belt;
                    if (userdb.Score < score)
                    {
                        userdb.Score = score;
                    }
                    userdb.Life = userdb.Life == 0 ? 0 : userdb.Life - 1;
                    _context.GameUsers.Update(userdb);
                    await _context.SaveChangesAsync();

                    var userSystem = await _context.Users.Where(x => x.Telegramid == tg.ToString()).FirstOrDefaultAsync();
                    if (userSystem != null)
                    {
                        if (score > 25000) {
                            var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "ScoreHmm", ErrorMessage = @$"Пользователь набрал {score} очков", StackTrace = null, Createtime = DateTime.Now.ToString() };
                            _context.ErrorLogs.Add(error);
                            await _context.SaveChangesAsync();
                            score = 25000; 
                        }
                        userSystem.Score = userSystem.Score + (score / 100);

                        var history = new ScoreHistory()
                        {
                            Id = Guid.NewGuid(),
                            Date = DateTime.Now,
                            GivenOrWrittenOff = true,
                            IsActive = true,
                            Score = (score / 100),
                            User = userSystem.Id,
                            Type = "Игра"
                        };
                        _context.ScoreHistories.Add(history);

                        _context.Users.Update(userSystem);
                        await _context.SaveChangesAsync();
                    }

                    return userdb;
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "SaveGameData", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return BadRequest();
            }
        }
    }
    public class TokenAuthRequestData
    {
        public Hashuser Hashuser { get; set; }
        public string Hash { get; set; }
        public string InitData { get; set; }
    }

    public class Hashuser
    {
        public long Id { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Username { get; set; }
        public string Language_Code { get; set; }
        public bool Allows_Write_to_PM { get; set; }
    }

    public class UpdateUserData
    {
        public string token;
        public string city;
        public string fio;
        public string street;
        public string phone;
    }

    public class GameData
    {
        public GameUser gameUser { get; set; }
        public bool isReferal { get; set; }
    }
}
