using System.ComponentModel.DataAnnotations;

namespace TheBlogProject.ViewModels
{
    public class ContactMe
    {
        [Required]
        [StringLength(80, ErrorMessage = "The {0} must be at least {2} and most {1} characters", MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and most {1} characters", MinimumLength = 2)]
        public string Subject { get; set; }

        [Required]
        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and most {1} characters", MinimumLength = 2)]
        public string Message { get; set; }
    }
}
