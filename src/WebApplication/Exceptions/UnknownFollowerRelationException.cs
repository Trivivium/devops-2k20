using System;

namespace WebApplication.Exceptions
{
    public class UnknownFollowerRelationException : Exception
    {
        public UnknownFollowerRelationException(int userToFollowID, int userFollowingID) 
            : base($"Unknown follower relation between users with IDs {userToFollowID} (whom) and {userFollowingID} (who).")
        {}
    }
}
