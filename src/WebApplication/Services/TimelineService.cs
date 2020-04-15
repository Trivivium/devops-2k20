using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using WebApplication.Entities;
using WebApplication.Exceptions;
using WebApplication.Models.Timeline;

namespace WebApplication.Services
{
    public class TimelineService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly UserService _userService;
        private readonly ILogger<TimelineService> _logger;

        public TimelineService(DatabaseContext dbContext, UserService userService, ILogger<TimelineService> logger)
        {
            _databaseContext = dbContext;
            _userService = userService;
            _logger = logger;
        }

        public async Task<List<Message>> GetMessagesForAnonymousUser(int resultsPerPage, bool includeFlaggedMessages)
        {
            IQueryable<Message> query = _databaseContext.Messages
                .Include(message => message.Author);

            if (!includeFlaggedMessages)
            {
                query = query.Where(message => !message.IsFlagged);
            }
            
            var messages = await query
                .OrderByDescending(message => message.PublishDate)
                .Take(resultsPerPage)
                .ToListAsync();

            return messages;
        }

        public async Task<List<Message>> GetMessagesForUser(string username, int resultsPerPage, bool includeFlaggedMessages)
        {
            var author = await _userService.GetUserFromUsername(username);
            
            IQueryable<Message> query = _databaseContext.Messages
                .Include(message => message.Author);

            if (!includeFlaggedMessages)
            {
                query = query.Where(message => !message.IsFlagged);
            }
                
            var messages = await query
                .Where(message => message.AuthorID == author.ID)
                .OrderByDescending(message => message.PublishDate)
                .Take(resultsPerPage)
                .ToListAsync();

            return messages;
        }

        public async Task<List<Message>> GetFollowerMessagesForUser(string username, int resultsPerPage)
        {
            var user = await _userService.GetUserFromUsername(username);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with username: {username}");
            }
            
            return await GetFollowerMessagesForUser(user.ID, resultsPerPage);
        }

        public async Task<List<Message>> GetFollowerMessagesForUser(int userID, int resultsPerPage)
        {
            var userExists = await _databaseContext.Users.AnyAsync(user => user.ID == userID);

            if (!userExists)
            {
                throw new UnknownUserException($"Unknown user with ID: {userID}.");
            }
            
            var messages = await _databaseContext.Messages
                .Include(message => message.Author)
                .Where(message => !message.IsFlagged)
                .Where(message => _databaseContext.Followers
                    .Where(f => f.WhoID == userID)
                    .Select(f => f.WhomID)
                    .Contains(message.AuthorID))
                .OrderByDescending(message => message.PublishDate)
                .Take(resultsPerPage)
                .ToListAsync();

            return messages;
        }

        public async Task CreateMessage(CreateMessageModel model, string username)
        {
            var user = await _userService.GetUserFromUsername(username);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with username: {username}");
            }
            
            await CreateMessage(model, user.ID);
        }
        
        public async Task CreateMessage(CreateMessageModel model, int userID)
        {
            var user = await _databaseContext.Users.SingleOrDefaultAsync(row => row.ID == userID);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with ID: {userID}.");
            }
            
            await CreateMessage(model, user);
        }
        
        private async Task CreateMessage(CreateMessageModel model, User user)
        {
            _databaseContext.Messages.Add(new Message
            {
                AuthorID = user.ID,
                Text = model.Content.Trim(),
                PublishDate = DateTimeOffset.Now,
                IsFlagged = false
            });

            await _databaseContext.SaveChangesAsync();
            
            _logger.LogInformation($"Created message for user: {user.Username}.");
        }

        public async Task AddFlagToMessage(int id)
        {
            var message = await _databaseContext.Messages.FirstOrDefaultAsync(m => m.ID == id);

            if (message == null)
            {
                throw new UnknownMessageException($"Unknown message with ID: {id}.");
            }

            // If the message is already flagged we can return early to avoid unnecessary database calls.
            if (message.IsFlagged)
            {
                return;
            }

            message.IsFlagged = true;

            await _databaseContext.SaveChangesAsync();
            
            // TODO: Add logging of the action was successful, when the 'feature/audit-logging' branch has been merged into master. Remember to include the user ID that performed the action.
        }
        
        public async Task RemoveFlagFromMessage(int id)
        {
            var message = await _databaseContext.Messages.FirstOrDefaultAsync(m => m.ID == id);

            if (message == null)
            {
                throw new UnknownMessageException($"Unknown message with ID: {id}.");
            }

            // If the message is not flagged we can return early to avoid unnecessary database calls.
            if (!message.IsFlagged)
            {
                return;
            }

            message.IsFlagged = false;

            await _databaseContext.SaveChangesAsync();
            
            // TODO: Add logging of the action was successful, when the 'feature/audit-logging' branch has been merged into master. Remember to include the user ID that performed the action.
        }
    }
}
