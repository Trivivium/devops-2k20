using System.Linq;
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
        public async Task<IActionResult> Timeline()
        {
            ViewData["title"] = "Timeline";

            var messages = await _timelineService.GetFollowerMessagesForUser(User.GetUserID(), ResultsPerPage);
            
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
        public async Task<IActionResult> PublicTimeline()
        {
            ViewData["title"] = "Public Timeline";

            var includeFlaggedMessages = User.IsInRole(AuthRoles.Administrator);
            
            var messages = await _timelineService.GetMessagesForAnonymousUser(ResultsPerPage, includeFlaggedMessages);

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
        public async Task<IActionResult> UserTimeline(string username)
        {
            try
            {
                var author = await _userService.GetUserFromUsername(username);

                if (author == null)
                {
                    return NotFound();
                }
                
                var includeFlaggedMessages = User.IsInRole(AuthRoles.Administrator);
                var messages = await _timelineService.GetMessagesForUser(author.Username, ResultsPerPage, includeFlaggedMessages);
                var isUserFollowing = await _userService.IsUserFollowing(User.GetUserID(), username);
            
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

                var vm = new UserTimelineVM(
                    new UserVM(
                        author.ID,
                        author.Username,
                        author.Email),
                    isUserFollowing,
                    mapped
                );
                
                ViewData["title"] = $"{author.Username}'s Timeline";

                return View(nameof(UserTimeline), vm);
            }
            catch (UnknownUserException e)
            {
                return BadRequest(new ErrorResponse(e));
            }
        }

        [HttpGet("/{username}/follow")]
        public async Task<IActionResult> AddFollow(string username)
        {
            try
            {
                await _userService.AddFollower(User.GetUserID(), username);
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
        public async Task<IActionResult> AddUnfollow(string username)
        {
            try
            {
                await _userService.RemoveFollower(User.GetUserID(), username);
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
        public async Task<IActionResult> AddMessage(CreateMessageModel model)
        {
            try
            {
                await _timelineService.CreateMessage(model, User.GetUserID());
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
        public async Task<IActionResult> AddFlagToMessage(int id)
        {
            if (id < 1)
            {
                return BadRequest(new ErrorResponse("The provided message ID cannot be less than 1."));
            }
            
            try
            {
                await _timelineService.AddFlagToMessage(id);
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
        public async Task<IActionResult> RemoveFlagFromMessage(int id)
        {
            if (id < 1)
            {
                return BadRequest(new ErrorResponse("The provided message ID cannot be less than 1."));
            }
            
            try
            {
                await _timelineService.RemoveFlagFromMessage(id);
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

        [AllowAnonymous]
        public IActionResult Error()
        {
            ViewData["title"] = "Error";
            

            return View();
        }
    }
}
