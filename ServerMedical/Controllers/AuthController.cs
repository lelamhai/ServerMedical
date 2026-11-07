using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Request.Author;
using ServerMedical.Utils;

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

        [HttpPost("register")]
        public async Task<IActionResult> Register(RequestRegister user)
        {
            if(user.FirstName == null)
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = "First name is required.",
                    Data = null,
                });
            }

            if (user.LastName == null)
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = "Last name is required.",
                    Data = null,
                });
            }

            if (user.Email == null)
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = "Email is required.",
                    Data = null,
                });
            }

            if (user.Password == null)
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = "Password is required.",
                    Data = null,
                });
            }

            if (user.Gender == null)
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = "Gender is required.",
                    Data = null,
                });
            }

            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (existingUser != null)
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = "Email already exists.",
                    Data = null,
                });
            }

            var newUser = new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PasswordHash = user.Password,
                Gender = user.Gender,
                Role = "Volunteer"
            };
            var passwordHash = new PasswordHasher<User>().HashPassword(newUser, user.Password);
            newUser.PasswordHash = passwordHash;
            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            return Ok(new WrapperData<string>
            {
                Message = "User registered successfully.",
                Data = null,
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users.ToListAsync();
            return Ok(users);
        }
    }
}
