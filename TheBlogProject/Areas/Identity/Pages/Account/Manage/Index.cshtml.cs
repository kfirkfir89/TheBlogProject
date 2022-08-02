// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using TheBlogProject.Models;
using TheBlogProject.Services;

namespace TheBlogProject.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<BlogUser> _userManager;
        private readonly SignInManager<BlogUser> _signInManager;
        private readonly IImageService _imageService;
        private readonly IBlogEmailSender _emailSender;
        private readonly ILogger<ChangePasswordModel> _logger;

        public IndexModel(
            UserManager<BlogUser> userManager,
            SignInManager<BlogUser> signInManager,
            IImageService imageService,
            IBlogEmailSender emailSender, 
            ILogger<ChangePasswordModel> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _imageService = imageService;
            _emailSender = emailSender;
            _logger = logger;
        }

        public string DisplayName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? Location { get; set; }
        public string? AboutMe { get; set; }
        public bool IsEmailConfirmed { get; set; }


        public string CurrentImage { get; set; }

        public string? LinkedinUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? GithubUrl { get; set; }
        public string? FacebookUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
            [Display(Name = "Display Name")]
            public string DisplayName { get; set; }

            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and no more then {1} characters", MinimumLength = 2)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            [StringLength(50, ErrorMessage = "The {0} must be at least {2} and no more then {1}", MinimumLength = 2)]
            public string? Location { get; set; }

            [StringLength(200, ErrorMessage = "The {0} must be at least {2} and no more then {1}", MinimumLength = 2)]
            public string? AboutMe { get; set; }

            [Required]
            [EmailAddress]
            [Display(Name = "New email")]
            public string NewEmail { get; set; }

            
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public IFormFile ImageFile { get; set; }

            public string? LinkedinUrl { get; set; }
            public string? TwitterUrl { get; set; }
            public string? GithubUrl { get; set; }
            public string? FacebookUrl { get; set; }
        }

        private async Task LoadAsync(BlogUser user)
        {
            DisplayName = user.DisplayName;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Location = user.Location;
            AboutMe = user.AboutMe;
            Email = user.Email;
            IsEmailConfirmed = user.EmailConfirmed;

            CurrentImage = _imageService.DecodeImage(user.ImageData, user.ContentType);

            FacebookUrl = user.FacebookUrl;
            LinkedinUrl = user.LinkedinUrl;
            TwitterUrl = user.TwitterUrl;
            GithubUrl = user.GithubUrl;


            Input = new InputModel
            {
                DisplayName = user.DisplayName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Location = user.Location,
                AboutMe = user.AboutMe,
                NewEmail = user.Email,


                LinkedinUrl = user.LinkedinUrl,
                TwitterUrl = user.TwitterUrl,
                GithubUrl = user.GithubUrl,
                FacebookUrl = user.FacebookUrl
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            string facebookPattern = @"^(https:\/\/)(?:www\.)?facebook\.com\/(?:(?:\w)*#!\/)?(?:pages\/)?(?:[\w\-]*\/)*?(\/)?([\w\-\.]{5,})+$";
            string linkedinPattern = @"^http(s)?:\/\/([\w]+\.)?linkedin\.com\/in\/[A-z0-9_-]+\/?$";
            string githubPattern = @"^(http(s)?:\/\/)(www\.)?github\.([a-z])+\/([A-Za-z0-9]{1,})+\/?$";
            string twitterPattern = @"^https?:\/\/(?:www\.)?twitter\.com\/(?:#!\/)?@?([^/?#]*)(?:[?#].*)?$";

            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var email = await _userManager.GetEmailAsync(user);
            if (Input.NewEmail != email)
            {
                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateChangeEmailTokenAsync(user, Input.NewEmail);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmailChange",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, email = Input.NewEmail, code = code },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.NewEmail, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                StatusMessage = "Confirmation link to change email sent. Please check your email.";
                return RedirectToPage();
            }

            if(Input.OldPassword != null && Input.NewPassword != null)
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
            }

            if(Input.Location != user.Location)
            {
                user.Location = Input.Location;
                await _userManager.UpdateAsync(user);
                return RedirectToPage();
            }

            if (Input.AboutMe != user.AboutMe)
            {
                user.AboutMe = Input.AboutMe;
                await _userManager.UpdateAsync(user);
                return RedirectToPage();
            }

            if (Input.DisplayName != user.DisplayName)
            {
                user.DisplayName = Input.DisplayName;
                await _userManager.UpdateAsync(user);
                return RedirectToPage();                
            }

            if (Input.FirstName != user.FirstName)
            {
                user.FirstName = Input.FirstName;
                await _userManager.UpdateAsync(user);
                return RedirectToPage();
            }

            if (Input.LastName != user.LastName)
            {
                user.LastName = Input.LastName;
                await _userManager.UpdateAsync(user);
                return RedirectToPage();
            }

            if (Input.LinkedinUrl != user.LinkedinUrl)
            {
                Regex reg = new Regex(linkedinPattern);
                var test = reg.IsMatch(Input.LinkedinUrl);

                if (test)
                {
                    user.LinkedinUrl = Input.LinkedinUrl;
                    await _userManager.UpdateAsync(user);
                    return RedirectToPage();
                }
                else
                {
                    StatusMessage = "Linkedin is not Url link";
                    return RedirectToPage();
                }
            }

            if (Input.TwitterUrl != user.TwitterUrl)
            {
                Regex reg = new Regex(twitterPattern);
                var test = reg.IsMatch(Input.TwitterUrl);

                if (test)
                {
                    user.TwitterUrl = Input.TwitterUrl;
                    await _userManager.UpdateAsync(user);
                    return RedirectToPage();
                }
                else
                {
                    StatusMessage = "Twitter is not Url link";
                    return RedirectToPage();
                }
            }

            if (Input.GithubUrl != user.GithubUrl)
            {
                Regex reg = new Regex(githubPattern);
                var test = reg.IsMatch(Input.GithubUrl);

                if (test)
                {
                    user.GithubUrl = Input.GithubUrl;
                    await _userManager.UpdateAsync(user);
                    return RedirectToPage();
                }
                else
                {
                    StatusMessage = "Github is not Url link";
                    return RedirectToPage();
                }
            }

            if (Input.FacebookUrl != user.FacebookUrl)
            {
                Regex reg = new Regex(facebookPattern);
                var test = reg.IsMatch(Input.FacebookUrl);

                if (test)
                {
                    user.FacebookUrl = Input.FacebookUrl;
                    await _userManager.UpdateAsync(user);
                    return RedirectToPage();
                }
                else
                {
                    StatusMessage = "FaceBook is not Url link";
                    return RedirectToPage();
                }
            }

            //if and only if the user selected a new image will i update their profile
            if (Input.ImageFile is not null)
            {
                user.ImageData = await _imageService.EncodeImageAsync(Input.ImageFile);
                user.ContentType = _imageService.ContentType(Input.ImageFile);
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            _logger.LogInformation("User changed their password successfully.");
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
