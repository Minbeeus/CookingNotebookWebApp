using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CookingNotebookWebApp.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly AppDbContext _context;

        public FavoritesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string searchTerm, string cookingMethodFilter, string typeOfDishFilter, string timeFilter)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var favoriteRecipesList = _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Recipe)
            .ThenInclude(r => r.User)
            .Select(f => f.Recipe)
            .ToList();

            var favoriteRecipes = favoriteRecipesList.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                favoriteRecipes = favoriteRecipes.Where(r => r.Title.ToLower().Contains(searchLower) || (r.Description != null && r.Description.ToLower().Contains(searchLower)));
            }

            if (!string.IsNullOrEmpty(cookingMethodFilter))
            {
                favoriteRecipes = favoriteRecipes.Where(r => r.Cooking_method == cookingMethodFilter);
            }
            if (!string.IsNullOrEmpty(typeOfDishFilter))
            {
                favoriteRecipes = favoriteRecipes.Where(r => r.Type_of_dish == typeOfDishFilter);
            }

            if (!string.IsNullOrEmpty(timeFilter) && timeFilter != "all")
            {
                favoriteRecipesList = favoriteRecipes.ToList(); // Convert to list before applying time filter
                switch (timeFilter)
                {
                    case "15":
                        favoriteRecipesList = favoriteRecipesList.Where(r => r.PrepTime + r.CookTime < 15).ToList();
                        break;
                    case "30":
                        favoriteRecipesList = favoriteRecipesList.Where(r => r.PrepTime + r.CookTime >= 15 && r.PrepTime + r.CookTime <= 30).ToList();
                        break;
                    case "60":
                        favoriteRecipesList = favoriteRecipesList.Where(r => r.PrepTime + r.CookTime > 30 && r.PrepTime + r.CookTime <= 60).ToList();
                        break;
                    case "120":
                        favoriteRecipesList = favoriteRecipesList.Where(r => r.PrepTime + r.CookTime > 60).ToList();
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

            return View(favoriteRecipesList);
        }
    }
}
