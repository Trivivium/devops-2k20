using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Bcr = BCrypt.Net;

using WebApplication.Entities;
using WebApplication.Exceptions;
using WebApplication.Models.Authentication;

namespace WebApplication.Services
{
    public class UserService
    {
        private readonly DatabaseContext _databaseContext;

        public UserService(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task CreateUser(RegisterModel model, CancellationToken ct)
        {
            var usernameExists = await _databaseContext.Users.AnyAsync(item => item.Username == model.Username, ct);
            var emailExists = await _databaseContext.Users.AnyAsync(item => item.Email == model.Email, ct);
            
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
            
            await _databaseContext.SaveChangesAsync(ct);
        }
        
        public async Task<User> GetUserFromUsername(string username, CancellationToken ct)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(p => p.Username == username, ct);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with username: {username}");
            }

            return user;
        }

        public async Task<bool> IsUserFollowing(int userID, string followerUsernameToCheck, CancellationToken ct)
        {
            return await _databaseContext.Followers
                .AnyAsync(f => f.Who.Username == followerUsernameToCheck && f.WhomID == userID, ct);
        }

        public async Task<List<Follower>> GetUserFollowers(string username, int resultsPerPage, CancellationToken ct)
        {
            var user = await _databaseContext.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync(ct);

            if (user == null)
            {
                throw new UnknownUserException($"Unknown user with username: {username}");
            }
            
            var followers = await _databaseContext.Followers
                .Include(f => f.Who)
                .Where(f => f.WhomID == user.ID)
                .Take(resultsPerPage)
                .ToListAsync(ct);

            return followers;
        }
        
        public async Task AddFollower(string whoUsername, string whomUsername, CancellationToken ct)
        {
            var who = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == whoUsername, ct);

            if (who == null)
            {
                throw new UnknownUserException($"Unknown user to follow: {whoUsername}.");
            }
            
            await AddFollower(who.ID, whomUsername, ct);
        }
        
        public async Task AddFollower(int whoID, string whomUsername, CancellationToken ct)
        {
            var whom = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == whomUsername, ct);

            if (whom == null)
            {
                throw new UnknownUserException($"Unknown user to follow: {whomUsername}.");
            }
            
            _databaseContext.Followers.Add(new Follower
            {
                WhomID = whoID,
                WhoID = whom.ID
            });

            await _databaseContext.SaveChangesAsync(ct);
        }
        
        public async Task RemoveFollower(string usernameToUnfollow, string unfollowerUsername, CancellationToken ct)
        {
            var user = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == usernameToUnfollow, ct);
            var userToRemove = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == unfollowerUsername, ct);

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
                .FirstOrDefaultAsync(ct);

            if (entry == null)
            {
                throw new UnknownFollowerRelationException(user.ID, userToRemove.ID);
            }
            
            _databaseContext.Followers.Remove(entry);
            
            await _databaseContext.SaveChangesAsync(ct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="whoID">Who follows</param>
        /// <param name="whomUsername">Whom is followed</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        /// <exception cref="UnknownUserException"></exception>
        /// <exception cref="UnknownFollowerRelationException"></exception>
        public async Task RemoveFollower(int whoID, string whomUsername, CancellationToken ct)
        {
            var whom = await _databaseContext.Users.FirstOrDefaultAsync(u => u.Username == whomUsername, ct);
            
            if (whom == null)
            {
                throw new UnknownUserException($"Unknown user to unfollow: {whomUsername}.");
            }
            
            var entry = await _databaseContext.Followers
                .Where(p => p.WhoID == whoID)
                .Where(p => p.WhomID == whom.ID)
                .FirstOrDefaultAsync(ct);

            if (entry == null)
            {
                throw new UnknownFollowerRelationException(whom.ID, whoID);
            }

            _databaseContext.Followers.Remove(entry);

            await _databaseContext.SaveChangesAsync(ct);
        }
    }
}
