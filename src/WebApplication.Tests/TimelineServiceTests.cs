using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using WebApplication.Entities;
using WebApplication.Exceptions;
using WebApplication.Models.Timeline;
using WebApplication.Services;

using Xunit;

namespace WebApplication.Tests
{
    public class TimelineServiceTests
    {
        private static UserService CreateUserService(DatabaseContext dbContext)
        {
            return new UserService(dbContext, Mock.Of<ILogger<UserService>>());
        }

        private static TimelineService CreateTimelineService(DatabaseContext dbContext, UserService userService = null)
        {
            return new TimelineService(dbContext, userService ?? CreateUserService(dbContext), Mock.Of<ILogger<TimelineService>>());
        }
        
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
                var service = CreateTimelineService(dbContext);
                var messages = await service.GetMessagesForAnonymousUser(30, false, CancellationToken.None);
                
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
                var service = CreateTimelineService(dbContext);
                var messages = await service.GetMessagesForAnonymousUser(30, false, CancellationToken.None);
                
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
                var userService = CreateUserService(dbContext);
                var service = CreateTimelineService(dbContext, userService);

                var user = await userService.GetUserFromUsername("a", CancellationToken.None);
                var messages = await service.GetMessagesForUser(user.Username, 30, false, CancellationToken.None);
                
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
                var userService = CreateUserService(dbContext);
                var service = CreateTimelineService(dbContext, userService);

                var user = await userService.GetUserFromUsername("a", CancellationToken.None);
                var messages = await service.GetMessagesForUser(user.Username, 30, false, CancellationToken.None);
                
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
                var service = CreateTimelineService(dbContext);
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
            
            await Assert.ThrowsAsync<UnknownUserException>(async () => {
                await using (var dbContext = new DatabaseContext(options))
                {
                    var service = CreateTimelineService(dbContext);
                    var messages = await service.GetFollowerMessagesForUser("b", 30, CancellationToken.None);
                
                    Assert.Empty(messages);
                }
            });
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
                var service = CreateTimelineService(dbContext);
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
                var service = CreateTimelineService(dbContext);
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
                var service = CreateTimelineService(dbContext);
                var model = new CreateMessageModel
                {
                    Content = "123"
                };

                await Assert.ThrowsAsync<UnknownUserException>(async () => {
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
                var service = CreateTimelineService(dbContext);
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

        [Fact]
        public async Task AddFlagToMessage_Works()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(AddFlagToMessage_Works))
                .Options;
            
            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });
                dbContext.Messages.Add(new Message { AuthorID = 1, IsFlagged = false, Text = "test", PublishDate = DateTimeOffset.UtcNow});

                dbContext.SaveChanges();
            }
            
            await using (var dbContext = new DatabaseContext(options))
            {
                var service = CreateTimelineService(dbContext);

                await service.AddFlagToMessage(1, CancellationToken.None);
            }
            
            await using (var dbContext = new DatabaseContext(options))
            {
                var message = dbContext.Messages.Single(m => m.ID == 1);

                Assert.True(message.IsFlagged);
            }
        }
        
        [Fact]
        public async Task AddFlagToMessage_Throws_WhenMessageDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(AddFlagToMessage_Throws_WhenMessageDoesNotExist))
                .Options;

            await Assert.ThrowsAsync<UnknownMessageException>(async () => 
            {
                await using (var dbContext = new DatabaseContext(options))
                {
                    var service = CreateTimelineService(dbContext);

                    await service.AddFlagToMessage(1, CancellationToken.None);
                }
            });
        }
        
        [Fact]
        public async Task RemoveFlagFromMessage_Works()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(RemoveFlagFromMessage_Works))
                .Options;
            
            await using (var dbContext = new DatabaseContext(options))
            {
                dbContext.Users.Add(new User { Username = "a", Email = "a@a.a", Password = "a" });
                dbContext.Messages.Add(new Message { AuthorID = 1, IsFlagged = true, Text = "test", PublishDate = DateTimeOffset.UtcNow});

                dbContext.SaveChanges();
            }
            
            await using (var dbContext = new DatabaseContext(options))
            {
                var service = CreateTimelineService(dbContext);

                await service.RemoveFlagFromMessage(1, CancellationToken.None);
            }
            
            await using (var dbContext = new DatabaseContext(options))
            {
                var message = dbContext.Messages.Single(m => m.ID == 1);

                Assert.False(message.IsFlagged);
            }
        }
        
        [Fact]
        public async Task RemoveFlagFromMessage_Throws_WhenMessageDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<DatabaseContext>()
                .UseInMemoryDatabase(nameof(RemoveFlagFromMessage_Throws_WhenMessageDoesNotExist))
                .Options;
            
            await Assert.ThrowsAsync<UnknownMessageException>(async () => 
            {
                await using (var dbContext = new DatabaseContext(options))
                {
                    var service = CreateTimelineService(dbContext);

                    await service.RemoveFlagFromMessage(1, CancellationToken.None);
                }
            });
        }
    }
}
