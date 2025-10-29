using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;

namespace CookingNotebookWebApp.Controllers
{
    public class RecipeController : Controller
    {
        private readonly AppDbContext _context;

        public RecipeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            try
            {
                var recipes = _context.Recipe.Include(r => r.User).ToList();
                return View(recipes);
            }
            catch (Exception ex)
            {
                // Return a simple error message
                return Content($"Lỗi: {ex.Message}\n\nStackTrace: {ex.StackTrace}", "text/plain");
            }
        }

        public IActionResult Details(int id)
        {
            var recipe = _context.Recipe
                .Include(r => r.User)
                .Include(r => r.Steps)
                .Include(r => r.Reviews.OrderByDescending(rv => rv.CreatedAt))
                    .ThenInclude(rv => rv.User)
                .FirstOrDefault(r => r.RecipeId == id);

            if (recipe == null)
            {
                return NotFound();
            }

            // Kiểm tra xem user đã yêu thích chưa và đã đánh giá chưa
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                recipe.IsFavorited = _context.Favorites
                    .Any(f => f.UserId == int.Parse(userId) && f.RecipeId == id);

                var userReview = _context.Reviews
                    .FirstOrDefault(r => r.UserId == int.Parse(userId) && r.RecipeId == id);
                if (userReview != null)
                {
                    recipe.UserRating = userReview.Rating;
                    recipe.UserComment = userReview.Comment;
                }
            }

            // Tính rating trung bình
            if (recipe.Reviews != null && recipe.Reviews.Any())
            {
                recipe.AverageRating = Math.Round(recipe.Reviews.Average(r => r.Rating), 1);
            }
            else
            {
                recipe.AverageRating = 0;
            }

            return View(recipe);
        }

        [Authorize]
        [HttpPost]
        public IActionResult ToggleFavorite(int recipeId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var existingFavorite = _context.Favorites
                .FirstOrDefault(f => f.UserId == userId && f.RecipeId == recipeId);

            if (existingFavorite != null)
            {
                // Bỏ thích
                _context.Favorites.Remove(existingFavorite);
                _context.SaveChanges();
                return Json(new { success = true, isFavorited = false });
            }
            else
            {
                // Thêm thích
                var favorite = new Favorite
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    CreatedAt = DateTime.Now
                };
                _context.Favorites.Add(favorite);
                _context.SaveChanges();
                return Json(new { success = true, isFavorited = true });
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult SubmitReview(int recipeId, int rating, string comment)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kiểm tra xem đã đánh giá chưa
            var existingReview = _context.Reviews
                .FirstOrDefault(r => r.UserId == userId && r.RecipeId == recipeId);

            if (existingReview != null)
            {
                // Cập nhật đánh giá
                existingReview.Rating = rating;
                existingReview.Comment = comment;
                existingReview.CreatedAt = DateTime.Now;
                _context.Reviews.Update(existingReview);
            }
            else
            {
                // Thêm đánh giá mới
                var review = new Review
                {
                    UserId = userId,
                    RecipeId = recipeId,
                    Rating = rating,
                    Comment = comment,
                    CreatedAt = DateTime.Now
                };
                _context.Reviews.Add(review);
            }

            _context.SaveChanges();
            return Json(new { success = true });
        }

        [Authorize]
        [HttpPost]
        public IActionResult DeleteReview(int recipeId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var existingReview = _context.Reviews
                .FirstOrDefault(r => r.UserId == userId && r.RecipeId == recipeId);

            if (existingReview != null)
            {
                _context.Reviews.Remove(existingReview);
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Review not found" });
        }

        public IActionResult GetReviews(int recipeId, int page = 1, int pageSize = 5)
        {
            var reviews = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.RecipeId == recipeId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new {
                    r.ReviewId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    UserName = r.User.FullName
                })
                .ToList();

            var totalReviews = _context.Reviews.Count(r => r.RecipeId == recipeId);
            var totalPages = (int)Math.Ceiling(totalReviews / (double)pageSize);

            return Json(new {
                reviews,
                currentPage = page,
                totalPages,
                hasNext = page < totalPages,
                hasPrev = page > 1
            });
        }
    }
}