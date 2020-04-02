using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using WebApplication.Entities;
using WebApplication.Exceptions;
using WebApplication.Models.Timeline;

namespace WebApplication.Services
{
    public class TimelineService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly UserService _userService;

        public TimelineService(DatabaseContext dbContext, UserService userService)
        {
            _databaseContext = dbContext;
            _userService = userService;
        }

        public async Task<List<Message>> GetMessagesForAnonymousUser(int resultsPerPage, bool includeFlaggedMessages, CancellationToken ct)
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
                .ToListAsync(ct);

            return messages;
        }

        public async Task<List<Message>> GetMessagesForUser(string username, int resultsPerPage, bool includeFlaggedMessages, CancellationToken ct)
        {
            var author = await _userService.GetUserFromUsername(username, ct);
            
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
                .ToListAsync(ct);

            return messages;
        }

        public async Task<List<Message>> GetFollowerMessagesForUser(string username, int resultsPerPage, CancellationToken ct)
        {
            var user = await _userService.GetUserFromUsername(username, ct);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with username: {username}");
            }
            
            return await GetFollowerMessagesForUser(user.ID, resultsPerPage, ct);
        }

        public async Task<List<Message>> GetFollowerMessagesForUser(int userID, int resultsPerPage, CancellationToken ct)
        {
            var userExists = await _databaseContext.Users.AnyAsync(user => user.ID == userID, ct);

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
                .ToListAsync(ct);

            return messages;
        }

        public async Task CreateMessage(CreateMessageModel model, string username, CancellationToken ct)
        {
            var user = await _userService.GetUserFromUsername(username, ct);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with username: {username}");
            }
            
            await CreateMessage(model, user.ID, ct);
        }
        
        public async Task CreateMessage(CreateMessageModel model, int userID, CancellationToken ct)
        {
            var user = await _databaseContext.Users.SingleOrDefaultAsync(row => row.ID == userID, ct);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with ID: {userID}.");
            }
            
            await CreateMessage(model, user, ct);
        }
        
        private async Task CreateMessage(CreateMessageModel model, User user, CancellationToken ct)
        {
            _databaseContext.Messages.Add(new Message
            {
                AuthorID = user.ID,
                Text = model.Content.Trim(),
                PublishDate = DateTimeOffset.Now,
                IsFlagged = false
            });

            await _databaseContext.SaveChangesAsync(ct);
        }

        public async Task AddFlagToMessage(int id, CancellationToken ct)
        {
            var message = await _databaseContext.Messages.FirstOrDefaultAsync(m => m.ID == id, ct);

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

            await _databaseContext.SaveChangesAsync(ct);
            
            // TODO: Add logging of the action was successful, when the 'feature/audit-logging' branch has been merged into master. Remember to include the user ID that performed the action.
        }
        
        public async Task RemoveFlagFromMessage(int id, CancellationToken ct)
        {
            var message = await _databaseContext.Messages.FirstOrDefaultAsync(m => m.ID == id, ct);

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

            await _databaseContext.SaveChangesAsync(ct);
            
            // TODO: Add logging of the action was successful, when the 'feature/audit-logging' branch has been merged into master. Remember to include the user ID that performed the action.
        }
    }
}
