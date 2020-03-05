using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace WebApplication.Models.Authentication
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Pwd { get; set; }
    }
}
