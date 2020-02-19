namespace WebApplication.Entities
{
    public class Follower
    {
        // WHOM is following WHO
        public int WhoID { get; set; }
        public int WhomID { get; set; }
        
        public User Who { get; set; }
        
        public User Whom { get; set; }
    }
}
