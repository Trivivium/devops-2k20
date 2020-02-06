using System.Collections.Generic;

namespace WebApplication.ViewModels.Timeline
{
    public class UserTimelineVM
    {
        public UserVM User { get; }
        public bool IsFollowing { get; }
        public List<TimelineMessageVM> Messages { get; }

        public UserTimelineVM(UserVM user, bool isFollowing, List<TimelineMessageVM> messages)
        {
            User = user;
            IsFollowing = isFollowing;
            Messages = messages;
        }
    }
}
