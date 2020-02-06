using System;

namespace WebApplication.ViewModels.Timeline
{
    public class TimelineMessageVM
    {
        public UserVM User { get; }
        public string Text { get; }
        public DateTimeOffset PublishDate { get; }

        public TimelineMessageVM(UserVM user, string text, DateTimeOffset publishDate)
        {
            User = user;
            Text = text;
            PublishDate = publishDate;
        }

        public string GetPublishDateFormatted()
        {
            return PublishDate.ToString("f");
        }

        public string GetGravatarURL(int size = 80)
        {
            // Python: (md5(email.strip().lower().encode('utf-8')).hexdigest()
            var hash = "";
            
            return $"http://www.gravatar.com/avatar/%{hash}?d=identicon&s=%{size}";
        }
    }
}
