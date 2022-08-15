using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services;
using TheBlogProject.ViewModels;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
{

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options => 
        options.UseNpgsql(connectionString), 
        ServiceLifetime.Transient);

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();


    builder.Services.AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddRoles<IdentityRole>()
        .AddDefaultUI()
        .AddDefaultTokenProviders()
        .AddEntityFrameworkStores<ApplicationDbContext>();

    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    //register my custom DataService class
    builder.Services.AddScoped<DataService>();
    builder.Services.AddScoped<BlogSearchService>();

    //register a preconfigured instance of the MailSettings class
    builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

    builder.Services.AddScoped<IBlogEmailSender, EmailService>();

    //register our image service
    builder.Services.AddScoped<IImageService, BasicImageService>();

    //register the slug service
    builder.Services.AddScoped<ISlugService, BasicSlugService>();

    builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "/Register");
        options.Conventions.AddAreaPageRoute("Identity", "/Account/Login", "/Login");
    });

}
                                                                                                                                                                                 
var app = builder.Build();

var dataService = app.Services.CreateScope()
                     .ServiceProvider
                     .GetRequiredService<DataService>();
await dataService.ManageDataAsync();    

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "SlugRoute",
    pattern: "BlogPosts/UrlFreindly/{slug}",
    defaults: new { controller = "Posts", action = "Details"});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
    name: "Register",
    pattern: "register",
    defaults: new { area = "Identity", controller = "Account", action = "Register" });

    endpoints.MapControllerRoute(
    name: "Login",
    pattern: "login",
    defaults: new { area = "Identity", controller = "Account", action = "Login" });


    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapRazorPages(); // this one
});

app.MapRazorPages();

app.Run();
