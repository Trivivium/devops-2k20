using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication.Entities;
using WebApplication.ViewModels;
using WebApplication.ViewModels.Timeline;

namespace WebApplication.Controllers
{
    [Authorize]
    public class TimelineController : Controller
    {
        [AllowAnonymous]
        [HttpGet("/")]
        [HttpGet("/public")]
        public IActionResult Timeline()
        {
            // TODO: If user isn't authenticated change to "Public Timeline"
            ViewData["title"] = "Timeline";

            var messages = new List<TimelineMessageVM>
            {
                new TimelineMessageVM(new UserVM(2, "Foo"), "This is a test... NOTHING ELSE IS WORKING ATM!", DateTimeOffset.UtcNow)
            };
            
            var user = new UserVM(1, "Thomas");
            var vm = new UserTimelineVM(user, false, messages);
            
            return View("Timeline", vm);
        }

        [HttpGet("/{username}")]
        public IActionResult UserTimeline(string username)
        {
            ViewData["title"] = "Timeline"; // TODO: Add the user's username before the title
            
            throw new NotImplementedException();
        }

        [HttpPost("/add_message")]
        public IActionResult AddMessage()
        {
            throw new NotImplementedException();
        }

        [HttpGet("/AccessDenied")]
        public IActionResult AccessDenied()
        {
            ViewData["title"] = "Access denied";

            return View();
        }

        [HttpGet("/Error")]
        public IActionResult Error()
        {
            ViewData["title"] = "Error";
            
            return View();
        }
    }
}
