using AdminPanelMarket.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminPanelMarket.Pages.image
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<string> files;

        public async Task<IActionResult> OnGetAsync()
        {
            String searchFolder = _configuration["UrlImages"];
            var filters = new String[] { "png" };
            files = await Helper.GetFilesFrom();
            return Page();
        }

        [BindProperty]
        public IFormFile Upload { get; set; }

        [BindProperty]
        public string Filename { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            String baseUrl = _configuration["MainAppUrl"];
            using (var client = new HttpClient())
            {
                using (var content = new MultipartFormDataContent())
                {
                    var fileStream = Upload.OpenReadStream();
                    var fileName = Filename;

                    content.Add(new StreamContent(fileStream), "file", fileName);

                    var response = await client.PostAsync(baseUrl+"/api/ProductImages/upload/"+ fileName, content);

                    if (response.IsSuccessStatusCode)
                    {
                        // Обработайте успешный ответ, если необходимо.
                        var imagePath = await response.Content.ReadAsStringAsync();
                        // imagePath содержит путь к сохраненному изображению во втором приложении.
                    }
                    else
                    {
                        var i = 0;
                    }
                }
            }
            files = await Helper.GetFilesFrom();
            return Page();
        }
    }
}
