using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WebApplication.Auth;
using WebApplication.Services;
using WebApplication.Extensions;
using WebApplication.Models.Timeline;
using WebApplication.ViewModels;
using WebApplication.ViewModels.Timeline;

namespace WebApplication.Controllers
{
    [Authorize(Policy = AuthPolicies.Registered)]
    public class TimelineController : Controller
    {
        private const int ResultsPerPage = 30;
        private readonly TimelineService _timelineService;
        private readonly UserService _userService;

        public TimelineController(TimelineService timelineService, UserService userService)
        {
            _timelineService = timelineService;
            _userService = userService;
        }

        [HttpGet("/")]
        public async Task<IActionResult> Timeline(CancellationToken ct)
        {
            ViewData["title"] = "Timeline";

            var messages = await _timelineService.GetFollowerMessagesForUser(User.GetUserID(), ResultsPerPage, ct);
            
            var mapped = messages.Select(message => new TimelineMessageVM(
                new UserVM(
                    message.Author.ID, 
                    message.Author.Username, 
                    message.Author.Email),
                message.Text,
                message.PublishDate
            )).ToList();
            
            var vm = new UserTimelineVM(
                new UserVM(
                    User.GetUserID(), 
                    User.GetUsername(), 
                    User.GetEmail()
                ), 
                false, 
                mapped
            );
            
            return View("Timeline", vm);
        }

        [AllowAnonymous]
        [HttpGet("/public")]
        public async Task<IActionResult> PublicTimeline(CancellationToken ct)
        {
            ViewData["title"] = "Public Timeline";

            var messages = await _timelineService.GetMessagesForAnonymousUser(ResultsPerPage, ct);

            var mapped = messages.Select(message => new TimelineMessageVM(
                new UserVM(
                    message.Author.ID, 
                    message.Author.Username, 
                    message.Author.Email),
                message.Text,
                message.PublishDate
            )).ToList();
            
            var vm = new UserTimelineVM(null, false, mapped);
            
            return View("Timeline", vm);
        }

        [HttpGet("/{username}")]
        public async Task<IActionResult> UserTimeline(string username, CancellationToken ct)
        {
            var author = await _userService.GetUserFromUsername(username, ct);

            if (author == null)
            {
                return NotFound();
            }
            
            var messages = await _timelineService.GetMessagesForUser(author, ResultsPerPage, ct);
            var isUserFollowing = await _userService.IsUserFollowing(User.GetUserID(), username, ct);
            
            var mapped = messages.Select(message => new TimelineMessageVM(
                new UserVM(
                    message.Author.ID,
                    message.Author.Username,
                    message.Author.Email
                ),
                message.Text,
                message.PublishDate
            )).ToList();
            
            var vm = new UserTimelineVM(
                new UserVM(
                    author.ID, 
                    author.Username, 
                    author.Email), 
                isUserFollowing, 
                mapped
            );
            
            ViewData["title"] = $"{author.Username}'s Timeline";

            return View("UserTimeline", vm);
        }

        [HttpGet("/{username}/follow")]
        public async Task<IActionResult> AddFollow(string username, CancellationToken ct)
        {
            await _userService.AddFollower(User.GetUserID(), username, ct);

            ViewData["messages"] = new List<string>
            {
                $"You are now following {username}"
            };

            return RedirectToAction(nameof(Timeline));
        }
        [HttpGet("/{username}/unfollow")]
        public async Task<IActionResult> AddUnfollow(string username, CancellationToken ct)
        {
            await _userService.RemoveFollower(User.GetUserID(), username, ct);

            ViewData["messages"] = new List<string>
            {
                $"You are now unfollowing {username}"
            };

            return RedirectToAction(nameof(Timeline));
        }

        [ValidateAntiForgeryToken]
        [HttpPost("/add_message")]
        public async Task<IActionResult> AddMessage(CreateMessageModel model, CancellationToken ct)
        {
            await _timelineService.CreateMessage(model, User.GetUserID(), ct);
                
            ViewData["messages"] = new List<string>
            {
                "Your message was recorded"
            };

            return RedirectToAction(nameof(Timeline));
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
