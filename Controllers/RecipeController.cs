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

        public IActionResult Index(string searchTerm, string cookingMethodFilter, string typeOfDishFilter, string timeFilter)
        {
            try
            {
                var recipes = _context.Recipe.Include(r => r.User).AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var searchLower = searchTerm.ToLower();
                    recipes = recipes.Where(r => r.Title.ToLower().Contains(searchLower) ||
                                                (r.Description != null && r.Description.ToLower().Contains(searchLower)));
                }

                if (!string.IsNullOrEmpty(cookingMethodFilter))
                {
                    recipes = recipes.Where(r => r.Cooking_method == cookingMethodFilter);
                }

                if (!string.IsNullOrEmpty(typeOfDishFilter))
                {
                    recipes = recipes.Where(r => r.Type_of_dish == typeOfDishFilter);
                }

                var recipesList = recipes.ToList();

                if (!string.IsNullOrEmpty(timeFilter) && timeFilter != "all")
                {
                    switch (timeFilter)
                    {
                        case "15":
                            recipesList = recipesList.Where(r => r.PrepTime + r.CookTime < 15).ToList();
                            break;
                        case "30":
                            recipesList = recipesList.Where(r => r.PrepTime + r.CookTime >= 15 && r.PrepTime + r.CookTime <= 30).ToList();
                            break;
                        case "60":
                            recipesList = recipesList.Where(r => r.PrepTime + r.CookTime > 30 && r.PrepTime + r.CookTime <= 60).ToList();
                            break;
                        case "120":
                            recipesList = recipesList.Where(r => r.PrepTime + r.CookTime > 60).ToList();
                            break;
                    }
                }

                // Hardcoded filter options
                var cookingMethods = new List<string>
                {
                "Xào", "Chiên / Rán", "Áp Chảo", "Luộc", "Hấp", "Ninh / Hầm", "Kho / Om / Rim",
                "Nấu (Canh/Súp)", "Nướng", "Quay", "Lẩu", "Trộn", "Muối / Ngâm"
                };

                var typeOfDishes = new List<string>
                {
                "Món chính", "Món phụ/Món ăn kèm", "Món Canh / Súp", "Món Khai Vị", "Món Tráng Miệng", "Nước Chấm / Nước Sốt"
                };

                ViewBag.CookingMethodFilter = cookingMethodFilter;
                ViewBag.TypeOfDishFilter = typeOfDishFilter;
                ViewBag.TimeFilter = timeFilter;
                ViewBag.CookingMethods = cookingMethods;
                ViewBag.TypeOfDishes = typeOfDishes;

                return View(recipesList);
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
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
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
                recipe.AverageRating = (decimal)Math.Round(recipe.Reviews.Average(r => r.Rating), 1);
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

        [HttpGet]
        public IActionResult GetRecipeDetails(int id)
        {
            var recipe = _context.Recipe
                .Include(r => r.User)
                .Include(r => r.Steps)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
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
                recipe.AverageRating = (decimal)Math.Round(recipe.Reviews.Average(r => r.Rating), 1);
            }
            else
            {
                recipe.AverageRating = 0;
            }

            return Json(new
            {
                recipeId = recipe.RecipeId,
                title = recipe.Title,
                description = recipe.Description,
                prepTime = recipe.PrepTime,
                cookTime = recipe.CookTime,
                servings = recipe.Servings,
                imageUrl = recipe.ImageUrl,
                note = recipe.Note,
                cookingMethod = recipe.Cooking_method,
                typeOfDish = recipe.Type_of_dish,
                userName = recipe.User?.FullName,
                isFavorited = recipe.IsFavorited,
                userRating = recipe.UserRating,
                userComment = recipe.UserComment,
                averageRating = recipe.AverageRating,
                reviewCount = recipe.Reviews?.Count ?? 0,
                ingredients = recipe.RecipeIngredients?.Select(ri => new
                {
                    name = ri.Ingredient?.Name,
                    quantity = ri.Quantity,
                    unit = ri.Unit,
                    notes = ri.Notes
                }).ToList(),
                steps = recipe.Steps?.Select(s => new
                {
                    stepNumber = s.StepNumber,
                    description = s.Description,
                    imageUrl = s.ImageUrl,
                    timerInMinutes = s.TimerInMinutes
                }).OrderBy(s => s.stepNumber).ToList()
            });
        }

        public IActionResult GetReviews(int recipeId, int page = 1, int pageSize = 5)
        {
            var reviews = _context.Reviews
                .Include(r => r.User)
                .Where(r => r.RecipeId == recipeId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new
                {
                    r.ReviewId,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    UserName = r.User.FullName
                })
                .ToList();

            var totalReviews = _context.Reviews.Count(r => r.RecipeId == recipeId);
            var totalPages = (int)Math.Ceiling(totalReviews / (double)pageSize);

            return Json(new
            {
                reviews,
                currentPage = page,
                totalPages,
                hasNext = page < totalPages,
                hasPrev = page > 1
            });
        }
    }
}