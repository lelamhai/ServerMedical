namespace ServerMedical.Models.Response
{
    public class ResponseLogin
    {
        private string accessToken;
        private string refreshToken;
        private DateTime refreshExpiresAt;

        public ResponseLogin(string accessToken, string refreshToken, DateTime refreshExpiresAt)
        {
            this.accessToken = accessToken;
            this.refreshToken = refreshToken;
            this.refreshExpiresAt = refreshExpiresAt;
        }

        public string AccessToken
        {
            get { return accessToken; }
            set { accessToken = value; }
        }

        public string RefreshToken
        {
            get { return refreshToken; }
            set { refreshToken = value; }
        }

        public DateTime RefreshExpiresAt
        {
            get { return refreshExpiresAt; }
            set { refreshExpiresAt = value; }
        }
    }
}
