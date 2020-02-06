namespace WebApplication.ViewModels
{
    public class UserVM
    {
        public int ID { get;  }
        public string Username { get; }

        public UserVM(int id, string username)
        {
            ID = id;
            Username = username;
        }
    }
}
