using System.Text.Json.Serialization;

namespace WebApplication.ResponseModels
{
    public class ErrorResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
        
        [JsonPropertyName("error_msg")]
        public string Error { get; set; }
    }
}
