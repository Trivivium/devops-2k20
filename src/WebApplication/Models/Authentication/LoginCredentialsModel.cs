using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.Authentication
{
    public class LoginCredentialsModel
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}