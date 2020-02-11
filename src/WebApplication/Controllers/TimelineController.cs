using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication.Entities;
using WebApplication.Extensions;
using WebApplication.Models.Timeline;
using WebApplication.ViewModels;
using WebApplication.ViewModels.Timeline;

namespace WebApplication.Controllers
{
    [Authorize]
    public class TimelineController : Controller
    {
        private const int ResultsPerPage = 30;
        private readonly DatabaseContext _databaseContext;

        public TimelineController(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        }

        [HttpGet("/")]
        public async Task<IActionResult> Timeline(CancellationToken ct)
        {
            ViewData["title"] = "Timeline";
                
            var messages = await _databaseContext.Messages
                .Include(message => message.Author)
                .Where(message => !message.IsFlagged)
                .Where(message => _databaseContext.Followers
                    .Where(f => f.WhoID == User.GetUserID())
                    .Select(f => f.WhomID)
                    .Contains(message.AuthorID))
                .OrderByDescending(message => message.PublishDate)
                .Take(ResultsPerPage)
                .ToListAsync(ct);
            
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

            var all = await _databaseContext.Messages.ToListAsync(ct);
            
            var messages = await  _databaseContext.Messages
                .Include(message => message.Author)
                .Where(message => !message.IsFlagged)
                .OrderByDescending(message => message.PublishDate)
                .Take(ResultsPerPage)
                .ToListAsync(ct);

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
            var author = await _databaseContext.Users
                .Where(u => u.Username == username)    // TODO: Add case insensitive string comparison here if SQLite supports it.
                .FirstOrDefaultAsync(ct);

            if (author == null)
            {
                return NotFound();
            }

            var isUserFollowing =await _databaseContext.Followers
                .AnyAsync(f => f.WhoID == author.ID && f.WhomID == User.GetUserID(), ct);

            var messages = await _databaseContext.Messages
                .Include(message => message.Author)
                .Where(message => message.AuthorID == author.ID)
                .OrderByDescending(message => message.PublishDate)
                .Take(ResultsPerPage)
                .ToListAsync(ct);
            
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

        [ValidateAntiForgeryToken]
        [HttpPost("/add_message")]
        public async Task<IActionResult> AddMessage(CreateMessageModel model, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(model.Text))
            {
                _databaseContext.Messages.Add(new Message
                {
                    AuthorID = User.GetUserID(),
                    Text = model.Text.Trim(),
                    PublishDate = DateTimeOffset.Now,
                    IsFlagged = false
                });

                await _databaseContext.SaveChangesAsync(ct);
                
                ViewData["messages"] = new List<string>
                {
                    "Your message was recorded"
                };
            }

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
