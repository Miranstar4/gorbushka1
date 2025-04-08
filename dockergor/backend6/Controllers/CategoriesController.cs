using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend6.Models;
using Microsoft.AspNetCore.Authorization;

namespace backend6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly TelegramAPPContext _context;

        public CategoriesController(TelegramAPPContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
          if (_context.Categories == null)
          {
              return NotFound();
          }
            return await _context.Categories.Where(x => x.IsActive == true).ToListAsync();
        }

        // GET: api/Categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategory(Guid id)
        {
          if (_context.Categories == null)
          {
              return NotFound();
          }
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            return category;
        }

        [HttpGet("image/{imageName}")]
        public async Task<IActionResult> GetImg(string imageName)
        {
            // Получение пути к папке с изображениями
            string imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");

            // Формирование полного пути к запрошенному изображению
            string imagePath = Path.Combine(imagesFolder, $"{imageName}.png");

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

        [HttpPost("image/{imageName}")]
        public async Task<IActionResult> DeleteImage(string imageName)
        {
            String searchFolder = Path.Combine(Directory.GetCurrentDirectory(), "images");
            var file = searchFolder + @"/" + imageName + ".png";
            if (System.IO.File.Exists(file))
            {
                System.IO.File.Delete(file);
            }
            else
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "DeleteImage", ErrorMessage = $@"Текущий файл {file} не найден", StackTrace = null, Createtime = DateTime.Now.ToString() };
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
            }
            return Ok();
        }
    }
}
