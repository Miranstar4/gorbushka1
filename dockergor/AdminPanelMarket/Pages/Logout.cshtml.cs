using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminPanelMarket.Pages
{
    public class LogoutModel : PageModel
    {
        private readonly IAuthenticationService _authenticationService;

        public LogoutModel(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var context = PageContext.HttpContext;
            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.Now.AddDays(5),
                IsPersistent = true,
            };
            await _authenticationService.SignOutAsync(context, User.Identity.AuthenticationType, authProperties);
            return RedirectToPage("/Index"); // Перенаправьте пользователя на главную страницу после выхода
        }
    }
}
