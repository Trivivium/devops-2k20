using System;

using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    public class UserController : Controller
    {
        [HttpPost("/{username}/follow")]
        public IActionResult Follow(string username)
        {
            throw new NotImplementedException();
        }
        
        [HttpPost("/{username}/unfollow")]
        public IActionResult Unfollow(string username)
        {
            throw new NotImplementedException();
        }
    }
}
