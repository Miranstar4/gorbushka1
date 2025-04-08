using AdminPanelMarket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminPanelMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodeController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly TelegramAPPContext _context;
        public CodeController(TelegramAPPContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }
        [HttpGet("{name}")]
        public async Task<IActionResult> Index(string name)
        {
            var admin = await _context.Admins.Where(x => x.Username == name).FirstAsync();
            if (admin != null)
            {
                String botToken = _configuration["BotToken"];

                // Replace "YOUR_BOT_API_TOKEN" with your actual bot API token.

                // Initialize the bot client.
                var botClient = new TelegramBotClient(botToken);

                // Replace with the chat ID of the user or group you want to send the message to.
                long chatId = admin.Telegramid.Value;
                long key = GenerateRandomSixDigitNumber();
                // The message text you want to send.
                string messageText = "Твой ключ " + key;

                try
                {
                    admin.Code = key;
                    _context.Admins.Update(admin);
                    await _context.SaveChangesAsync();
                    // Send the message.
                    await botClient.SendTextMessageAsync(chatId, messageText);
                    Console.WriteLine("Message sent successfully!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
            }
            return Ok();
        }
        long GenerateRandomSixDigitNumber()
        {
            Random random = new Random();
            return random.Next(100000, 999999); // Генерируем случайное число от 100000 до 999999
        }
        [HttpPost]
        [Route("PreWatch")]
        public async Task<IActionResult> PreWatch([FromForm] IFormFile file, [FromForm] string text, [FromForm] string nameadmin)
        {
            var user = User;
            var admin = await _context.Admins.Where(x => x.Username == nameadmin).FirstOrDefaultAsync();
            String botToken = _configuration["BotToken"];
            var botClient = new TelegramBotClient(botToken);

            if (admin != null)
            {
                var fileStream = file.OpenReadStream();

                // Определите имя файла из IFormFile
                string fileName = file.FileName;

                // Создайте InputMediaPhoto с изображением и подписью
                var inputMediaPhoto = new InputFileStream(fileStream, fileName);
                await botClient.SendPhotoAsync(admin.Telegramid, inputMediaPhoto, caption: text);
            }

            return Ok();
        }
    }
}
