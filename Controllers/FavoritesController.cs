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

        public IActionResult Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var favoriteRecipes = _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Recipe)
                    .ThenInclude(r => r.User)
                .Select(f => f.Recipe)
                .ToList();

            return View(favoriteRecipes);
        }
    }
}
