using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CookingNotebookWebApp.Controllers
{
    public class IngredientController : Controller
    {
        private readonly AppDbContext _context;

        public IngredientController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var ingredients = _context.Ingredient.ToList();
            return View(ingredients);
        }
    }
}