using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.Authentication
{
    public class RegisterModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

	    public string RepeatedPassword {get; set;}
    }
}
