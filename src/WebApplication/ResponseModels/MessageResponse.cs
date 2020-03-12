using System;
using System.Text.Json.Serialization;

namespace WebApplication.ResponseModels
{
    public class MessageResponse
    {
        public string Content { get; set; }
        
        [JsonPropertyName("pub_date")]
        public DateTimeOffset PublishDate { get; set; }
        
        [JsonPropertyName("user")]
        public string Author { get; set; }
    }
}
