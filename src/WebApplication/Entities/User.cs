using System.Collections.Generic;

namespace WebApplication.Entities
{
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        
        public List<Follower> Followers { get; set; } = new List<Follower>();
        public List<Message> Messages { get; set; } = new List<Message>();
    }
}
