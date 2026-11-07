using ServerMedical.Entities;

namespace ServerMedical.Services.Auth
{
    public interface ITokenService
    {
        string CreateAccessToken(User user);
        RefreshToken CreateRefreshToken(int userId);
    }
}
