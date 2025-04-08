using AdminPanelMarket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace AdminPanelMarket.Pages.bot
{
    public class IndexModel : PageModel
    {
        private readonly AdminPanelMarket.Models.TelegramAPPContext _context;
        private readonly IConfiguration _configuration;
        public IndexModel(AdminPanelMarket.Models.TelegramAPPContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        [BindProperty]
        public string Filename { get; set; }

        [BindProperty]
        public string TextPost { get; set; }

        public IList<Post> _Posts { get; set; } = default!;

        public IList<string> Errors { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            _Posts = await _context.Posts
                    .OrderBy(x=>x.DateSend)
                    .Include(x=>x.AdminNavigation)
                .Take(10).ToListAsync();
            Errors = new List<string>();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = User;
            var admin = await _context.Admins.Where(x => x.Username == user.Identity.Name).FirstOrDefaultAsync();
            String botToken = _configuration["BotToken"];
            Errors = new List<string>();
            var botClient = new TelegramBotClient(botToken);

            if (admin != null)
            {
                Post _post = new Post()

                {
                    Id = Guid.NewGuid(),
                    Text = TextPost,
                    Admin = admin.Id,
                    DateSend = DateTime.Now,
                };
                _context.Posts.Add(_post);
                await _context.SaveChangesAsync();

                var users = await _context.Users.Where(x=>x.IsActive == true).ToListAsync();

                var chatIds = users.Select(usr => usr.Telegramid).ToList();
                var semaphore = new SemaphoreSlim(10); // Ограничьте количество одновременных запросов
                var messagesPerSecond = 25; // Ограничение на сообщения в секунду

                string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "postPhoto");
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                if (Upload != null && Upload.Length > 0)
                {
                    var imagePath = Path.Combine(folderPath, Upload.FileName);

                    using (var stream = System.IO.File.Create(imagePath))
                    {
                        Upload.CopyTo(stream);
                    }
                }

                var inputMediaPhoto = new InputFileUrl("https://admin.gorbuskamsc.ru/api/image/" + Upload.FileName);

                var tasks = chatIds.Select(async chatId =>
                {
                    semaphore.WaitAsync();
                    try
                    {
                        await botClient.SendPhotoAsync(chatId, inputMediaPhoto, caption: TextPost);
                    }
                    catch (Exception ex)
                    {
                        Errors.Add(ex.Message + @$" {chatId}");
                    }
                    finally
                    {
                        await Task.Delay(1000 / messagesPerSecond);
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
            }

            _Posts = await _context.Posts
                    .OrderBy(x => x.DateSend)
                    .Include(x => x.AdminNavigation)
                .Take(10).ToListAsync();
            TextPost = null;
            Upload = null;
            return Page();
        }
    }
}
