using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using WebApplication.Entities;
using WebApplication.Exceptions;
using WebApplication.Helpers;
using WebApplication.Models.Api;
using WebApplication.Models.Authentication;
using WebApplication.Models.Timeline;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly DatabaseContext _databaseContext;
        private readonly TimelineService _timelineService;
        private readonly UserService _userService;

        public ApiController(DatabaseContext databaseContext, TimelineService timelineService, UserService userService)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _timelineService = timelineService;
            _userService = userService;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest(CancellationToken ct)
        {
            var latest = await TestingUtils.GetLatest(_databaseContext, ct);
            
            return Ok(latest);
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> AddUser(RegisterModel model, CancellationToken ct)    // TODO Add request model
        {
            try
            {
                await _userService.CreateUser(model, ct);
            }
            catch (CreateUserException exception)
            {
                return BadRequest(new
                {
                    status = 400,
                    error_msg = exception.Message
                });
            }

            return NoContent();
        }
        
        [HttpGet("msgs")]
        public async Task<IActionResult> GetMessages([FromQuery] int no = 20, CancellationToken ct = default)
        {
            var messages = await _timelineService.GetMessagesForAnonymousUser(no, ct);

            return Ok(messages.Select(msg => new
            {
                content = msg.Text,
                pub_date = msg.PublishDate,
                user = msg.Author.Username
            }));
        }
        
        [HttpGet("msgs/{username}")]
        public async Task<IActionResult> GetMessagesFromUser(string username, [FromQuery] int no = 20, CancellationToken ct = default)
        {
            var user = await _userService.GetUserFromUsername(username, ct);
            var messages = await _timelineService.GetMessagesForUser(user, no, ct);

            return Ok(messages.Select(msg => new
            {
                content = msg.Text,
                pub_date = msg.PublishDate,
                user = msg.Author.Username
            }));
        }
        
        [HttpPost("msgs/{username}")]
        public async Task<IActionResult> AddMessageToUser(string username, CreateMessageModel model, CancellationToken ct)
        {
            await _timelineService.CreateMessage(model, username, ct);

            return NoContent();
        }
        
        [HttpGet("fllws/{username}")]
        public async Task<IActionResult> GetFollowersFromUser(string username, [FromQuery] int no = 20, CancellationToken ct = default)
        {
            var followers = await _userService.GetUserFollowers(username, no, ct);

            return Ok(new
            {
                follows = followers.Select(f => f.Who.Username)
            });
        }
        
        [HttpPost("fllws/{username}")]
        public async Task<IActionResult> AddOrRemoveFollowerFromUser(string username, ChangeUserFollowerModel model,  CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(model.Follow) && string.IsNullOrWhiteSpace(model.Unfollow))
            {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(model.Follow))
            {
                await _userService.AddFollower(username, model.Follow, ct);
            }
            else
            {
                await _userService.RemoveFollower(username, model.Unfollow, ct);
            }

            return NoContent();
        }
    }
}
