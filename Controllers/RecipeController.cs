using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}