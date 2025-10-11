using Microsoft.AspNetCore.Mvc;

namespace CookingNotebookWebApp.Controllers
{
    public class HomepageController : Controller
    {
        public IActionResult Homepage()
        {
            return View();
        }
    }
}