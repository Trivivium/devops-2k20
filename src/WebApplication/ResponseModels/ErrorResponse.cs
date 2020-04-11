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

        public ErrorResponse(string message)
        {
            if(message == null)
                throw new ArgumentNullException(nameof(message));
            
            if(string.IsNullOrWhiteSpace(message))
                throw new ArgumentException($"The provided error response message cannot be empty or only whitespace.", nameof(message));
            
            Status = StatusCodes.Status400BadRequest;
            Error = message;
        }

        public ErrorResponse(Exception exception)
        {
            Status = StatusCodes.Status400BadRequest;
            Error = exception.Message;
        }
    }
}
