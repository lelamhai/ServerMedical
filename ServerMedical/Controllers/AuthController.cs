using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ServerMedical.Models;
using Microsoft.EntityFrameworkCore;

namespace ServerMedical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbmedicalContext _db;

        public AuthController(DbmedicalContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users.ToListAsync();
            return Ok(users);
        }
    }
}
