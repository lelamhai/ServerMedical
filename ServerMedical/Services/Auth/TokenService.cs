using Microsoft.Identity.Client.NativeInterop;
using Microsoft.IdentityModel.Tokens;
using ServerMedical.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ServerMedical.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string CreateAccessToken(User user)
        {
            //var claims = new List<Claim>
            //{
            //    new Claim(JwtRegisteredClaimNames.Sub, _config["Jwt:Subject"]!),
            //    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    
            //    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
    
            //    new Claim(ClaimTypes.Email, user.Email),
            //    new Claim(ClaimTypes.Role, user.Role)
            //};

            //var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            //var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            //var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:AccessTokenExpirationMinutes"] ?? "60"));

            //var token = new JwtSecurityToken(
            //    issuer: _config["Jwt:Issuer"],
            //    audience: _config["Jwt:Audience"],
            //    claims: claims,
            //    expires: expires,
            //    signingCredentials: creds);

            //return new JwtSecurityTokenHandler().WriteToken(token);
            return "token";
        }
    }
}
