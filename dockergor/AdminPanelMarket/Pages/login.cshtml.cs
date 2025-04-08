using AdminPanelMarket.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Telegram.Bot;

namespace AdminPanelMarket.Pages
{
    public class loginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public string Msg { get; set; }

        private readonly IConfiguration _configuration;
        private readonly TelegramAPPContext _context;
        public loginModel(TelegramAPPContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var admin = await _context.Admins.Where(x => x.Username == Username).FirstAsync();
            if (admin == null) return NotFound();
            var context = PageContext.HttpContext;

            if (Username.Equals(admin.Username) && Password.Equals(admin.Code.ToString()))
            {
                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Username));
                identity.AddClaim(new Claim(ClaimTypes.Name, Username));
                identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

                var principal = new ClaimsPrincipal(identity);
                // установка аутентификационных куки
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.Now.AddDays(5),
                    IsPersistent = true,
                };
                context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(principal), authProperties).Wait();
                return Redirect("Index");
            }
            else
            {
                Msg = "Invalid";
                return BadRequest();
            }
        }
    }
}
