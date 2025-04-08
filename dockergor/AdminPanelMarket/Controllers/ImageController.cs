using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanelMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ImageController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("{imageName}")]
        public async Task<IActionResult> GetImg(string imageName)
        {
            // Получение пути к папке с изображениями
            string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "postPhoto");

            // Формирование полного пути к запрошенному изображению
            string imagePath = Path.Combine(imagesFolder, imageName);

            // Проверка существования файла
            if (System.IO.File.Exists(imagePath))
            {
                // Выгрузка изображения с использованием MIME-типа "image/png"
                return PhysicalFile(imagePath, "image/png");
            }
            else
            {
                // Если изображение не найдено, можно вернуть другой результат
                // Например, отдать изображение-заглушку или сообщение об ошибке
                return NotFound("Image not found");
            }
        }

        [HttpPost("{imageName}")]
        public async Task<IActionResult> DeleteImage(string imageName)
        {
            String searchFolder = _configuration["UrlImages"];
            var file = searchFolder + @"\" + imageName + ".png";
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
            }
            return Ok();
        }
    }
}
