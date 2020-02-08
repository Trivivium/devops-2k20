namespace WebApplication.ViewModels
{
    public class UserVM
    {
        public int ID { get;  }
        public string Username { get; }
        public string Email { get; }

        public UserVM(int id, string username, string email)
        {
            ID = id;
            Username = username;
            Email = email;
        }
    }
}
