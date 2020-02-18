using System;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.Entities
{
    public class DatabaseContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Follower> Followers { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Latest> Latests { get; set; }

        public DatabaseContext(DbContextOptions options) : base(options)
        {}

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            base.OnConfiguring(options);

            options.UseSqlite("Data Source=minitwit.db");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var users = builder.Entity<User>().ToTable("user");
            var followers = builder.Entity<Follower>().ToTable("follower");
            var messages = builder.Entity<Message>().ToTable("message");
            var latests = builder.Entity<Latest>().ToTable("latest");

            users.HasKey(user => user.ID);
            users.Property(user => user.ID).HasColumnName("user_id").ValueGeneratedOnAdd();
            users.Property(user => user.Username).HasColumnName("username");
            users.Property(user => user.Email).HasColumnName("email");
            users.HasMany(user => user.Followers);
            users.HasMany(user => user.Messages);

            followers.HasKey(follower => new
            {
                follower.WhoID,
                follower.WhomID
            });
            
            messages.HasKey(message => message.ID);
            messages.Property(message => message.ID).HasColumnName("message_id").ValueGeneratedOnAdd();
            messages.Property(message => message.AuthorID).HasColumnName("author_id");
            messages.Property(message => message.Text).HasColumnName("text");
            messages.Property(message => message.PublishDate).HasColumnName("pub_date").HasConversion(
                dateTimeOffset => dateTimeOffset.UtcDateTime,
                dateTime => new DateTimeOffset(dateTime)
            );
            messages.Property(message => message.IsFlagged).HasColumnName("flagged");
            messages.HasOne(message => message.Author);
            
            latests.Property(latest => latest.id).HasColumnName("id");
            latests.Property(latest => latest.latest).HasColumnName("latest");
        }
    }
}
