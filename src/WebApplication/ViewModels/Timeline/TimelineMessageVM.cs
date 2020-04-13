using System;
using System.Text;
using System.Security.Cryptography;

namespace WebApplication.ViewModels.Timeline
{
    public class TimelineMessageVM
    {
        public int ID { get; }
        public UserVM User { get; }
        public string Text { get; }
        public DateTimeOffset PublishDate { get; }
        public bool IsFlagged { get; }

        public TimelineMessageVM(int id, UserVM user, string text, DateTimeOffset publishDate, bool isFlagged)
        {
            ID = id;
            User = user;
            Text = text;
            PublishDate = publishDate;
            IsFlagged = isFlagged;
        }

        public string GetPublishDateFormatted()
        {
            return PublishDate.ToString("f");
        }

        public string GetGravatarURL(int size = 80)
        {
            var hash = CalculateMD5Hash(User.Email);
            
            return $"http://www.gravatar.com/avatar/{hash}?d=identicon&s={size}";
        }
        
        public string CalculateMD5Hash(string input)
        {
            // we want to remove any wild spaces etc.
            input = input.Trim().ToLower();

            // calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
