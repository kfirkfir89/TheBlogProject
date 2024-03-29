﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheBlogProject.Models
{
    public class BlogUser : IdentityUser
    {
        [Required]
        [StringLength(50, ErrorMessage ="The {0} must be at least {2} and no more then {1} characters", MinimumLength =2)]
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
        [Display(Name = "Display Name")]
        public string DisplayName { get; set; }

        public byte[]? ImageData { get; set; }
        public string? ContentType { get; set; }

        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and no more then {1}", MinimumLength = 2)]
        public string? Location { get; set; }

        [StringLength(500, ErrorMessage = "The {0} must be at least {2} and no more then {1}", MinimumLength = 2)]
        public string? AboutMe { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
        public string? FacebookUrl { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
        public string? GithubUrl { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
        public string? LinkedinUrl { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
        public string? TwitterUrl { get; set; }

        [NotMapped]
        public string FullName 
        {
            get { return $"{FirstName} {LastName}"; }
        }

        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}
