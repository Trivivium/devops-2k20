using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using WebApplication.Auth;
using WebApplication.Exceptions;
using WebApplication.Services;
using WebApplication.Extensions;
using WebApplication.Models.Timeline;
using WebApplication.ResponseModels;
using WebApplication.ViewModels;
using WebApplication.ViewModels.Timeline;

namespace WebApplication.Controllers
{
    [Authorize(Policy = AuthPolicies.Registered)]
    [ApiExplorerSettings(IgnoreApi = true)]
    [ActionLogger]
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
                message.ID,
                new UserVM(
                    message.Author.ID, 
                    message.Author.Username, 
                    message.Author.Email),
                message.Text,
                message.PublishDate,
                message.IsFlagged
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

            var includeFlaggedMessages = User.IsInRole(AuthRoles.Administrator);
            
            var messages = await _timelineService.GetMessagesForAnonymousUser(ResultsPerPage, includeFlaggedMessages, ct);

            var mapped = messages.Select(message => new TimelineMessageVM(
                message.ID,
                new UserVM(
                    message.Author.ID, 
                    message.Author.Username, 
                    message.Author.Email),
                message.Text,
                message.PublishDate,
                message.IsFlagged
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

            UserTimelineVM vm;

            try
            {
                var includeFlaggedMessages = User.IsInRole(AuthRoles.Administrator);
                var messages = await _timelineService.GetMessagesForUser(author.Username, ResultsPerPage, includeFlaggedMessages, ct);
                var isUserFollowing = await _userService.IsUserFollowing(User.GetUserID(), username, ct);
            
                var mapped = messages.Select(message => new TimelineMessageVM(
                    message.ID,
                    new UserVM(
                        message.Author.ID,
                        message.Author.Username,
                        message.Author.Email
                    ),
                    message.Text,
                    message.PublishDate,
                    message.IsFlagged
                )).ToList();

                vm = new UserTimelineVM(
                    new UserVM(
                        author.ID,
                        author.Username,
                        author.Email),
                    isUserFollowing,
                    mapped
                );
            }
            catch (UnknownUserException e)
            {
                return BadRequest(new ErrorResponse(e));
            }
            
            ViewData["title"] = $"{author.Username}'s Timeline";

            return View(nameof(UserTimeline), vm);
        }

        [HttpGet("/{username}/follow")]
        public async Task<IActionResult> AddFollow(string username, CancellationToken ct)
        {
            try
            {
                await _userService.AddFollower(User.GetUserID(), username, ct);
            }
            catch (UnknownUserException e)
            {
                return BadRequest(new ErrorResponse(e));
            }

            ViewData["messages"] = new List<string>
            {
                $"You are now following {username}"
            };

            return RedirectToAction(nameof(Timeline));
        }
        [HttpGet("/{username}/unfollow")]
        public async Task<IActionResult> AddUnfollow(string username, CancellationToken ct)
        {
            try
            {
                await _userService.RemoveFollower(User.GetUserID(), username, ct);
            }
            catch (UnknownUserException e)
            {
                return BadRequest(new ErrorResponse(e));
            }

            ViewData["messages"] = new List<string>
            {
                $"You are no longer following {username}"
            };

            return RedirectToAction(nameof(Timeline));
        }

        [ValidateAntiForgeryToken]
        [HttpPost("/add_message")]
        public async Task<IActionResult> AddMessage(CreateMessageModel model, CancellationToken ct)
        {
            try
            {
                await _timelineService.CreateMessage(model, User.GetUserID(), ct);
            }
            catch (UnknownUserException e)
            {
                return BadRequest(new ErrorResponse(e));
            }
                
            ViewData["messages"] = new List<string>
            {
                "Your message was recorded"
            };

            return RedirectToAction(nameof(Timeline));
        }

        [HttpPost("flag/{id:int}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.Administrator)]
        public async Task<IActionResult> AddFlagToMessage(int id, CancellationToken ct)
        {
            if (id < 1)
            {
                return BadRequest(new ErrorResponse("The provided message ID cannot be less than 1."));
            }
            
            try
            {
                await _timelineService.AddFlagToMessage(id, ct);
            }
            catch (UnknownMessageException exception)
            {
                // TODO: Add logging of the exception, when the 'feature/audit-logging' branch has been merged into master
                
                return BadRequest(new ErrorResponse(exception));
            }
            
            ViewData["messages"] = new List<string>
            {
                "The message was successfully flagged and hidden from non-administrator users."
            };

            return RedirectToAction(nameof(Timeline));
        }
        
        [HttpPost("unflag/{id:int}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = AuthPolicies.Administrator)]
        public async Task<IActionResult> RemoveFlagFromMessage(int id, CancellationToken ct)
        {
            if (id < 1)
            {
                return BadRequest(new ErrorResponse("The provided message ID cannot be less than 1."));
            }
            
            try
            {
                await _timelineService.RemoveFlagFromMessage(id, ct);
            }
            catch (UnknownMessageException exception)
            {
                // TODO: Add logging of the exception, when the 'feature/audit-logging' branch has been merged into master
                
                return BadRequest(new ErrorResponse(exception));
            }
            
            ViewData["messages"] = new List<string>
            {
                "The message was successfully been un-flagged and is now visible to all users."
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
