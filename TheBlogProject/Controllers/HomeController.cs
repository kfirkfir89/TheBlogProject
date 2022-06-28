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
        private readonly ISlugService _slugService;

        public HomeController(ILogger<HomeController> logger, IBlogEmailSender emailSender, ApplicationDbContext context, UserManager<BlogUser> userManager, ISlugService slugService)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _userManager = userManager;
            _slugService = slugService;
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

        [HttpPost]
        public async Task<IActionResult> TagManagement(List<string> tagValues)
        {

            _context.Tags.RemoveRange(_context.Tags);

            foreach (var tag in tagValues)
            {
                _context.Add(new Tag()
                {
                    Text = tag
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(TagManagement));
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
        public async Task<IActionResult> UserTags([Bind("Id ,BlogUserId, Text")] Tag tag, List<string> tagValues)
        {

            var user = await _userManager.GetUserAsync(User);
            string[] array = tagValues.ToArray();
            user.MyTags = array;
            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(UserTags));
        }

        [HttpPost]
        public JsonResult Like(string? slug)
        {

            var post = _context.Posts.Where(x => x.Slug == slug).FirstOrDefault();
            var user =  _userManager.GetUserAsync(User).Result;

            List<string> likes = new List<string>();

            if (post.Likes == null)
            {
                likes.Add(user.Id);
                post.Likes = likes;
            }
            else if (post.Likes.Contains(user.Id))
            {
                post.Likes.Remove(user.Id);
                if(post.Likes.Count <= 0)
                {
                    post.Likes = null;
                }
            }
            else
            {
                post.Likes.Add(user.Id);

            }

            _context.SaveChanges();

            return Json(post.Likes);
        }

        [HttpPost]
        public JsonResult UsefulCode(string? slug)
        {

            var post = _context.Posts.Where(x => x.Slug == slug).FirstOrDefault();
            var user = _userManager.GetUserAsync(User).Result;

            List<string> usefulCodes = new List<string>();

            if (post.UsefulCodes == null)
            {
                usefulCodes.Add(user.Id);
                post.UsefulCodes = usefulCodes;
            }
            else if (post.UsefulCodes.Contains(user.Id))
            {
                post.UsefulCodes.Remove(user.Id);
                if (post.UsefulCodes.Count <= 0)
                {
                    post.UsefulCodes = null;
                }
            }
            else
            {
                post.UsefulCodes.Add(user.Id);

            }

            _context.SaveChanges();

            return Json(post.UsefulCodes);
        }


    }
}