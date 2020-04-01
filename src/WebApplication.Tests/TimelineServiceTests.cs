using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using WebApplication.Entities;
using WebApplication.Models.Timeline;
using WebApplication.Services;

using Xunit;

namespace WebApplication.Tests
{
    public class TimelineServiceTests
    {
        [Fact]
        public async Task GetMessagesForAnonymousUser_Works()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(GetMessagesForAnonymousUser_Works))
                .Options;

            // Seed the database
            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });
                
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "abc", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "123", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });

                dbContext.SaveChanges();
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var service = new TimelineService(dbContext, new UserService(dbContext));
                var messages = await service.GetMessagesForAnonymousUser(30, CancellationToken.None);
                
                Assert.Equal(2, messages.Count);
            }
        }
        
        [Fact]
        public async Task GetMessagesForAnonymousUser_IgnoresFlaggedMessages()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(GetMessagesForAnonymousUser_IgnoresFlaggedMessages))
                .Options;

            // Seed the database
            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });
                
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "abc", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "123", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "bad", PublishDate = DateTimeOffset.UtcNow, IsFlagged = true });

                dbContext.SaveChanges();
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var service = new TimelineService(dbContext, new UserService(dbContext));
                var messages = await service.GetMessagesForAnonymousUser(30, CancellationToken.None);
                
                Assert.Equal(2, messages.Count);
            }
        }
        
        [Fact]
        public async Task GetMessagesForUser_Works()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(GetMessagesForUser_Works))
                .Options;

            // Seed the database
            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });
                dbContext.Users.Add(new User { Username = "b", Email = "b@b.b", Password = "b" });
                
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "abc", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "123", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 2, Text = "should not be included in results", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                
                dbContext.SaveChanges();
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var userService = new UserService(dbContext);
                var service = new TimelineService(dbContext, userService);

                var user = await userService.GetUserFromUsername("a", CancellationToken.None);
                var messages = await service.GetMessagesForUser(user.Username, 30, CancellationToken.None);
                
                Assert.Equal(2, messages.Count);
            }
        }
        
        [Fact]
        public async Task GetMessagesForUser_IgnoresFlaggedMessages()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(GetMessagesForUser_IgnoresFlaggedMessages))
                .Options;

            // Seed the database
            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });
                dbContext.Users.Add(new User { Username = "b", Email = "b@b.b", Password = "b" });
                
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "abc", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "123", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "bad", PublishDate = DateTimeOffset.UtcNow, IsFlagged = true });
                dbContext.Messages.Add(new Message { AuthorID = 2, Text = "should not be included in results", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                
                dbContext.SaveChanges();
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var userService = new UserService(dbContext);
                var service = new TimelineService(dbContext, userService);

                var user = await userService.GetUserFromUsername("a", CancellationToken.None);
                var messages = await service.GetMessagesForUser(user.Username, 30, CancellationToken.None);
                
                Assert.Equal(2, messages.Count);
            }
        }
        
        [Fact]
        public async Task GetFollowerMessagesForUser_UsingUsername_Works()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(GetFollowerMessagesForUser_UsingUsername_Works))
                .Options;

            // Seed the database
            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });
                dbContext.Users.Add(new User { Username = "b", Email = "b@b.b", Password = "b" });
                
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "abc", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "123", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 2, Text = "should not be included in results", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });

                dbContext.Followers.Add(new Follower { WhoID = 2, WhomID = 1 });
                
                dbContext.SaveChanges();
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var service = new TimelineService(dbContext, new UserService(dbContext));
                var messages = await service.GetFollowerMessagesForUser("b", 30, CancellationToken.None);
                
                Assert.Equal(2, messages.Count);
            }
        }
        
        [Fact]
        public async Task GetFollowerMessagesForUser_UsingUsername_ReturnsEmptyCollection_WhenUserDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(GetFollowerMessagesForUser_UsingUsername_ReturnsEmptyCollection_WhenUserDoesNotExist))
                .Options;

            await using (var dbContext = new DatabaseContext(options))
            {
                var service = new TimelineService(dbContext, new UserService(dbContext));
                var messages = await service.GetFollowerMessagesForUser("b", 30, CancellationToken.None);
                
                Assert.Empty(messages);
            }
        }
        
        [Fact]
        public async Task GetFollowerMessagesForUser_UsingUserID_Works()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(GetFollowerMessagesForUser_UsingUserID_Works))
                .Options;

            // Seed the database
            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });
                dbContext.Users.Add(new User { Username = "b", Email = "b@b.b", Password = "b" });
                
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "abc", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 1, Text = "123", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });
                dbContext.Messages.Add(new Message { AuthorID = 2, Text = "should not be included in results", PublishDate = DateTimeOffset.UtcNow, IsFlagged = false });

                dbContext.Followers.Add(new Follower { WhoID = 2, WhomID = 1 });
                
                dbContext.SaveChanges();
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var service = new TimelineService(dbContext, new UserService(dbContext));
                var messages = await service.GetFollowerMessagesForUser(2, 30, CancellationToken.None);
                
                Assert.Equal(2, messages.Count);
            }
        }

        [Fact]
        public async Task CreateMessage_UsingUsername_Works()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(CreateMessage_UsingUsername_Works))
                .Options;

            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });

                dbContext.SaveChanges();
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var service = new TimelineService(dbContext, new UserService(dbContext));
                var model = new CreateMessageModel
                {
                    Content = "123"
                };

                await service.CreateMessage(model, "a", CancellationToken.None);
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var messages = dbContext.Messages.Where(message => message.AuthorID == 1).ToList();

                Assert.Single(messages);
            }
        }
        
        [Fact]
        public async Task CreateMessage_UsingUsername_Throws_WhenUserDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(CreateMessage_UsingUsername_Throws_WhenUserDoesNotExist))
                .Options;
            
            await using (var dbContext = new DatabaseContext(options))
            {
                var service = new TimelineService(dbContext, new UserService(dbContext));
                var model = new CreateMessageModel
                {
                    Content = "123"
                };

                await Assert.ThrowsAsync<InvalidOperationException>(async () => {
                    await service.CreateMessage(model, "a", CancellationToken.None);
                });
            }
        }
        
        [Fact]
        public async Task CreateMessage_UsingUserID_Works()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(CreateMessage_UsingUserID_Works))
                .Options;

            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });

                dbContext.SaveChanges();
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var service = new TimelineService(dbContext, new UserService(dbContext));
                var model = new CreateMessageModel
                {
                    Content = "123"
                };

                await service.CreateMessage(model, 1, CancellationToken.None);
            }

            await using (var dbContext = new DatabaseContext(options))
            {
                var messages = dbContext.Messages.Where(message => message.AuthorID == 1).ToList();

                Assert.Single(messages);
            }
        }
    }
}
