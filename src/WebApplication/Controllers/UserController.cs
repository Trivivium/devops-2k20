using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication.Controllers
{
    [Authorize]
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
