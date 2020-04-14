using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Bcr = BCrypt.Net;

using WebApplication.Entities;
using WebApplication.Exceptions;
using WebApplication.Models.Authentication;

namespace WebApplication.Services
{
    public class UserService
    {
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<UserService> _logger;

        public UserService(DatabaseContext databaseContext, ILogger<UserService> logger)
        {
            _databaseContext = databaseContext;
            _logger = logger;
        }

        public async Task CreateUser(RegisterModel model)
        {
            var usernameExists = await _databaseContext.Users.AnyAsync(item => item.Username == model.Username);
            var emailExists = await _databaseContext.Users.AnyAsync(item => item.Email == model.Email);
            
            if (usernameExists) 
            {
                throw new CreateUserException("The provided username is already in use");
            }

            if (emailExists)
            {
                throw new CreateUserException("The provided email is already in use");
            }
            
            _databaseContext.Add(new User
            {
                Email = model.Email, 
                Password = Bcr.BCrypt.HashPassword(model.Pwd), 
                Username = model.Username
            });
            
            await _databaseContext.SaveChangesAsync();
            
            _logger.LogInformation($"Created user with username: {model.Username}.");
        }
        
        public async Task<User> GetUserFromUsername(string username)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(p => p.Username == username);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with username: {username}");
            }

            return user;
        }

        public async Task<bool> IsUserFollowing(int userID, string followerUsernameToCheck)
        {
            return await _databaseContext.Followers
                .AnyAsync(f => f.Who.Username == followerUsernameToCheck && f.WhomID == userID);
        }

        public async Task<List<Follower>> GetUserFollowers(string username, int resultsPerPage)
        {
            var user = await _databaseContext.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with username: {username}");
            }
            
            var followers = await _databaseContext.Followers
                .Include(f => f.Who)
                .Where(f => f.WhomID == user.ID)
                .Take(resultsPerPage)
                .ToListAsync();

            return followers;
        }
        
        public async Task AddFollower(string whoUsername, string whomUsername)
        {
            var who = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == whoUsername);

            if (who == null)
            {
                throw new UnknownUserException($"Unknown user to follow: {whoUsername}.");
            }
            
            await AddFollower(who.ID, whomUsername);
            
            _logger.LogInformation($"Added user with username '{whoUsername}' as follower of: {whomUsername}.");
        }
        
        public async Task AddFollower(int whoID, string whomUsername)
        {
            var whom = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == whomUsername);

            if (whom == null)
            {
                throw new UnknownUserException($"Unknown user to follow: {whomUsername}.");
            }
            
            _databaseContext.Followers.Add(new Follower
            {
                WhomID = whoID,
                WhoID = whom.ID
            });

            await _databaseContext.SaveChangesAsync();
            
            _logger.LogInformation($"Added user with user ID '{whoID}' as follower of: {whom.Username}.");
        }
        
        public async Task RemoveFollower(string usernameToUnfollow, string unfollowerUsername)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == usernameToUnfollow);
            var userToRemove = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == unfollowerUsername);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user to unfollow: {usernameToUnfollow}.");
            }

            if (userToRemove == null)
            {
                throw new UnknownUserException($"Unknown user to remove as follower: {unfollowerUsername}.");
            }
            
            var entry = await _databaseContext.Followers
                .Where(p => p.WhoID == userToRemove.ID)
                .Where(p => p.WhomID == user.ID)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                throw new UnknownFollowerRelationException(user.ID, userToRemove.ID);
            }
            
            _databaseContext.Followers.Remove(entry);
            
            await _databaseContext.SaveChangesAsync();
            
            _logger.LogInformation($"Removed user with username '{userToRemove.Username}' as a follower of: {user.Username}.");
        }
        
        public async Task RemoveFollower(int whoID, string whomUsername)
        {
            var whom = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == whomUsername);
            
            if (whom == null)
            {
                throw new UnknownUserException($"Unknown user to unfollow: {whomUsername}.");
            }
            
            var entry = await _databaseContext.Followers
                .Where(p => p.WhoID == whoID)
                .Where(p => p.WhomID == whom.ID)
                .FirstOrDefaultAsync();

            if (entry == null)
            {
                throw new UnknownFollowerRelationException(whom.ID, whoID);
            }

            _databaseContext.Followers.Remove(entry);

            await _databaseContext.SaveChangesAsync();
            
            _logger.LogInformation($"Removed user with user ID '{whoID}' as a follower of: {whom.Username}.");
        }
    }
}
