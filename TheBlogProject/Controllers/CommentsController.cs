﻿#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBlogProject.Data;
using TheBlogProject.Models;
using X.PagedList;

namespace TheBlogProject.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BlogUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<BlogUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Comments
        public async Task<IActionResult> Index(int? page, string? id)
        {
            var pageNumber = page ?? 1;
            var pageSize = 20;
            var user = _userManager.GetUserAsync(User);

            var allComments = _context.Comments.Where(c => c.BlogUserId == id)
                .Include(c => c.Post)
                .Include(c => c.BlogUser)
                .ToPagedList(pageNumber, pageSize);


            ViewBag.CommentUser = id;

            return View(allComments);
        }

        public async Task<IActionResult> CommentsManagement(int? page,string? type)
        {
            var pageNumber = page ?? 1;
            var pageSize = 20;

            if(type == "moderated")
            {
                var selectedComments = await _context.Comments
                    .Where(c => c.Moderator != null)
                    .Include(c => c.Post)
                    .Include(c => c.BlogUser)
                    .OrderByDescending(c => c.Created)
                    .ToPagedListAsync(pageNumber,pageSize);

                ViewBag.type = "moderated";
                return View(selectedComments);
            }
            else if (type == "new")
            {
                var selectedComments = await _context.Comments
                    .Where(c => c.Moderator == null)
                    .Include(c => c.Post)
                    .Include(c => c.BlogUser)
                    .OrderByDescending(c => c.Created)
                    .ToPagedListAsync(pageNumber, pageSize);

                ViewBag.type = "new";
                return View(selectedComments);
            }
            else
            {
                var selectedComments = await _context.Comments
                    .Include(c => c.Post)
                    .Include(c => c.BlogUser)
                    .ToPagedListAsync(pageNumber, pageSize);

                ViewBag.type = "all";
                return View(selectedComments);
            }
            
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Body")] Comment comment)
        {
            if (ModelState.IsValid)
            {
                var post = await _context.Posts.SingleOrDefaultAsync(p => comment.PostId == p.Id);
                comment.BlogUserId = _userManager.GetUserId(User);
                comment.Created = DateTime.UtcNow;
                _context.Add(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Posts", new { slug = post.Slug }, "commentSection");
            }

            return View(comment);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            ViewData["BlogUserId"] = new SelectList(_context.Users, "Id", "Id", comment.BlogUserId);
            ViewData["ModeratorId"] = new SelectList(_context.Users, "Id", "Id", comment.ModeratorId);
            ViewData["PostId"] = new SelectList(_context.Posts, "Id", "Abstract", comment.PostId);
            return View(comment);
        }

        // POST: Comments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id, Body")] Comment comment, int? backto)
        {
            if (id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);
                try
                {
                    newComment.Body = comment.Body;
                    newComment.Updated = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                if(backto != null)
                {
                    return RedirectToAction("Details", "Posts", new { slug = _context.Posts.Find(backto).Slug });
                }
                else
                {
                    //Redirect to action Details of the Post controller and the route data is the slug
                    return RedirectToAction("Index", "Comments", new { id = _userManager.GetUserAsync(User).Result.Id });
                }
            }
            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Moderator, Administrator")]
        public async Task<IActionResult> Moderate(int id, [Bind("Id, Body, ModeratedBody, ModerationType")] Comment comment,string type ,int backto)
        {
            if(id != comment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var newComment = await _context.Comments.Include(c => c.Post).FirstOrDefaultAsync(c => c.Id == comment.Id);

                try
                {
                    newComment.ModeratedBody = comment.ModeratedBody;
                    newComment.ModerationType = comment.ModerationType;

                    newComment.Moderated = DateTime.UtcNow;
                    newComment.ModeratorId = _userManager.GetUserId(User);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentExists(comment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                if(backto != null && backto > 0)
                {
                    return RedirectToAction("Details", "Posts", new { slug = _context.Posts.Find(backto).Slug });
                }
                else
                {
                    return RedirectToAction("CommentsManagement", "Comments", new { type = type });
                }

            }
            return View(comment);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comment = await _context.Comments
                .Include(c => c.BlogUser)
                .Include(c => c.Moderator)
                .Include(c => c.Post)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comment == null)
            {
                return NotFound();
            }

            return View(comment);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, string? type, int? backto)
        {
            if(type == "commentsManagement")
            {
                var comment = await _context.Comments.FindAsync(id);
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("CommentsManagement", "Comments");
            }
            else if(backto != null)
            {
                var comment = await _context.Comments.FindAsync(id);
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Posts", new { slug = _context.Posts.Find(backto).Slug });

            }
            else
            {
                var comment = await _context.Comments.FindAsync(id);
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Comments", new { id = _userManager.GetUserAsync(User).Result.Id });
            }

        }

        private bool CommentExists(int id)
        {
            return _context.Comments.Any(e => e.Id == id);
        }
    }
}
