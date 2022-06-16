using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TheBlogProject.Data;
using TheBlogProject.Enums;
using TheBlogProject.Models;
using TheBlogProject.Services;
using TheBlogProject.ViewModels;
using X.PagedList;

namespace TheBlogProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public HomeController(ILogger<HomeController> logger, IBlogEmailSender emailSender, ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _userManager = userManager;
        }




        /*        public async Task<IActionResult> Index()
                {
                    var posts = await _context.Posts
                                .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                                .Include(p => p.Tags)
                                .OrderByDescending(p => p.Created)
                                .ToListAsync();

                    return View(posts);
                }*/

        public async Task<IActionResult> Index(int? page)
        {
/*            var pageNumber = page ?? 1;
            var pageSize = 5;

            var blogs = _context.Blogs.Where(
                b => b.Posts.Any(p => p.ReadyStatus == Enums.ReadyStatus.ProductionReady))
                .OrderByDescending(b => b.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            var blogs = _context.Blogs
                .Include(b => b.BlogUser)
                .OrderByDescending(b => b.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            return View(await blogs);*/

            var posts = await _context.Posts
                .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                .Include(p => p.Tags)
                .OrderByDescending(p => p.Created)
                .ToListAsync();


            return View(posts);



        }

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> Contact()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Contact(ContactMe model)
        {
            model.Message = $"{model.Message}<hr/> Phone: {model.Phone}";
            await _emailSender.SendContactEmailAsync(model.Email, model.Name, model.Subject, model.Message);

            return RedirectToAction("Index");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }



        public async Task<IActionResult> TagManagement()
        {


/*            this.ViewData["TagValues"] = _context.Tags.Select(x => new SelectListItem
            {
                Text = x.Text.ToString()
            }).ToList();
*/

            ViewData["TagValues"] = string.Join(",", _context.Tags.Select(t => t.Text));


            return View();
        }


        public async Task<IActionResult> UserTags()
        {
            var user = await _userManager.GetUserAsync(User);
            List<string> tags = new List<string>();
            tags = _context.Tags.Select(t => t.Text).ToList();


            foreach (var tag in user.MyTags)
            {
                foreach (var tagText in _context.Tags.Select(t => t.Text).ToList())
                {
                    if (tag == tagText)
                    {
                        tags.Remove(tagText);
                    }
                }
            }

            this.ViewData["DatabaseTagValues"] = tags.Select(x => new SelectListItem
            {
                Text = x.ToString()
            }).ToList();

            ViewData["TagValues"] = string.Join(",", user.MyTags);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UserTags([Bind("Id ,BlogUserId, Text")] Tag tag ,List<string> tagValues)
        {
/*            var user = await _userManager.GetUserAsync(User);
            var index = 1;

            //how do i loop over the incoming list of string?
            foreach (var t in tagValues)
            {
                tag.Text = t.ToString();
                tag.BlogUserId = user.Id;
                tag.Id = index;
                user.Tags.Add(tag);
                index++;
            }
*/

            var user = await _userManager.GetUserAsync(User);
            string[] array = tagValues.ToArray();
            user.MyTags = array;
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(UserTags));
        }

        /*        [HttpPost]
                public async Task<IActionResult> TagManagement(List<string> TagValues)
                {
                    return View();
                }*/
    }
}