using Microsoft.IdentityModel.Tokens;
using ServerMedical.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),

                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]!);
            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"] ?? "15"));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public RefreshToken CreateRefreshToken(int userId)
        {
            var lifetime = TimeSpan.FromDays(int.Parse(_config["Jwt:RefreshTokenExpireDays"] ?? "7"));
            var bytes = new byte[64];
            RandomNumberGenerator.Fill(bytes);
            var random = Convert.ToBase64String(bytes);
            return new RefreshToken
            {
                UserId = userId,
                Token = random,
                IsRevoked = false,
                ExpiresAt = DateTime.UtcNow.Add(lifetime),
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
