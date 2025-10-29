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

        public IActionResult RecipeList()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var recipes = _context.Recipe.Include(r => r.User).ToList();
            return View(recipes);
        }

        public IActionResult EditRecipe(int id)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var recipe = _context.Recipe.Find(id);
            if (recipe == null)
            {
                return NotFound();
            }
            return View(recipe);
        }

        [HttpPost]
        public IActionResult EditRecipe(Recipe model)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            if (ModelState.IsValid)
            {
                _context.Recipe.Update(model);
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

        public IActionResult IngredientList()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            var ingredients = _context.Ingredient.ToList();
            return View(ingredients);
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