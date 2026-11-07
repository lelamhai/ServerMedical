namespace ServerMedical.Models.Request.Author
{
    public class RequestRegister
    {
        private string email;
        private string password;
        private string firstName;
        private string lastName;
        private string gender;

        public RequestRegister(string email, string password, string firstName, string lastName, string gender)
        {
            this.email = email;
            this.password = password;
            this.firstName = firstName;
            this.lastName = lastName;
            this.gender = gender;
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

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public string Gender
        {
            get { return gender; }
            set { gender = value; }
        }
    }
}
