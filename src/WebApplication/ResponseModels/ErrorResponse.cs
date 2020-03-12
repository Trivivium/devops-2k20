using System;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Http;

namespace WebApplication.ResponseModels
{
    public class ErrorResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
        
        [JsonPropertyName("error_msg")]
        public string Error { get; set; }

        public ErrorResponse()
        { }

        public ErrorResponse(Exception exception)
        {
            Status = StatusCodes.Status400BadRequest;
            Error = exception.Message;
        }
    }
}
