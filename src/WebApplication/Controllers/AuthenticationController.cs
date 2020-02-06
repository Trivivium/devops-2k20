using System;

using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    public class AuthenticationController : Controller
    {
        [HttpGet("/login")]
        public IActionResult Login()
        {
            ViewData["title"] = "Login";
            
            throw new NotImplementedException();
        }
        
        [HttpGet("/logout")]
        public IActionResult Logout()
        {
            ViewData["title"] = "Logout";
            
            throw new NotImplementedException();
        }
        
        [HttpGet("/register")]
        public IActionResult Register()
        {
            ViewData["title"] = "Register";
            
            throw new NotImplementedException();
        }
    }
}
