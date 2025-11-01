using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models.ViewModels;
using CookingNotebookWebApp.Models;

namespace CookingNotebook.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
        {
            return User.IsInRole("Admin");
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var model = new DashboardViewModel
            {
                IngredientCount = _context.Ingredient.Count(),
                RecipeCount = _context.Recipe.Count(),
                UserCount = _context.Users.Count(),
                ReviewCount = _context.Reviews.Count()
            };

            return View(model);
        }

        public IActionResult UserList()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult EditUser(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(User model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            if (ModelState.IsValid)
            {
                _context.Users.Update(model);
                _context.SaveChanges();
                return RedirectToAction("UserList");
            }
            return View(model);
        }

        public IActionResult LockUnlock(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            user.Status = !user.Status;
            _context.SaveChanges();
            return RedirectToAction("UserList");
        }

        public IActionResult RecipeList(string searchTerm)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var recipes = _context.Recipe.Include(r => r.User).AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                recipes = recipes.Where(r => r.Title.Contains(searchTerm) ||
                                           (r.Description != null && r.Description.Contains(searchTerm)));
            }

            return View(recipes.ToList());
        }

        public IActionResult EditRecipe(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var recipe = _context.Recipe
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Steps)
                .FirstOrDefault(r => r.RecipeId == id);
            if (recipe == null)
            {
                return NotFound();
            }

            var viewModel = new EditRecipeViewModel
            {
                Recipe = recipe,
                Ingredients = recipe.RecipeIngredients.ToList(),
                Steps = recipe.Steps.OrderBy(s => s.StepNumber).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult EditRecipe(EditRecipeViewModel model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            if (ModelState.IsValid)
            {
                model.Recipe.UpdatedAt = DateTime.Now;
                _context.Recipe.Update(model.Recipe);

                // Handle ingredients - for now, keep as is, later enhance
                // Handle steps - for now, keep as is, later enhance

                _context.SaveChanges();
                return RedirectToAction("RecipeList");
            }
            return View(model);
        }

        public IActionResult CreateRecipe()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateRecipe(Recipe model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.Now;
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                {
                    model.UserId = userId;
                }
                else
                {
                    // Nếu không có user ID, có thể set một giá trị mặc định hoặc admin user
                    model.UserId = 1; // Hoặc tìm admin user đầu tiên
                }
                _context.Recipe.Add(model);
                _context.SaveChanges();
                return RedirectToAction("RecipeList");
            }
            return View(model);
        }

        public IActionResult DeleteRecipe(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var recipe = _context.Recipe.Find(id);
            if (recipe != null)
            {
                _context.Recipe.Remove(recipe);
                _context.SaveChanges();
            }
            return RedirectToAction("RecipeList");
        }

        public IActionResult IngredientList(string searchTerm)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var ingredients = _context.Ingredient.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                ingredients = ingredients.Where(i => i.Name.Contains(searchTerm) ||
                                                   (i.Description != null && i.Description.Contains(searchTerm)));
            }

            return View(ingredients.ToList());
        }

        public IActionResult EditIngredient(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var ingredient = _context.Ingredient.Find(id);
            if (ingredient == null)
            {
                return NotFound();
            }
            return View(ingredient);
        }

        [HttpPost]
        public IActionResult EditIngredient(Ingredient model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            if (ModelState.IsValid)
            {
                _context.Ingredient.Update(model);
                _context.SaveChanges();
                return RedirectToAction("IngredientList");
            }
            return View(model);
        }

        public IActionResult CreateIngredient()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }
            return View();
        }

        [HttpPost]
        public IActionResult CreateIngredient(Ingredient model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            if (ModelState.IsValid)
            {
                _context.Ingredient.Add(model);
                _context.SaveChanges();
                return RedirectToAction("IngredientList");
            }
            return View(model);
        }

        public IActionResult GetRecipeDetails(int id)
        {
            if (!IsAdmin())
            {
                return Content("Không có quyền truy cập.");
            }

            var recipe = _context.Recipe
                .Include(r => r.User)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .Include(r => r.Steps)
                .Include(r => r.Reviews)
                .FirstOrDefault(r => r.RecipeId == id);

            if (recipe == null)
            {
                return Content("Không tìm thấy công thức.");
            }

            var averageRating = recipe.Reviews.Any() ? Math.Round(recipe.Reviews.Average(r => r.Rating), 1) : 0;

            var html = $@"
                <div class='recipe-details-modal'>
                    <div class='recipe-header'>
                        <h4>{recipe.Title}</h4>
                        <div class='recipe-meta'>
                            <span><i class='fas fa-user'></i> {recipe.User?.FullName}</span>
                            <span><i class='fas fa-calendar'></i> {recipe.CreatedAt:dd/MM/yyyy}</span>
                            <span><i class='fas fa-star'></i> {averageRating}/5 ({recipe.Reviews.Count} đánh giá)</span>
                        </div>
                    </div>

                    <div class='recipe-info'>
                        <div class='info-item'>
                            <strong>Thời gian chuẩn bị:</strong> {recipe.PrepTime} phút
                        </div>
                        <div class='info-item'>
                            <strong>Thời gian nấu:</strong> {recipe.CookTime} phút
                        </div>
                        <div class='info-item'>
                            <strong>Khẩu phần:</strong> {recipe.Servings} người
                        </div>
                    </div>

                    <div class='recipe-description'>
                        <h5>Mô tả:</h5>
                        <p>{recipe.Description}</p>
                    </div>

                    <div class='recipe-ingredients'>
                        <h5>Nguyên liệu:</h5>
                        <ul>
                            {string.Join("", recipe.RecipeIngredients.Select(ri => $"<li>{ri.Quantity} {ri.Unit} {ri.Ingredient.Name}{(string.IsNullOrEmpty(ri.Notes) ? "" : $" ({ri.Notes})")}</li>"))}
                        </ul>
                    </div>

                    <div class='recipe-steps'>
                        <h5>Các bước thực hiện:</h5>
                        <ol>
                            {string.Join("", recipe.Steps.OrderBy(s => s.StepNumber).Select(s => $"<li>{s.Description}{(s.TimerInMinutes.HasValue ? $" <em>({s.TimerInMinutes} phút)</em>" : "")}</li>"))}
                        </ol>
                    </div>

                    {(string.IsNullOrEmpty(recipe.Note) ? "" : $"<div class='recipe-note'><h5>Ghi chú:</h5><p>{recipe.Note}</p></div>")}

                    {(string.IsNullOrEmpty(recipe.ImageUrl) ? "" : $"<div class='recipe-image'><img src='{recipe.ImageUrl}' alt='{recipe.Title}' style='max-width: 100%; height: auto;' /></div>")}
                </div>";

            return Content(html, "text/html");
        }

        public IActionResult GetIngredientDetails(int id)
        {
            if (!IsAdmin())
            {
                return Content("Không có quyền truy cập.");
            }

            var ingredient = _context.Ingredient.Find(id);
            if (ingredient == null)
            {
                return Content("Không tìm thấy nguyên liệu.");
            }

            var html = $@"
                <div class='ingredient-details-modal'>
                    <div class='ingredient-header'>
                        <h4>{ingredient.Name}</h4>
                    </div>

                    <div class='ingredient-info'>
                        <div class='info-row'>
                            <strong>ID:</strong> {ingredient.IngredientId}
                        </div>
                        <div class='info-row'>
                            <strong>Tên:</strong> {ingredient.Name}
                        </div>
                        {(string.IsNullOrEmpty(ingredient.Description) ? "" : $"<div class='info-row'><strong>Mô tả:</strong> {ingredient.Description}</div>")}
                        {(string.IsNullOrEmpty(ingredient.ImagePath) ? "" : $"<div class='ingredient-image'><img src='{ingredient.ImagePath}' alt='{ingredient.Name}' style='max-width: 100%; height: auto;' /></div>")}
                    </div>
                </div>";

            return Content(html, "text/html");
        }

        public IActionResult DeleteIngredient(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var ingredient = _context.Ingredient.Find(id);
            if (ingredient != null)
            {
                _context.Ingredient.Remove(ingredient);
                _context.SaveChanges();
            }
            return RedirectToAction("IngredientList");
        }

        public IActionResult ReviewList()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var reviews = _context.Reviews.Include(r => r.User).Include(r => r.Recipe).ToList();
            return View(reviews);
        }

        public IActionResult DeleteReview(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var review = _context.Reviews.Find(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                _context.SaveChanges();
            }
            return RedirectToAction("ReviewList");
        }
    }
}