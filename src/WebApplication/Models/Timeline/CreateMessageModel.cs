using System.ComponentModel.DataAnnotations;

namespace WebApplication.Models.Timeline
{
    public class CreateMessageModel
    {
        [Required]
        public string Content { get; set; }
    }
}