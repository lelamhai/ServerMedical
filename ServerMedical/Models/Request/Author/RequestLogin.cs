namespace ServerMedical.Models.Request.Author
{
    public class RequestLogin
    {
        private string email;
        private string password;

        public RequestLogin(string email, string password)
        {
            this.email = email;
            this.password = password;
        }
        public string Email
        {
            get { return email; }
            set { email = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }
    }
}
