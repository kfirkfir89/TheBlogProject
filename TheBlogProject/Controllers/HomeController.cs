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
        private readonly SignInManager<BlogUser> _signInManager;


        public HomeController(ILogger<HomeController> logger, IBlogEmailSender emailSender, ApplicationDbContext context, UserManager<BlogUser> userManager, ISlugService slugService, SignInManager<BlogUser> signInManager = null)
        {
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
            _userManager = userManager;
            _slugService = slugService;
            _signInManager = signInManager;

        }



        public List<Post> GetPosts(int BlockNumber, string? text)
        {
            int BlockSize = 3;
            var user = _userManager.GetUserAsync(User);

            if(text != null)
            {
                if(text == "top" || text == "useful" || text == "foryou")
                {
                    if (text == "top")
                    {
                        var selectedPosts = _context.Posts
                            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                            .Where(p => p.Views != null)
                            .Include(p => p.Tags)
                            .OrderByDescending(p => p.Views.Value)
                            .ToPagedList(BlockNumber, BlockSize)
                            .ToList();

                        return selectedPosts;
                    }

                    else if (text == "useful")
                    {

                        var selectedPosts = _context.Posts
                            .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                            .Where(p => p.UsefulCodes != null)
                            .Include(p => p.Tags)
                            .OrderByDescending(p => p.UsefulCodes.Count)
                            .ToPagedList(BlockNumber, BlockSize)
                            .ToList();

                        return selectedPosts;

                    }

                    else if (text == "foryou")
                    {
                        var userTags = _context.Tags.Where(t => t.BlogUserId == user.Result.Id).ToList();
                        if(userTags != null && userTags.Count() > 0)
                        {
                            List<Post> selectedPosts = new List<Post>();
                            bool flag = false;
                            int counter = 0;
                            List<int> postsIds = new List<int>();
                            
                            foreach (var post in _context.Posts.Where(p => p.Tags.Count() > 0).Where(p => p.ReadyStatus == ReadyStatus.ProductionReady).Include(t => t.Tags))
                            {
                                foreach (var userTag in userTags)
                                {
                                    var postTags = post.Tags.Select(t => t.Text);

                                    foreach (var item in postTags)
                                    {
                                        if (item == userTag.Text)
                                        {
                                            counter++;
                                            break;
                                        }
                                    }

                                    if (counter >= 2)
                                    {
                                        counter = 0;
                                        flag = false;

                                        foreach (var pId in postsIds)
                                        {
                                            if (pId == post.Id)
                                            {
                                                flag = true;
                                                break;
                                            }
                                        }
                                        if (flag == false)
                                        {
                                            postsIds.Add(post.Id);
                                            selectedPosts.Add(post);
                                        }
                                    }
                                }
                            }

                            return selectedPosts.ToPagedList(BlockNumber, BlockSize).ToList();
                        }
                    }
                }
                else 
                {
                    /*tags*/
                    List<Post> selectedPosts = new List<Post>();

                    var posts = _context.Posts.Where(p => p.Tags.Count() > 0).Where(p => p.ReadyStatus == ReadyStatus.ProductionReady).Include(t => t.Tags).ToList();

                    foreach (var post in posts)
                    {

                        var postTags = post.Tags.Select(t => t.Text).ToList();

                        foreach (var item in postTags)
                        {
                            if (item == text)
                            {
                                selectedPosts.Add(post);
                                break;
                            }
                        }
                    }

                    return selectedPosts.ToPagedList(BlockNumber, BlockSize).ToList();
                }
            }


            var query = _context.Posts
                    .Include(p => p.BlogUser)
                    .Include(p => p.Tags)
                    .Include(p => p.Comments)
                    .ThenInclude(p => p.BlogUser)
                    .Include(p => p.Comments)
                    .ThenInclude(c => c.Moderator)
                    .OrderByDescending(p => p.Created)
                    .ToPagedList(BlockNumber, BlockSize).ToList();

            return query;
        }




        public async Task<IActionResult> SortBy(string? text, string? tag)
        {
            if(text != null)
            {
                ViewBag.Text = text;
            }
            if (tag != null)
            {
                ViewBag.Tag = tag;
            }        
            return View();

        }

        public async Task<IActionResult> SortByPages(int? page, string? text, string? id)
        {
            var user = _userManager.GetUserAsync(User);

            var pageNumber = page ?? 1;
            var pageSize = 10;

            if(id != null)
            {

                var selectedPosts = _context.Posts
                    .Where(p => p.BlogUserId == id && p.ReadyStatus == ReadyStatus.ProductionReady)
                    .Include(p => p.Tags)
                    .OrderByDescending(p => p.Created)
                    .ToPagedList(pageNumber, pageSize);


                
                ViewBag.CommetnsCount = _context.Comments.Where(c => c.BlogUserId == id).Count();
                ViewBag.PostsCount = _context.Posts.Where(p => p.BlogUserId == id).Count();
                ViewBag.PostsUser = id;

                return View(selectedPosts);
            }

            if (text == "myposts" || text == "mylikes" || text == "myuseful")
            {
                if(text == "myposts")
                {
                    var selectedPosts = _context.Posts
                        .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                        .Where(p => p.BlogUserId == user.Result.Id)
                        .Include(p => p.Tags)
                        .OrderByDescending(p => p.Created)
                        .ToPagedList(pageNumber, pageSize);

                    ViewBag.CommetnsCount = _context.Comments.Where(c => c.BlogUserId == id).Count();
                    ViewBag.PostsCount = _context.Posts.Where(p => p.BlogUserId == id).Count();
                    ViewBag.PostsUser = user.Result.Id;

                    return View(selectedPosts);
                }
                else if (text == "mylikes")
                {
                    var selectedPosts = _context.Posts
                        .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                        .Where(p => p.Likes.Contains(user.Result.Id))
                        .Include(p => p.Tags)
                        .OrderByDescending(p => p.Created)
                        .ToPagedList(pageNumber, pageSize);

                    ViewBag.BtnText = text;
                    return View(selectedPosts);
                }
                else if (text == "myuseful")
                {
                    var selectedPosts = _context.Posts
                        .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                        .Where(p => p.UsefulCodes.Contains(user.Result.Id))
                        .Include(p => p.Tags)
                        .OrderByDescending(p => p.Created)
                        .ToPagedList(pageNumber, pageSize);

                    ViewBag.BtnText = text;

                    return View(selectedPosts);
                }

            }
            return View();

        }

        [HttpPost]
        public IActionResult InfinateScroll(int BlockNumber, string? text)
        {
            var posts = GetPosts(BlockNumber,text);

            if(posts.Count == 0)
            {
                var noMoreData = true;
                return Json(noMoreData);
            }
            //////////////// THis line of code only for demo. Needs to be removed ////
/*            System.Threading.Thread.Sleep(01);*/
            //////////////////////////////////////////////////////////////////////////
            ViewBag.BlockNumber = BlockNumber;
            return PartialView("_postPartial",posts);
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Posts = _context.Posts.Where(p => p.Tags.Count > 0).Include(p => p.Tags).ToList();
            return View();

        }











        public async Task<IActionResult> About(int? page , string? text, string? tag)

        public ActionResult GetData(int BlockNumber, int BlockSize)
        {
            System.Threading.Thread.Sleep(4000);
            var query = _context.Posts.Skip(BlockNumber*BlockSize).Take(BlockSize).OrderByDescending(p=>p.Id).ToList();
            ViewData.Model = query;
            return Json(query);
        }


        public async Task<IActionResult> About()
        {
            var BlockSize = 5;
            var BlockNumber = 0;
            var query = _context.Posts.Skip(BlockNumber * BlockSize).Take(BlockSize).ToList();

            return View(query);
        }


        public async Task<IActionResult> Index(int? page , string? text, string? tag)

        {
            var pageNumber = page ?? 1;
            var pageSize = 5;

/*            var blogs = _context.Blogs.Where(
                b => b.Posts.Any(p => p.ReadyStatus == Enums.ReadyStatus.ProductionReady))
                .OrderByDescending(b => b.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            var blogs = _context.Blogs
                .Include(b => b.BlogUser)
                .OrderByDescending(b => b.Created)
                .ToPagedListAsync(pageNumber, pageSize);

            return View(await blogs);*/

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                var guestPosts = _context.Posts

                    .Include(p => p.Tags)
                    .OrderByDescending(p => p.Created)
                    .ToPagedList(pageNumber, pageSize);


                return View(guestPosts);
            }

            var userTags = _context.Tags.Where(t => t.BlogUserId == user.Id).ToList();


            if(text != null && _signInManager.IsSignedIn(User))
            {
                if(text == "foryou" && userTags != null && userTags.Count() > 0)
                {
                    List<Post> selectedPosts = new List<Post>();
                    bool flag = false;
                    int counter = 0;
                    List<int> postsIds = new List<int>();

                    foreach(var post in _context.Posts.Where(p => p.Tags.Count() > 0).Where(p => p.ReadyStatus == ReadyStatus.ProductionReady).Include(t => t.Tags))
                    {
                        foreach(var userTag in userTags)
                        {
                            var postTags = post.Tags.Select(t => t.Text);

                            foreach (var item in postTags)
                            {
                                if (item == userTag.Text)
                                {
                                    counter++;
                                    break;
                                }
                            }

                            if (counter >= 2)
                            {
                                counter = 0;
                                flag = false;

                                foreach (var pId in postsIds)
                                {
                                    if (pId == post.Id)
                                    {
                                        flag = true;
                                        break;
                                    }
                                }
                                if (flag == false)
                                {
                                    postsIds.Add(post.Id);
                                    selectedPosts.Add(post);
                                }
                            }
                        }
                    }


                    var pagedList = selectedPosts.ToPagedList(pageNumber, pageSize);
                    return View(pagedList);
                }

                else if(text == "useful" && userTags != null)
                {
                    var selectedPosts = _context.Posts
                        .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                        .Where(p => p.UsefulCodes != null)
                        .Include(p => p.Tags)
                        .OrderByDescending(p => p.UsefulCodes).ToList()
                        .ToPagedList(pageNumber, pageSize);



                    return View(selectedPosts);
                }

                else if(text == "top" && userTags != null)
                {
                    var selectedPosts = _context.Posts
                        .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                        .Where(p => p.Views != null)
                        .Include(p => p.Tags)
                        .OrderByDescending(p => p.Views.Value)
                        .ToPagedList(pageNumber, pageSize);

                    return View(selectedPosts);
                }
            }
            else if(tag != null)
            {
                List<Post> selectedPosts = new List<Post>();
                bool flag = false;
                int counter = 0;
                List<int> postsIds = new List<int>();

                foreach (var post in _context.Posts.Where(p => p.Tags.Count() > 0).Where(p => p.ReadyStatus == ReadyStatus.ProductionReady).Include(t => t.Tags))
                {

                        var postTags = post.Tags.Select(t => t.Text);

                        foreach (var item in postTags)
                        {
                            if (item == tag)
                            {
                                counter++;
                                break;
                            }
                        }

                        if (counter >= 1)
                        {
                            counter = 0;
                            flag = false;

                            foreach (var pId in postsIds)
                            {
                                if (pId == post.Id)
                                {
                                    flag = true;
                                    break;
                                }
                            }
                            if (flag == false)
                            {
                                postsIds.Add(post.Id);
                                selectedPosts.Add(post);
                            }
                        }

                }

                var pagedList = selectedPosts.ToPagedList(pageNumber, pageSize);
                return View(pagedList);

            }

            var posts = _context.Posts
                .Where(p => p.ReadyStatus == ReadyStatus.ProductionReady)
                .Include(p => p.Tags)
                .OrderByDescending(p => p.Created)
                .ToPagedList(pageNumber, pageSize);


            return View(posts);


        }


        public async Task<IActionResult> Contact()
        {

            ViewBag.Posts = _context.Posts.Where(p => p.Tags.Count > 0).Include(p => p.Tags).ToList();
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
            /*            ViewBag.Posts = _context.Posts.Where(p => p.Tags.Count > 0).Include(p => p.Tags).ToList();*/


            ViewData["TagValues"] = string.Join(",", _context.Tags.Where(t => t.PostId == null && t.BlogUserId == null).Select(t => t.Text));


            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TagManagement(List<string> tagValues)
        {

            var db = _context.Tags.Where(t => t.BlogUserId == null && t.PostId == null);
            _context.Tags.RemoveRange(db);
            
            foreach(var tag in tagValues)
            {
                _context.Tags.Add(new Tag()
                {
                    Text = tag
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(TagManagement));
        }

        [HttpGet]
        public async Task<IActionResult> UserTags()
        {
            var user = await _userManager.GetUserAsync(User);
            List<string> tagsDb = new List<string>();
            tagsDb = _context.Tags.Where(t => t.PostId == null && t.BlogUserId == null).Select(t => t.Text).ToList();
            var userTags = _context.Tags.Where(t => t.BlogUserId == user.Id).ToList();
            List<string> userDb = new List<string>();
            userDb = _context.Tags.Where(t => t.BlogUserId == user.Id).Select(t => t.Text).ToList();


            if (userTags != null && userTags.Count() > 0)
            {
                foreach (var tag in userTags)
                {
                    foreach (var tagText in _context.Tags.Where(t => t.PostId == null && t.BlogUserId == null).Select(t => t.Text).ToList())
                    {
                        if (tag.Text == tagText)
                        {
                            tagsDb.Remove(tagText);
                        }
                    }
                }

                this.ViewData["DatabaseTagValues"] = tagsDb.Select(x => new SelectListItem
                {
                    Text = x.ToString()
                }).ToList();

                this.ViewData["TagValues"] = userDb.Select(x => new SelectListItem
                {
                    Text = x.ToString()
                }).ToList();

                /*                ViewData["TagValues"] = string.Join(",", userTags.Select(t => t.Text));*/

                //sidebar suggestions
                ViewBag.Posts = _context.Posts.Where(p => p.Tags.Count > 0).Include(p => p.Tags).ToList();

                return PartialView();
            }

            else
            {
                this.ViewData["DatabaseTagValues"] = tagsDb.Select(x => new SelectListItem
                {
                    Text = x.ToString()
                }).ToList();

                return PartialView();
            }

        }

        [HttpPost]
        public async Task<IActionResult> UserTags([Bind("Id ,BlogUserId, Text")] Tag tag, List<string> tagValues)
        {

            var user = await _userManager.GetUserAsync(User);
            var userTags = _context.Tags.Where(t => t.BlogUserId == user.Id).ToList();

            _context.Tags.RemoveRange(userTags);

            foreach (var item in tagValues)
            {
                _context.Tags.Add(new Tag()
                {
                    BlogUserId = user.Id,
                    Text = item
                });
            }

            await _context.SaveChangesAsync();


            return RedirectToAction(nameof(Index));
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

        public async Task<IActionResult> DemoLogin(string? Id)
        {

            if (Id == "admin")
            {
                await _signInManager.PasswordSignInAsync("kfirkfir89@gmail.com", "kfir123455AA!", false, lockoutOnFailure: false);
                return RedirectToAction(nameof(Index));
            }
            else if(Id == "moderator")
            {
                await _signInManager.PasswordSignInAsync("kfirkfirAAA@mailinator.com", "Abc&123!33", false, lockoutOnFailure: false);
                return RedirectToAction(nameof(Index));
            }
            else if(Id == "user1")
            {
                await _signInManager.PasswordSignInAsync("kfirAAA2334@mailinator.com", "Abc&123!33", false, lockoutOnFailure: false);
                return RedirectToAction(nameof(Index));
            }

            return View();
        }


    }
}