using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using backend6.Models;

namespace backend6.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoriesController : ControllerBase
    {
        private readonly TelegramAPPContext _context;

        public StoriesController(TelegramAPPContext context)
        {
            _context = context;
        }

        // GET: api/Stories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Story>>> GetStories(string token)
        {
          if (_context.Stories == null)
          {
              return NotFound();
          }
            return await _context.Stories.Where(x=>x.IsActive == true).Include(x=>x.StoriesVisits.Where(y=>y.UserNavigation.Token == token)).ToListAsync();
        }

        [HttpPost]
        [Route ("WatchStory")] async Task<ActionResult> WatchStory(string token)
        {
            return Ok();
        }
    }
}
