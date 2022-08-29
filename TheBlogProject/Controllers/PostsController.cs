#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Models;
using TheBlogProject.Services;
using TheBlogProject.Enums;
using X.PagedList;
using TheBlogProject.ViewModels;
using System.Drawing;



namespace TheBlogProject.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISlugService _slugService;
        private readonly IImageService _imageService;
        private readonly UserManager<BlogUser> _userManager;
        private readonly BlogSearchService _blogSearchService;

        public PostsController(ApplicationDbContext context, ISlugService slugService, IImageService imageService, UserManager<BlogUser> userManager, BlogSearchService blogSearchService)
        {
            _context = context;
            _slugService = slugService;
            _imageService = imageService;
            _userManager = userManager;
            _blogSearchService = blogSearchService;
        }

        public async Task<IActionResult> SearchIndex(int? page, string searchTerm)
        {
            ViewData["searchTerm"] = searchTerm;

            var pageNumber = page ?? 1;
            var pageSize = 5;

            var posts = _blogSearchService.Search(searchTerm);

            return View(await posts.ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: Posts
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Posts.Include(p => p.BlogUser);
            return View(await applicationDbContext.ToListAsync());
        }

        public async Task<IActionResult> TagIndex(string tag)
        {
            //get all posts that contain this tag
            var allPosts = _context.Tags.Where(t => t.Text == tag).Select(t => t.PostId);
            var posts = _context.Posts.Where(p => allPosts.Contains(p.Id)).ToList();
            return View("Index", posts);
        }


        // GET: Posts/Details/5
        public async Task<IActionResult> Details(string? slug)
        {
            ViewData["Title"] = "Post Details Page";
/*
            if (string.IsNullOrEmpty(slug) && postCreated.Slug != null)
            {
                slug = postCreated.Slug;
            }
            else
            {
                return NotFound();
            }
*/
            var post = await _context.Posts
                .Include(p => p.BlogUser)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .ThenInclude(p => p.BlogUser)
                .Include(p => p.Comments)
                .ThenInclude(c => c.Moderator)
                .FirstOrDefaultAsync(m => m.Slug == slug);

            if (post == null)
            {
                return NotFound();
            }

            var dataVM = new PostDetailViewModel()
            {
                Post = post,
                Tags = _context.Tags
                        .Select(t => t.Text.ToLower())
                        .Distinct().ToList()
            };

            ViewData["HeaderImage"] = _imageService.DecodeImage(post.ImageData, post.ContentType);
            ViewData["MainText"] = post.Title;
            ViewData["SubText"] = post.Abstract;

            if(post.Views == null)
            {
                var views = 1;
                post.Views = views;
            }
            else
            {
                var views = post.Views;
                views++;
                post.Views = views;
            }

            await _context.SaveChangesAsync();

            ViewBag.ImageNow = _imageService.DecodeImage(post.ImageData, post.ContentType);
            return View(dataVM);
        }

        // GET: Posts/Create
        public async Task<IActionResult> Create()
        {

            List<string> tags = new List<string>();
            tags = _context.Tags.Where(t => t.BlogUserId == null && t.PostId == null).Select(t => t.Text).ToList();

            this.ViewData["DatabaseTagValues"] = tags.Select(x => new SelectListItem
            {
                Text = x.ToString()
            }).ToList();

            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Posts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Abstract,Content,ReadyStatus,Image")] Post post, List<string> tagValues,IFormFile postImage)
        {
            if (ModelState.IsValid)
            {
                post.Created = DateTime.UtcNow;

                var authorId = _userManager.GetUserId(User);
                post.BlogUserId = authorId;

                //create a variable to store whether an error has occurred
                var validationError = false;

                post.Image = postImage;
                //checking for image size
/*                if(post.Image != null)
                {
                    MemoryStream memoryStreamImgByte = new MemoryStream(await _imageService.EncodeImageAsync(post.Image));
                    Bitmap image = new Bitmap(memoryStreamImgByte);
                    int width = image.Width;
                    int height = image.Height;
                    if (width < 744)
                    {
                        validationError = true;
                        ModelState.AddModelError("Image", "Minimum image width 744px");
                    }
                }*/

                //use the _imageService to store the incoming user specified image
                post.ImageData = await _imageService.EncodeImageAsync(post.Image);
                post.ContentType = _imageService.ContentType(post.Image);
                
                //creating the slug and determine if it is unique
                var slug = _slugService.UrlFriendly(post.Title);

                
                //detect incoming empty Slugs
                if (string.IsNullOrEmpty(slug))
                {
                    validationError = true;
                    ModelState.AddModelError("", "The Title you provided cannot be used as it results in a empty slug.");
                }

                //detect incoming duplicate Slugs
                else if (!_slugService.IsUnique(slug))
                {
                    validationError = true;
                    ModelState.AddModelError("Title", "The Title you provided cannot be used as it results in a duplicate slug.");
                }

/*                else if (slug.Contains("test")) 
                {
                    validationError = true;
                    ModelState.AddModelError("", "Uh-oh are you testing again??");
                    ModelState.AddModelError("Title", "The Title cannot contain the word test.");
                }
*/
/*                if (validationError)
                {
                    ViewData["TagValues"] = string.Join("," , tagValues);
                    return View(post);
                }*/


                //how do i loop over the incoming list of string?

                List<Tag> postTags = new List<Tag>();

                foreach (var tag in tagValues)
                {
                    postTags.Add(new Tag()
                    {
                        Text = tag
                    });
                }

                post.Tags = postTags;
                post.Slug = slug;

                _context.Add(post);

                await _context.SaveChangesAsync();


                return RedirectToAction("Details",new { slug = post.Slug });
            }

            var user = await _userManager.GetUserAsync(User);
            List<string> tags = new List<string>();
            tags = _context.Tags.Select(t => t.Text).ToList();

            this.ViewData["DatabaseTagValues"] = tags.Select(x => new SelectListItem
            {
                Text = x.ToString()
            }).ToList();

            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id");

            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            List<string> tags = new List<string>();
            tags = _context.Tags.Where(t => t.PostId == null && t.BlogUserId == null).Select(t => t.Text).ToList();

            var post = await _context.Posts.Include(p => p.Tags).FirstOrDefaultAsync(p => p.Id == id);

            if (id == null)
            {
                return NotFound();
            }

            if (post == null)
            {
                return NotFound();
            }

            if (post.Tags != null && post.Tags.Count() > 0)
            {
                foreach (var postTag in post.Tags)
                {
                    foreach (var tagText in _context.Tags.Select(t => t.Text).ToList())
                    {
                        if (postTag.Text == tagText)
                        {
                            tags.Remove(tagText);
                        }
                    }

                }

                this.ViewData["DatabaseTagValues"] = tags.Select(x => new SelectListItem
                {
                    Text = x.ToString()
                }).ToList();

                ViewData["TagValues"] = string.Join(",", post.Tags.Where(t => t.PostId == post.Id).Select(t => t.Text).ToList());

                return View(post);
            }

            else
            {

                this.ViewData["DatabaseTagValues"] = tags.Select(x => new SelectListItem
                {
                    Text = x.ToString()
                }).ToList();

                return View(post);
            }

            ViewData["TagValues"] = string.Join(",", post.Tags.Where(t => t.PostId == post.Id).Select(t => t.Text).ToList());

            return View(post);
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Abstract,Content,ReadyStatus")] Post post,List<Tag> postTags, IFormFile newImage, List<string> tagValues)
        {
            if(id != post.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //the original Post
                    var newPost = await _context.Posts.Where(p => p.Id == post.Id).Include(p => p.Tags).FirstOrDefaultAsync();

                    newPost.Updated = DateTime.UtcNow;
                    newPost.Title = post.Title;
                    newPost.Abstract = post.Abstract;
                    newPost.Content = post.Content;
                    newPost.ReadyStatus = post.ReadyStatus;

                    var newSlug = _slugService.UrlFriendly(post.Title);
                    if (newSlug != newPost.Slug)
                    {
                        if (_slugService.IsUnique(newSlug))
                        {
                            newPost.Title = post.Title;
                            newPost.Slug = newSlug;
                        }
                        else
                        {
                            ModelState.AddModelError("Title", "This Title cannot beused as it result in a duplicate slug");
                            ViewData["TagValues"] = string.Join(",", post.Tags.Select(t => t.Text));
                            return View(post);
                        }
                    }

                    if (newImage is not null)
                    {
                        newPost.ImageData = await _imageService.EncodeImageAsync(newImage);
                        newPost.ContentType = _imageService.ContentType(newImage);
                    }

                    //remove all Tage previously assicuated with this post

                    _context.RemoveRange(newPost.Tags);

                    //add in the new Tags from edit form


                    foreach (var tag in tagValues)
                    {
                        newPost.Tags.Add(new Tag()
                        {
                            Text = tag
                        });
                    }
                    await _context.SaveChangesAsync();


                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction("Details",new {slug = _context.Posts.Find(id).Slug } );
            }


            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", post.BlogUserId);
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.BlogUser)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.Where(p => p.Id == id)
                .Include(p => p.Tags)
                .Include(p => p.Comments)
                .FirstOrDefaultAsync();

            foreach(var tag in post.Tags)
            {
                _context.Tags.Remove(tag);
            }
            foreach (var comment in post.Comments)
            {
                _context.Comments.Remove(comment);
            }
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction("SortByPages", "Home" , new { text = "myposts" });
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.Id == id);
        }
    }
}
