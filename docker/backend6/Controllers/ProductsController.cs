using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend6.Models;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

namespace backend6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly TelegramAPPContext _context;
        private readonly IConfiguration _configuration;

        public ProductsController(TelegramAPPContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            return await _context.Products.Where(x=>x.IsActive == true).ToListAsync();
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products
                .Include(x=>x.SubcategoryNavigation)
                .Include(x=>x.ProductImages)
                .Where(x=>x.Id == id).FirstAsync();

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpGet]
        [Route("score")]
        public async Task<ActionResult<long>> GetScoreUser(string usertoken, string product)
        {
            long _score = 0;

            var userByUser = await _context.Users.Where(x=>x.Token == usertoken).FirstAsync();
            var productByProduct = await _context.Products.Where(x => x.Id == new Guid(product)).FirstAsync();

            long _scoreMax = (productByProduct.Cost.Value / 10);

            if (userByUser.Score > _scoreMax)
            {
                _score = _scoreMax;
            }
            else
            {
                _score = userByUser.Score.Value;
            }

            return _score;
        }

        [HttpGet("category/{id}")]
        public async Task<ActionResult<IEnumerable<SubcategoriesAndProducts>>> GetByCategories(string id)
        {
            try
            {
                if (id != null)
                {
                    var idGuid = new Guid(id);
                    var category = await _context.Categories.Where(x => x.Id == idGuid).FirstOrDefaultAsync();
                    if (category != null)
                    {
                        List<SubcategoriesAndProducts> subcategoriesAndProducts = new List<SubcategoriesAndProducts>();
                        var subcategory = await _context.Subcategories.Where(x => x.Category == category.Id && x.IsActive == true).ToListAsync();

                        foreach (Subcategory sub in subcategory)
                        {
                            var products = await _context.Products.Where(x => x.Subcategory == sub.Id && x.IsActive == true).Include(x=>x.ProductImages).ToListAsync();
                            var characterist = await _context.Characteristics.Where(x => x.Subcategory == sub.Id && x.IsActive == true).ToListAsync();

                            var subcategoriesAndProduct = new SubcategoriesAndProducts()
                            {
                                subguid = sub.Id,
                                subcategory = sub,
                                characteristics = characterist,
                                products = products
                            };
                            subcategoriesAndProducts.Add(subcategoriesAndProduct);
                        }
                        return subcategoriesAndProducts;
                    }
                    else
                    {
                        return BadRequest("Не найдена категория");
                    }
                }
                else
                {
                    return BadRequest("Не найдена категория");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        [HttpPost]
        [Route("addorder")]
        public async Task<ActionResult> AddOrder(OrderToken ordertoken)
        {
            try
            {
                String botToken = _configuration["BotToken"];

                List<Admin> managers = await _context.Admins.Where(x => x.IsManager == true).ToListAsync();

                var currentUser = await _context.Users.Where(x => x.Token == ordertoken.token).FirstAsync();
                var order = ordertoken.order;

                currentUser.Phone = order.Phone;
                currentUser.City = order.City;
                currentUser.Street = order.Street;
                currentUser.Fio = order.Fio;

                if (!order.IsScoreAdd.Value)
                {
                    currentUser.Score = currentUser.Score - order.Score;
                }
                _context.Users.Update(currentUser);

                order.IsFinish = false;
                order.User = currentUser.Id;
                order.Date = DateTime.UtcNow;
                order.LastUpdateDate = DateTime.UtcNow;
                order.Status = "Менеджер скоро свяжется";
                order.Id = Guid.NewGuid();

                await _context.Orders.AddAsync(order);

                if (!order.IsScoreAdd.Value)
                {
                    var history = new ScoreHistory()
                    {
                        Id = Guid.NewGuid(),
                        Date = DateTime.Now,
                        GivenOrWrittenOff = order.IsScoreAdd.Value,
                        IsActive = true,
                        Score = order.Score,
                        User = order.User,
                        Type = order.IsScoreAdd.Value ? "Зачисление" : "Списание"
                    };
                    _context.ScoreHistories.Add(history);
                }

                try
                {
                    var referalUser = await _context.Users.Where(x => x.Id == currentUser.Referal).FirstOrDefaultAsync();
                    if (referalUser != null)
                    {
                        var _referalRefStatistic = await _context.ReferalStatistics.Where(x => x.User == referalUser.Id).FirstOrDefaultAsync();
                        if (_referalRefStatistic != null)
                        {
                            _referalRefStatistic.MadeOrder = _referalRefStatistic.MadeOrder + 1;
                            _context.ReferalStatistics.Update(_referalRefStatistic);
                        }
                        else
                        {
                            var _referalStatistic = new ReferalStatistic()
                            {
                                Id = Guid.NewGuid(),
                                User = referalUser.Id,
                                ClickOnLink = 0,
                                MadeOrder = 1,
                                Paid = 0,
                                ShippedOrders = 0,
                                AllScore = 0,
                            };
                            _context.ReferalStatistics.Add(_referalStatistic);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "addorder_ReferallError", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                    if (ex.InnerException != null)
                    {
                        error.InnerException = ex.InnerException.Message;
                    }
                    _context.ErrorLogs.Add(error);
                    _context.SaveChanges();
                }

                await _context.SaveChangesAsync();

                var _order = await _context.Orders
                    .Include(x => x.ProductNavigation)
                    .Include(x => x.UserNavigation)
                    .Include(x => x.ProductTypeNavigation).Where(x => x.Id == order.Id).FirstAsync();

                var productString = _order.ProductType != null ? _order.ProductTypeNavigation.Name : _order.ProductNavigation.Name;
                var productCost = _order.ProductType != null ? _order.ProductTypeNavigation.Cost : _order.ProductNavigation.Cost;
                string productPromocodeText = "";
                
                bool isUsePromocode = false;
                if (ordertoken.promocode != null)
                {
                    Promocode promocodeObj = await _context.Promocodes.Include(x=>x.PromocodeSubcategories).Where(x=>x.Id == ordertoken.promocode).FirstOrDefaultAsync();
                    var userPromocode = await _context.PromocodeUserActives.Where(x => x.User == _order.User).FirstOrDefaultAsync();
                    if (userPromocode == null)
                    {
                        userPromocode = new PromocodeUserActive()
                        {
                            Id = Guid.NewGuid(),
                            Data = DateTime.Now,
                            User = _order.User,
                            Promocode = promocodeObj.Id
                        };
                        _context.PromocodeUserActives.Add(userPromocode);
                        await _context.SaveChangesAsync();
                        productPromocodeText = promocodeObj.Code;
                        switch (promocodeObj.Type)
                        {
                            case "cena":
                                if (productCost >= promocodeObj.PriceСondition)
                                {
                                    isUsePromocode = true;
                                    productCost = productCost - promocodeObj.BonusMoney;
                                }
                                break;
                            case "cenaprocent":
                                if (productCost >= promocodeObj.PriceСondition)
                                {
                                    isUsePromocode = true;
                                    productCost = productCost - (productCost / 100 * promocodeObj.BonusProcent);
                                }
                                break;
                            case "category":
                                foreach (var sub in promocodeObj.PromocodeSubcategories)
                                {
                                    if (sub.Subcategory == _order.ProductNavigation.Subcategory)
                                    {
                                        isUsePromocode = true;
                                        productCost = productCost - promocodeObj.BonusMoney;
                                    }
                                }
                                break;
                            case "categoryprocent":
                                foreach (var sub in promocodeObj.PromocodeSubcategories)
                                {
                                    if (sub.Subcategory == _order.ProductNavigation.Subcategory)
                                    {
                                        isUsePromocode = true;
                                        productCost = productCost - (productCost / 100 * promocodeObj.BonusProcent);
                                    }
                                }
                                break;
                            case "onlyprocent":
                                isUsePromocode = true;
                                productCost = productCost - (productCost / 100 * promocodeObj.BonusProcent);
                                break;
                            case "onlysumma":
                                isUsePromocode = true;
                                productCost = productCost - promocodeObj.BonusMoney;
                                break;
                        }
                        if (isUsePromocode) _order.Promocode = promocodeObj.Id;
                    }
                }
                var promocodeMessage = isUsePromocode == true ? $@"Он ввёл промокод {productPromocodeText}, товар со скидкой 

" : "";
                var scoreMessage = !_order.IsScoreAdd.Value ? $@"Он выбрал потратить баллы {_order.Score}. Итоговая стоимость {productCost - _order.Score}" : String.Empty;
                var botClient = new TelegramBotClient(botToken);
                foreach (Admin manager in managers)
                {
                    try
                    {
                        // Создаем клавиатуру с двумя кнопками.
                        var keyboard = new InlineKeyboardMarkup(new[]
                        {
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Оплатили", $@"pay_{_order.Id}"),
                        InlineKeyboardButton.WithCallbackData("Написать сообщение", $@"msg_{_order.Id}"),
                    },
                });

                        // Текст сообщения и клавиатура.
                        var messageText = $@"Статус: Новый
Поступил новый заказ от {_order.Fio}
Товар: {productString}
Стоимость {productCost}р
{scoreMessage}
Контактные данные:
Имя: {_order.Fio}
Телефон: {_order.Phone.Replace("+", "\\+")}
Город доставки: {_order.City}
Адрес доставки: {_order.Street}

{promocodeMessage}Логин пользователя: @{_order.UserNavigation.Username}";

                        // Отправляем сообщение с клавиатурой и получаем ID сообщения.
                        var message = await botClient.SendTextMessageAsync(
                            chatId: manager.Telegramid.Value,
                            text: messageText,
                            parseMode: ParseMode.MarkdownV2,
                            disableWebPagePreview: true,
                            disableNotification: false,
                            replyToMessageId: 0,
                            replyMarkup: keyboard);
                        var messageId = message.MessageId;

                        MessageTelegram msg = new MessageTelegram()
                        {
                            Id = Guid.NewGuid(),
                            Order = _order.Id,
                            Admin = manager.Id,
                            Messageid = messageId,
                            IsActive = true
                        };
                        await _context.MessageTelegrams.AddAsync(msg);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "AddOrder_"+manager.Telegramid.ToString(), ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                        if (ex.InnerException != null)
                        {
                            error.InnerException = ex.InnerException.Message;
                        }
                        _context.ErrorLogs.Add(error);
                        _context.SaveChanges();
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "AddOrder", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return Ok();
            }
        }

        [HttpGet]
        [Route("checkpromocode")]
        public async Task<ActionResult<PromocodeResult>> CheckPromocode(string promocode, Guid product, string usertoken)
        {
            try
            {
                var result = new PromocodeResult();
                result.resultText = "Данного промокода не существует";
                var _promocodes = await _context.Promocodes.Where(x => x.IsActive == true).Include(x => x.PromocodeSubcategories).ToListAsync();
                var promocodeObj = _promocodes.Where(x => string.Equals(x.Code, promocode, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                User? user = await _context.Users.Where(x => x.Token == usertoken).FirstOrDefaultAsync();
                if (user != null)
                {
                    var userPromocode = await _context.PromocodeUserActives.Where(x => x.User == user.Id).FirstOrDefaultAsync();
                    if (userPromocode != null)
                    {
                        result.resultText = "Вы уже вводили данный промокод";
                        return result;
                    }

                    if (promocodeObj != null)
                    {
                        var _product = await _context.Products.FindAsync(product);
                        switch (promocodeObj.Type)
                        {
                            case "cena":
                                if (_product.Cost >= promocodeObj.PriceСondition)
                                {
                                    result.promocode = promocodeObj;
                                    result.resultText = promocodeObj.SuccessMessage;
                                }
                                else
                                {
                                    result.resultText = promocodeObj.ErrorMessage;
                                }
                                break;
                            case "cenaprocent":
                                result.isProcent = true;
                                if (_product.Cost >= promocodeObj.PriceСondition)
                                {
                                    result.promocode = promocodeObj;
                                    result.resultText = promocodeObj.SuccessMessage;
                                }
                                else
                                {
                                    result.resultText = promocodeObj.ErrorMessage;
                                }
                                break;
                            case "category":
                                result.resultText = promocodeObj.ErrorMessage;
                                foreach (var sub in promocodeObj.PromocodeSubcategories)
                                {
                                    if (sub.Subcategory == _product.Subcategory)
                                    {
                                        result.promocode = promocodeObj;
                                        result.resultText = promocodeObj.SuccessMessage;
                                    }
                                }
                                break;
                            case "categoryprocent":
                                result.isProcent = true;
                                result.resultText = promocodeObj.ErrorMessage;
                                foreach (var sub in promocodeObj.PromocodeSubcategories)
                                {
                                    if (sub.Subcategory == _product.Subcategory)
                                    {
                                        result.promocode = promocodeObj;
                                        result.resultText = promocodeObj.SuccessMessage;
                                    }
                                }
                                break;
                            case "onlyprocent":
                                result.isProcent = true;
                                result.promocode = promocodeObj;
                                result.resultText = promocodeObj.SuccessMessage;
                                break;
                            case "onlysumma":
                                result.promocode = promocodeObj;
                                result.resultText = promocodeObj.SuccessMessage;
                                break;
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("search")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsSearch(string search)
        {
            if (_context.Products == null)
            {
                return NotFound();
            }
            return await _context.Products.Where(x=>x.Name.Contains(search)).Take(10).ToListAsync();
        }

        [HttpGet]
        [Route("TopProduct")]
        public async Task<ActionResult<IEnumerable<ProductTop>>> TopProduct()
        {
            if (_context.ProductTops == null)
            {
                return NotFound();
            }
            return await _context.ProductTops.OrderBy(x=>x.Date).Include(x=>x.ProductNavigation).ThenInclude(y=>y.ProductImages).Take(10).ToListAsync();
        }
    }

    public class SubcategoriesAndProducts
    {
        public Guid subguid;
        public Subcategory subcategory;
        public List<Characteristic> characteristics;
        public List<Product> products;
    }

    public class OrderToken
    {
        public Order order;
        public string token;
        public Guid? promocode;
    }

    public class PromocodeResult
    {
        public Promocode? promocode;
        public string? resultText;
        public bool? isProcent;
    }
}
