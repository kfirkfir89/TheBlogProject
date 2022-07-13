using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TheBlogProject.Enums;

namespace TheBlogProject.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string? BlogUserId { get; set; }

        [Required]
        [StringLength(75, ErrorMessage ="The {0} must be at least {2} and no more then {1}", MinimumLength = 2)]
        public string? Title { get; set; }
        
        [StringLength(200, ErrorMessage = "The {0} must be at least {2} and no more then {1}", MinimumLength = 2)]
        public string? Abstract { get; set; }
        
        [Required]
        public string? Content { get; set; }

        [DataType(DataType.Date)]
        [Display(Name ="Created Date")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Updated Date")]
        public DateTime? Updated { get; set; }

        public ReadyStatus ReadyStatus { get; set; }

        public string? Slug { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ContentType { get; set; }

        public List<string>? Likes { get; set; } = new List<string>();

        public List<string>? UsefulCodes { get; set; } = new List<string>();

        public int? Views { get; set; }

        [NotMapped]
        public  IFormFile? Image { get; set; }

        //navigation property
        public virtual BlogUser? BlogUser { get; set; }

        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
