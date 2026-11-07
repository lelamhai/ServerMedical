using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ServerMedical.Entities;
using ServerMedical.Models.Request.Author;
using ServerMedical.Services.Auth;
using ServerMedical.Utils;
using ServerMedical.Models.Response.Author;

namespace ServerMedical.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DbmedicalContext _db;
        private readonly ITokenService _tokens;

        public AuthController(DbmedicalContext db, ITokenService tokens)
        {
            _db = db;
            _tokens = tokens;
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

            try
            {
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
            catch (Exception ex)
            {
                return StatusCode(500, new WrapperData<string>
                {
                    Message = "An error occurred while processing the request.",
                    Data = null,
                });
            }
            
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(RequestLogin request)
        {
            if (request.Email == null)
            {
                return BadRequest(new WrapperData<string> { Message = "Email is required.", Data = null });
            } 
                
            if (request.Password == null)
            {
                return BadRequest(new WrapperData<string> { Message = "Password is required.", Data = null });
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null) return BadRequest(new WrapperData<string> { Message = "Invalid email or password.", Data = null });

            var passwordVerificationResult = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (passwordVerificationResult != PasswordVerificationResult.Success)
            {
                return BadRequest(new WrapperData<string> { Message = "Invalid email or password.", Data = null });
            }

            var token = _tokens.CreateAccessToken(user);
            var refresh = _tokens.CreateRefreshToken(user.UserId);

            var existingToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == user.UserId);

            if (existingToken != null)
            {
                existingToken.Token = refresh.Token;
                existingToken.ExpiresAt = refresh.ExpiresAt;
                existingToken.IsRevoked = false;
            }
            else
            {
                _db.RefreshTokens.Add(refresh);
            }

            await _db.SaveChangesAsync();
            return Ok(new WrapperData<ResponseLogin> { Message = "Login successful.", Data = new ResponseLogin(token, refresh.Token, refresh.ExpiresAt) });
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RequestRefreshToken request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = "Refresh token is required.",
                    Data = null,
                });
            }

            var storedToken = await _db.RefreshTokens
                .Include(token => token.User)
                .FirstOrDefaultAsync(token => token.Token == request.RefreshToken);

            if (storedToken?.User == null || storedToken.IsRevoked == true || storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                return Unauthorized(new WrapperData<string>
                {
                    Message = "Refresh token is invalid or expired.",
                    Data = null,
                });
            }

            var accessToken = _tokens.CreateAccessToken(storedToken.User);
            var newRefreshToken = _tokens.CreateRefreshToken(storedToken.User.UserId);

            storedToken.Token = newRefreshToken.Token;
            storedToken.ExpiresAt = newRefreshToken.ExpiresAt;
            storedToken.IsRevoked = false;

            await _db.SaveChangesAsync();

            return Ok(new WrapperData<ResponseLogin>
            {
                Message = "Token refreshed successfully.",
                Data = new ResponseLogin(accessToken, newRefreshToken.Token, newRefreshToken.ExpiresAt),
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(RequestLogout request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new WrapperData<string>
                {
                    Message = "Refresh token is required.",
                    Data = null,
                });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(token =>
                token.Token == request.RefreshToken && token.UserId == userId);

            if (refreshToken == null)
            {
                return NotFound(new WrapperData<string>
                {
                    Message = "Refresh token not found.",
                    Data = null,
                });
            }

            refreshToken.IsRevoked = true;
            await _db.SaveChangesAsync();

            return Ok(new WrapperData<string>
            {
                Message = "Logout successful.",
                Data = null,
            });
        }

        [HttpGet("test")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users.ToListAsync();
            return Ok(users);
        }
    }
}
