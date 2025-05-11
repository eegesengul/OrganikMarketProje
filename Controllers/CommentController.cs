using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrganikMarketProje.Data;
using OrganikMarketProje.Models;

namespace OrganikMarketProje.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public CommentController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ Adminlerin tüm yorumları listelemesi
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var comments = _context.Comments
                .Include(c => c.User)
                .Include(c => c.Product)
                .Include(c => c.Recipe)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            return View(comments);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Comment comment)
        {
            if (string.IsNullOrWhiteSpace(comment.Text) || comment.Rating < 1 || comment.Rating > 5)
            {
                TempData["CommentError"] = "Yorum boş olamaz ve puan 1 ile 5 arasında olmalıdır.";

                if (comment.ProductId != null)
                    return RedirectToAction("Details", "Product", new { id = comment.ProductId });

                if (comment.RecipeId != null)
                    return RedirectToAction("Details", "Recipe", new { id = comment.RecipeId });

                return RedirectToAction("Index", "Home");
            }

            comment.UserId = _userManager.GetUserId(User);
            comment.CreatedAt = DateTime.Now;

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            if (comment.ProductId != null)
                return RedirectToAction("Details", "Product", new { id = comment.ProductId });

            if (comment.RecipeId != null)
                return RedirectToAction("Details", "Recipe", new { id = comment.RecipeId });

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (comment.UserId != userId && !User.IsInRole("Admin"))
                return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            if (comment.ProductId != null)
                return RedirectToAction("Details", "Product", new { id = comment.ProductId });

            if (comment.RecipeId != null)
                return RedirectToAction("Details", "Recipe", new { id = comment.RecipeId });

            return RedirectToAction("Index", "Home");
        }

        // ✅ Admin için özel silme (gerekirse)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeleteByAdmin(int id)
        {
            var comment = await _context.Comments
                .Include(c => c.Product)
                .Include(c => c.Recipe)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null)
                return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (comment.UserId != userId)
                return Forbid();

            return View(comment);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Comment updatedComment)
        {
            var comment = await _context.Comments.FindAsync(updatedComment.Id);
            if (comment == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            if (comment.UserId != userId)
                return Forbid();

            if (string.IsNullOrWhiteSpace(updatedComment.Text) || updatedComment.Rating < 1 || updatedComment.Rating > 5)
            {
                ModelState.AddModelError("", "Yorum boş olamaz ve puan 1 ile 5 arasında olmalıdır.");
                return View(updatedComment);
            }

            comment.Text = updatedComment.Text;
            comment.Rating = updatedComment.Rating;
            comment.CreatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            if (comment.ProductId != null)
                return RedirectToAction("Details", "Product", new { id = comment.ProductId });

            if (comment.RecipeId != null)
                return RedirectToAction("Details", "Recipe", new { id = comment.RecipeId });

            return RedirectToAction("Index", "Home");
        }
    }
}
