using Microsoft.AspNetCore.Identity;
using TheBlogProject.Data;
using TheBlogProject.Enums;
using TheBlogProject.Models;

namespace TheBlogProject.Services
{
    public class DataService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BlogUser> _userManager;

        public DataService(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager, UserManager<BlogUser> userManager)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task ManageDataAsync()
        {
            //seed a few roles into the system
            await SeedRolesAsync();

            //seed a user into the system
            await SeedUsersAsync();
        }

        private async Task SeedRolesAsync()
        {
            //if there are already roles in the system,do nothing.
            if (_dbContext.Roles.Any())
            {
                return;
            }
            //otherwise we want to create a few roles.
            foreach (var role in Enum.GetNames(typeof(BlogRole)))
            {
                //need to use the Role manager to create roles
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        private async Task SeedUsersAsync()
        {
            //if there are already users in the system,do nothing.
            if (!_dbContext.Users.Any())
            {
            //creates a new instance of BlogUser
            var adminUser = new BlogUser()
            {
                FirstName = "Kfir",
                LastName = "Avraham",
                Email = "admin@contoso.com",
                UserName = "admin@contoso.com",
                NormalizedUserName = "ADMIN@CONTOSO.COM",
                NormalizedEmail = "ADMIN@CONTOSO.COM",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };
            //use the UserManager to create a new user that is defined by adminUser.
            
            await _userManager.CreateAsync(adminUser, "123");

            //add this new user to the Administrator role
            await _userManager.AddToRoleAsync(adminUser, BlogRole.Administrator.ToString());
            }
            else
            {
                return;
            }



/*
            //Create the moderator user
            var modUser = new BlogUser()
            {
                UserName = "kfirkfir89@gmail.com",
                Email = "kfirkfir89@gmail.com",
                FirstName = "ModKfir",
                LastName = "ModAvraham",
                PhoneNumber = "054-3546015",
                EmailConfirmed = true
            };
            await _userManager.CreateAsync(modUser, "123");
            await _userManager.AddToRoleAsync(modUser, BlogRole.Moderator.ToString());
 */
        }
    }
}
