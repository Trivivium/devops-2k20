using System;

namespace WebApplication.Entities
{
    public class Message
    {
        public int ID { get; set; }
        public int AuthorID { get; set; }
        public string Text { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public bool IsFlagged { get; set; }
        
        public User Author { get; set; }
    }
}
