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
    [Authorize]
    [ApiController]
    public class ProductImagesController : ControllerBase
    {
        private readonly TelegramAPPContext _context;
        private readonly IConfiguration _configuration;

        public ProductImagesController(TelegramAPPContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: api/ProductImages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductImage>>> GetProductImages(string productid = null, string productTypeid = null)
        {
            try
            {
                if (_context.ProductImages == null)
                {
                    return NotFound();
                }
                if (productid != "0")
                {
                    return await _context.ProductImages.Where(x => x.Product == new Guid(productid)).OrderBy(x => x.Order).ToListAsync();
                }
                else if (productTypeid != "0")
                {
                    return await _context.ProductImages.Where(x => x.ProductType == new Guid(productTypeid)).OrderBy(x => x.Order).ToListAsync();
                }
                return BadRequest();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: api/ProductImages/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductImage>> GetProductImage(Guid id)
        {
          if (_context.ProductImages == null)
          {
              return NotFound();
          }
            var productImage = await _context.ProductImages.FindAsync(id);

            if (productImage == null)
            {
                return NotFound();
            }

            return productImage;
        }

        [HttpPost("upload/{filename}")]
        [AllowAnonymous]
        public IActionResult UploadImage(string filename, IFormFile file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    var imagesDir = Path.Combine(Directory.GetCurrentDirectory(), "images");
                    if (!Directory.Exists(imagesDir))
                    {
                        Directory.CreateDirectory(imagesDir);
                    }
                    var imagePath = Path.Combine(imagesDir, filename + ".png");

                    using (var stream = System.IO.File.Create(imagePath))
                    {
                        file.CopyTo(stream);
                    }

                    return Ok(new { imagePath });
                }
                else
                {
                    throw new Exception("Файл пустой");
                }
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "UploadImage", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                return BadRequest("No file uploaded.");
            }
        }

        [HttpGet]
        [Route("GetFilesFrom")]
        [AllowAnonymous]
        public async Task<List<string>> GetFilesFrom()
        {
            try
            {
                string directoryPath = Directory.GetCurrentDirectory() + $@"//" + "images";
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var filters = new String[] { "png" };
                bool isRecursive = false;
                List<String> filesFound = new List<String>();
                var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                foreach (var filter in filters)
                {
                    filesFound.AddRange(Directory.GetFiles(Directory.GetCurrentDirectory() + $@"//" + "images", String.Format("*.{0}", filter), searchOption));
                }
                List<string> str = new List<string>();
                foreach (var strFile in filesFound.ToArray())
                {
                    string filePath = strFile;
                    int lastSlashIndex = filePath.LastIndexOf('\\');
                    int lastDotIndex = filePath.LastIndexOf('.');
                    string fileName = filePath.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1).Replace("/app//images/", "");
                    str.Add(fileName);
                }
                return str;
            }
            catch (Exception ex)
            {
                var error = new ErrorLog() { Id = Guid.NewGuid(), FuncName = "GetFilesFrom", ErrorMessage = ex.Message, StackTrace = ex.StackTrace, Createtime = DateTime.Now.ToString() };

                if (ex.InnerException != null)
                {
                    error.InnerException = ex.InnerException.Message;
                }
                _context.ErrorLogs.Add(error);
                _context.SaveChanges();
                throw ex;
            }
        }
    }
}
