using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models.ViewModels;
using CookingNotebookWebApp.Models;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Text.Encodings.Web;

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

        public IActionResult RecipeList(string searchTerm, string cookingMethodFilter, string typeOfDishFilter)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

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
            ViewBag.CookingMethods = cookingMethods;
            ViewBag.TypeOfDishes = typeOfDishes;

            return View(recipes.ToList());
        }

        [HttpGet]
        public IActionResult SearchRecipes(string term)
        {
            if (!IsAdmin())
            {
                return Json(new { error = "Unauthorized" });
            }

            if (string.IsNullOrEmpty(term))
            {
                return Json(new { results = new List<object>() });
            }

            var searchLower = term.ToLower();
            var recipes = _context.Recipe
                .Include(r => r.User)
                .Where(r => r.Title.ToLower().Contains(searchLower) ||
                           (r.Description != null && r.Description.ToLower().Contains(searchLower)))
                .Take(10)
                .Select(r => new {
                    id = r.RecipeId,
                    text = r.Title,
                    description = !string.IsNullOrEmpty(r.Description) && r.Description.Length > 50 ? r.Description.Substring(0, 50) + "..." : r.Description,
                    author = r.User != null ? r.User.FullName : ""
                })
                .ToList();

            return Json(new { results = recipes });
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

        ViewBag.Ingredients = _context.Ingredient.OrderBy(i => i.Name).ToList();

        var viewModel = new EditRecipeViewModel
        {
        Recipe = recipe,
            Ingredients = recipe.RecipeIngredients.ToList(),
                Steps = recipe.Steps.OrderBy(s => s.StepNumber).ToList()
        };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditRecipe(EditRecipeViewModel model, IFormFile imageFile)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            if (ModelState.IsValid)
            {
                model.Recipe.UpdatedAt = DateTime.Now;

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Create directory for recipe images
                    var recipeId = model.Recipe.RecipeId;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "dishes", recipeId.ToString());
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    model.Recipe.ImageUrl = $"/assets/dishes/{recipeId}/{fileName}";
                }

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
            ViewBag.Ingredients = _context.Ingredient.OrderBy(i => i.Name).ToList();
            var allIngredientOptions = string.Join("", ((List<Ingredient>)ViewBag.Ingredients).Select(i => $"<option value='{i.IngredientId}'>{i.Name}</option>"));
            ViewBag.AllIngredientOptions = allIngredientOptions;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipe(Recipe model, IFormFile imageFile)
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
                    model.UserId = 1;
                }

                // Handle image upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Create directory for recipe images
                    var recipeId = _context.Recipe.Max(r => (int?)r.RecipeId) ?? 0;
                    recipeId++;
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "dishes", recipeId.ToString());
                    Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    model.ImageUrl = $"/assets/dishes/{recipeId}/{fileName}";
                }

                _context.Recipe.Add(model);
                _context.SaveChanges();
                return RedirectToAction("RecipeList");
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult DeleteRecipe(int id, string confirmDelete)
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Homepage", "Homepage");
            }

            if (confirmDelete == "yes")
            {
                var recipe = _context.Recipe.Find(id);
                if (recipe != null)
                {
                    // Delete associated images
                    if (!string.IsNullOrEmpty(recipe.ImageUrl))
                    {
                        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", recipe.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    _context.Recipe.Remove(recipe);
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("RecipeList");
        }

        public IActionResult IngredientList(string searchTerm, string typeFilter, int page = 1, int pageSize = 10)
        {
        if (!IsAdmin())
        {
        return RedirectToAction("Homepage", "Homepage");
        }

        var ingredients = _context.Ingredient.AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
        var searchLower = searchTerm.ToLower();
        ingredients = ingredients.Where(i =>
        i.Name.ToLower().Contains(searchLower) ||
        (!string.IsNullOrEmpty(i.EnName) && i.EnName.ToLower().Contains(searchLower)) ||
        (!string.IsNullOrEmpty(i.ViNameWithoutAccents) && i.ViNameWithoutAccents.Contains(searchLower)) ||
        (!string.IsNullOrEmpty(i.Description) && i.Description.ToLower().Contains(searchLower)));
        }

        if (!string.IsNullOrEmpty(typeFilter))
        {
        ingredients = ingredients.Where(i => i.Type == typeFilter);
        }

        var totalItems = ingredients.Count();
            var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            var pagedIngredients = ingredients
                .OrderBy(i => i.IngredientId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Get distinct types for filter dropdown
            var types = _context.Ingredient
                .Where(i => !string.IsNullOrEmpty(i.Type))
                .Select(i => i.Type)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.TypeFilter = typeFilter;
            ViewBag.Types = types;

            return View(pagedIngredients);
        }

        [HttpGet]
        public IActionResult SearchIngredients(string term)
        {
            if (!IsAdmin())
            {
                return Json(new { error = "Unauthorized" });
            }

            if (string.IsNullOrEmpty(term))
            {
                return Json(new { results = new List<object>() });
            }

            var searchLower = term.ToLower();
            var ingredients = _context.Ingredient
                .Where(i =>
                    i.Name.ToLower().Contains(searchLower) ||
                    (!string.IsNullOrEmpty(i.EnName) && i.EnName.ToLower().Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(i.ViNameWithoutAccents) && i.ViNameWithoutAccents.Contains(searchLower)) ||
                    (!string.IsNullOrEmpty(i.Description) && i.Description.ToLower().Contains(searchLower)))
                .Take(10)
                .Select(i => new {
                    id = i.IngredientId,
                    text = i.Name + (!string.IsNullOrEmpty(i.EnName) ? $" ({i.EnName})" : ""),
                    name = i.Name,
                    enName = i.EnName,
                    description = i.Description
                })
                .ToList();

            return Json(new { results = ingredients });
        }



        public IActionResult GetIngredientCreateForm()
        {
            try
            {
                if (!IsAdmin())
                {
                    return Content("Không có quyền truy cập.");
                }

                var html = $@"
                <form id='createIngredientForm' enctype='multipart/form-data'>
                    <input type='hidden' name='ViNameWithoutAccents' id='viNameWithoutAccents' />
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Tên nguyên liệu:</label>
                        <input name='Name' id='ingredientName' class='form-control' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Loại nguyên liệu:</label>
                        <select name='Type' class='form-control' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>
                            <option value=''>-- Chọn loại --</option>
                            <option value='Thịt'>Thịt</option>
                            <option value='Rau'>Rau</option>
                            <option value='Củ'>Củ</option>
                            <option value='Trái cây'>Trái cây</option>
                            <option value='Gia vị'>Gia vị</option>
                            <option value='Ngũ cốc'>Ngũ cốc</option>
                            <option value='Sữa'>Sữa</option>
                            <option value='Đồ uống'>Đồ uống</option>
                            <option value='Khác'>Khác</option>
                        </select>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Tên tiếng Anh:</label>
                        <input name='EnName' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Mô tả:</label>
                        <textarea name='Description' class='form-control' rows='3' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'></textarea>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Hình ảnh:</label>
                        <input type='file' name='imageFile' accept='image/*' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; padding: 8px; border-radius: 4px; width: 100%;' />
                    </div>
                </form>
                <datalist id='ingredientList'></datalist>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi tải form: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateIngredientAjax(Ingredient model, IFormFile imageFile)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "ingredients");
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        model.ImagePath = $"/assets/ingredients/{fileName}";
                    }

                    _context.Ingredient.Add(model);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Thêm thành công." });
                }
                catch (Exception ex)
                {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                    Console.WriteLine("Error in CreateRecipeAjax: " + innerMessage);
                    return Json(new { success = false, message = "Lỗi khi thêm: " + innerMessage });
                }
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        public IActionResult GetRecipeCreateForm()
        {
            try
            {
                if (!IsAdmin())
                {
                    return Content("Không có quyền truy cập.");
                }

                var ingredients = _context.Ingredient.OrderBy(i => i.Name).ToList();
                var allIngredientOptions = string.Join("", ingredients.Select(i => $"<option value='{i.IngredientId}'>{HtmlEncoder.Default.Encode(i.Name)}</option>"));

                var html = $@"
                <div id='ingredientOptionsTemplate' style='display: none;'>
                    <option value=''>-- Chọn nguyên liệu --</option>
                    {allIngredientOptions}
                </div>
                <form id='createRecipeForm' enctype='multipart/form-data'>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Tiêu đề:</label>
                        <input name='Title' class='form-control' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Mô tả:</label>
                        <textarea name='Description' class='form-control' rows='3' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'></textarea>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Phương pháp nấu:</label>
                        <select name='Cooking_method' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>
                            <option value=''>-- Chọn phương pháp --</option>
                            <option value='Xào'>Xào</option>
                            <option value='Chiên / Rán'>Chiên / Rán</option>
                            <option value='Áp Chảo'>Áp Chảo</option>
                            <option value='Luộc'>Luộc</option>
                            <option value='Hấp'>Hấp</option>
                            <option value='Ninh / Hầm'>Ninh / Hầm</option>
                            <option value='Kho / Om / Rim'>Kho / Om / Rim</option>
                            <option value='Nấu (Canh/Súp)'>Nấu (Canh/Súp)</option>
                            <option value='Nướng'>Nướng</option>
                            <option value='Quay'>Quay</option>
                            <option value='Lẩu'>Lẩu</option>
                            <option value='Trộn'>Trộn</option>
                            <option value='Muối / Ngâm'>Muối / Ngâm</option>
                        </select>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Loại món:</label>
                        <select name='Type_of_dish' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>
                            <option value=''>-- Chọn loại món --</option>
                            <option value='Món chính'>Món chính</option>
                            <option value='Món phụ/Món ăn kèm'>Món phụ/Món ăn kèm</option>
                            <option value='Món Canh / Súp'>Món Canh / Súp</option>
                            <option value='Món Khai Vị'>Món Khai Vị</option>
                            <option value='Món Tráng Miệng'>Món Tráng Miệng</option>
                            <option value='Nước Chấm / Nước Sốt'>Nước Chấm / Nước Sốt</option>
                        </select>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Thời gian chuẩn bị (phút):</label>
                        <input name='PrepTime' type='number' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Thời gian nấu (phút):</label>
                        <input name='CookTime' type='number' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Khẩu phần:</label>
                        <input name='Servings' type='number' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Nguyên liệu:</label>
                        <div id='ingredientsContainer'>
                        <div class='ingredient-item' style='margin-bottom: 20px; padding: 15px; background: #1B263B; border-radius: 8px; border: 1px solid #3A475C; width: 100%; display: flex; flex-direction: row; align-items: center; gap: 10px; flex-wrap: wrap;'>
                        <div style='display: flex; align-items: center; gap: 5px; min-width: 100px;'>
                        <i class='fas fa-grip-vertical ingredient-drag-handle' style='color: #68C69F; cursor: grab;'></i>
                        <h6 style='color: #68C69F; margin: 0;'>Nguyên liệu 1</h6>
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Nguyên liệu:</label>
                        <input type='text' name='Ingredients[0].IngredientName' list='ingredientList' class='form-control' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 120px;' />
                        <input type='hidden' name='Ingredients[0].RecipeId' value='0' />
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Số lượng:</label>
                        <input name='Ingredients[0].Quantity' type='number' step='0.01' class='form-control' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 120px;' />
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Đơn vị:</label>
                        <select name='Ingredients[0].Unit' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 100px;'>
                        <option value=''>-- Chọn --</option>
                        <option value='g'>g</option>
                        <option value='thìa cà phê'>thìa cà phê</option>
                        <option value='thìa canh'>thìa canh</option>
                        <option value='tép'>tép</option>
                        <option value='quả'>quả</option>
                        <option value='củ'>củ</option>
                        <option value='kg'>kg</option>
                        </select>
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Ghi chú:</label>
                        <input name='Ingredients[0].Notes' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 200px;' />
                        </div>
                        <button type='button' class='btn btn-danger btn-sm' onclick='removeIngredient(this)' style='background: #D9534F; color: white; border: none; padding: 4px 8px; border-radius: 4px; font-size: 12px; margin-left: auto;'>Xóa</button>
                        </div>
                        </div>
                        <button type='button' id='addIngredientBtn' class='btn' style='background: #28a745; color: white; border: none; padding: 8px 16px; border-radius: 4px; margin-top: 10px;'>Thêm nguyên liệu</button>
                        </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Các bước thực hiện:</label>
                        <div id='stepsContainer'>
                        <div class='step-item' style='margin-bottom: 20px; padding: 15px; background: #1B263B; border-radius: 8px; border: 1px solid #3A475C; width: 100%; display: flex; flex-direction: row; align-items: center; gap: 10px; flex-wrap: wrap;'>
                        <div style='display: flex; align-items: center; gap: 5px; min-width: 100px;'>
                        <i class='fas fa-grip-vertical drag-handle' style='color: #68C69F; cursor: grab;'></i>
                        <h6 style='color: #68C69F; margin: 0;'>Bước 1</h6>
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Mô tả:</label>
                        <textarea name='Steps[0].Description' class='form-control' rows='3' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; min-height: 80px; width: 500px;'></textarea>
                        <input type='hidden' name='Steps[0].RecipeStepId' value='0' />
                        <input type='hidden' name='Steps[0].StepNumber' value='1' />
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Timer phút:</label>
                        <input name='Steps[0].TimerInMinutes' type='number' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 100px;' />
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Hình ảnh bước:</label>
                        <input type='file' name='stepImages[0]' accept='image/*' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; padding: 8px; border-radius: 4px; width: 200px;' />
                        </div>
                        <button type='button' class='btn btn-danger btn-sm' onclick='removeStep(this)' style='background: #D9534F; color: white; border: none; padding: 4px 8px; border-radius: 4px; font-size: 12px; margin-left: auto;'>Xóa</button>
                        </div>
                        </div>
                        <button type='button' id='addStepBtn' class='btn' style='background: #28a745; color: white; border: none; padding: 8px 16px; border-radius: 4px; margin-top: 10px;'>Thêm bước</button>
                        </div>
                        <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Ghi chú:</label>
                        <textarea name='Note' class='form-control' rows='2' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'></textarea>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Hình ảnh:</label>
                        <input type='file' name='imageFile' accept='image/*' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; padding: 8px; border-radius: 4px; width: 100%;' />
                    </div>
                </form>
                <datalist id='ingredientList'></datalist>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi tải form: " + ex.Message);
            }
        }

        public IActionResult GetRecipeEditForm(int id)
        {
            try
            {
                if (!IsAdmin())
                {
                    return Content("Không có quyền truy cập.");
                }

                var recipe = _context.Recipe
                    .Include(r => r.Steps)
                    .Include(r => r.RecipeIngredients)
                        .ThenInclude(ri => ri.Ingredient)
                    .AsSplitQuery()
                    .FirstOrDefault(r => r.RecipeId == id);
                if (recipe == null)
                {
                    return Content("Không tìm thấy công thức.");
                }

                var encoder = HtmlEncoder.Default;

                ViewBag.Ingredients = _context.Ingredient.OrderBy(i => i.Name).ToList();

                // Hidden options for JS
                var allIngredientOptions = string.Join("", ((List<Ingredient>)ViewBag.Ingredients).Select(i => $"<option value='{i.IngredientId}'>{encoder.Encode(i.Name)}</option>"));
                ViewBag.AllIngredientOptions = allIngredientOptions;
                var imageHtml = !string.IsNullOrEmpty(recipe.ImageUrl) ? $"<div class='current-image' style='margin-bottom: 15px;'><small style='color: #E0E1DD;'>Hình ảnh hiện tại:</small><br><div style='display: flex; align-items: center; gap: 10px;'><img src='{encoder.Encode(recipe.ImageUrl)}' alt='Current' style='width: 100px; height: 100px; object-fit: cover; border-radius: 4px; border: 1px solid #3A475C;'><button type='button' onclick='deleteCurrentImage()' class='btn btn-danger btn-sm' style='background: #D9534F; color: white; border: none; padding: 4px 8px; border-radius: 4px;'>Xóa ảnh</button></div></div><input type='hidden' name='deleteImage' value='false' id='deleteImageFlag' />" : "";

                var ingredientsHtml = "";
                if (recipe.RecipeIngredients != null && recipe.RecipeIngredients.Any())
                {
                    var ingredientIndex = 0;
                    foreach (var ri in recipe.RecipeIngredients)
                    {
                        var ingredientOptions = string.Join("", ((List<Ingredient>)ViewBag.Ingredients).Select(i => $"<option value='{i.IngredientId}' {(i.IngredientId == ri.IngredientId ? "selected" : "")}>{encoder.Encode(i.Name)}</option>"));
                        ingredientsHtml += $@"
                        <div class='ingredient-item' style='margin-bottom: 20px; padding: 15px; background: #1B263B; border-radius: 8px; border: 1px solid #3A475C; width: 100%; display: flex; flex-direction: row; align-items: center; gap: 10px; flex-wrap: wrap;'>
                        <div style='display: flex; align-items: center; gap: 5px; min-width: 100px;'>
                        <i class='fas fa-grip-vertical ingredient-drag-handle' style='color: #68C69F; cursor: grab;'></i>
                        <h6 style='color: #68C69F; margin: 0;'>Nguyên liệu {ingredientIndex + 1}</h6>
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                            <label style='color: #E0E1DD; font-weight: 500;'>Nguyên liệu:</label>
                            <input type='text' name='Ingredients[{ingredientIndex}].IngredientName' list='ingredientList' class='form-control' value='{encoder.Encode(ri.Ingredient?.Name ?? "")}' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 120px;' />
                            <input type='hidden' name='Ingredients[{ingredientIndex}].RecipeId' value='{ri.RecipeId}' />
                        <input type='hidden' name='Ingredients[{ingredientIndex}].IsDeleted' value='false' class='ingredient-is-deleted' />
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Số lượng:</label>
                        <input name='Ingredients[{ingredientIndex}].Quantity' type='number' step='0.01' class='form-control' value='{ri.Quantity}' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 120px;' />
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Đơn vị:</label>
                        <select name='Ingredients[{ingredientIndex}].Unit' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 100px;'>
                        <option value=''>-- Chọn --</option>
                        <option value='g' {(ri.Unit == "g" ? "selected" : "")}>g</option>
                        <option value='thìa cà phê' {(ri.Unit == "thìa cà phê" ? "selected" : "")}>thìa cà phê</option>
                        <option value='thìa canh' {(ri.Unit == "thìa canh" ? "selected" : "")}>thìa canh</option>
                        <option value='tép' {(ri.Unit == "tép" ? "selected" : "")}>tép</option>
                        <option value='quả' {(ri.Unit == "quả" ? "selected" : "")}>quả</option>
                        <option value='củ' {(ri.Unit == "củ" ? "selected" : "")}>củ</option>
                        <option value='kg' {(ri.Unit == "kg" ? "selected" : "")}>kg</option>
                        </select>
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Ghi chú:</label>
                        <input name='Ingredients[{ingredientIndex}].Notes' class='form-control' value='{encoder.Encode(ri.Notes ?? "")}' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 200px;' />
                        </div>
                        <button type='button' class='btn btn-danger btn-sm' onclick='removeIngredient(this)' style='background: #D9534F; color: white; border: none; padding: 4px 8px; border-radius: 4px; font-size: 12px; margin-left: auto;'>Xóa</button>
                        </div>
                        </div>";
                        ingredientIndex++;
                    }
                }

                var stepsHtml = "";
                if (recipe.Steps != null && recipe.Steps.Any())
                {
                    var stepIndex = 0;
                    foreach (var step in recipe.Steps.OrderBy(s => s.StepNumber))
                    {
                        var stepImageHtml = !string.IsNullOrEmpty(step.ImageUrl) ? $"<div class='current-image' style='margin-bottom: 15px;'><small style='color: #E0E1DD;'>Hình ảnh bước hiện tại:</small><br><div style='display: flex; align-items: center; justify-content: center; width: 100px; height: 100px; background: #2A3441; border-radius: 6px; border: 1px solid #3A475C; margin-top: 5px;'><img src='{encoder.Encode(step.ImageUrl)}' alt='Current Step' style='width: 90px; height: 90px; object-fit: cover; border-radius: 4px;'></div></div>" : "";
                        stepsHtml += $@"
                        <div class='step-item' style='margin-bottom: 20px; padding: 15px; background: #1B263B; border-radius: 8px; border: 1px solid #3A475C; width: 100%; display: flex; flex-direction: row; align-items: center; gap: 10px; flex-wrap: wrap;'>
                        <div style='display: flex; align-items: center; gap: 5px; min-width: 100px;'>
                        <i class='fas fa-grip-vertical drag-handle' style='color: #68C69F; cursor: grab;'></i>
                        <h6 style='color: #68C69F; margin: 0;'>Bước {step.StepNumber}</h6>
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Mô tả:</label>
                        <textarea name='Steps[{stepIndex}].Description' class='form-control' rows='3' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; min-height: 80px; width: 500px;'>{encoder.Encode(step.Description)}</textarea>
                        <input type='hidden' name='Steps[{stepIndex}].RecipeStepId' value='{step.RecipeStepId}' />
                        <input type='hidden' name='Steps[{stepIndex}].StepNumber' value='{step.StepNumber}' />
                        <input type='hidden' name='Steps[{stepIndex}].IsDeleted' value='false' class='is-deleted' />
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Timer phút:</label>
                        <input name='Steps[{stepIndex}].TimerInMinutes' type='number' class='form-control' value='{step.TimerInMinutes}' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; width: 100px;' />
                        </div>
                        <div class='form-group' style='display: flex; align-items: center; gap: 5px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Hình ảnh bước:</label>
                        <input type='file' name='stepImages[{stepIndex}]' accept='image/*' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; padding: 8px; border-radius: 4px; width: 200px;' />
                        {stepImageHtml}
                        </div>
                        <button type='button' class='btn btn-danger btn-sm' onclick='removeStep(this)' style='background: #D9534F; color: white; border: none; padding: 4px 8px; border-radius: 4px; font-size: 12px; margin-left: auto;'>Xóa</button>
                        </div>
                        </div>";
                        stepIndex++;
                    }
                }

                var html = $@"
                <div id='ingredientOptionsTemplate' style='display: none;'>
                    <option value=''>-- Chọn nguyên liệu --</option>
                    {ViewBag.AllIngredientOptions}
                </div>
                <form id='editRecipeForm' enctype='multipart/form-data'>
                    <input type='hidden' name='RecipeId' value='{recipe.RecipeId}' />
                    <input type='hidden' name='UserId' value='{recipe.UserId}' />
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Tiêu đề:</label>
                        <input name='Title' class='form-control' required value='{encoder.Encode(recipe.Title)}' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Mô tả:</label>
                        <textarea name='Description' class='form-control' rows='3' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>{encoder.Encode(recipe.Description ?? "")}</textarea>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Phương pháp nấu:</label>
                        <select name='Cooking_method' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>
                            <option value=''>-- Chọn phương pháp --</option>
                            <option value='Xào' {(recipe.Cooking_method == "Xào" ? "selected" : "")}>Xào</option>
                            <option value='Chiên / Rán' {(recipe.Cooking_method == "Chiên / Rán" ? "selected" : "")}>Chiên / Rán</option>
                            <option value='Áp Chảo' {(recipe.Cooking_method == "Áp Chảo" ? "selected" : "")}>Áp Chảo</option>
                            <option value='Luộc' {(recipe.Cooking_method == "Luộc" ? "selected" : "")}>Luộc</option>
                            <option value='Hấp' {(recipe.Cooking_method == "Hấp" ? "selected" : "")}>Hấp</option>
                            <option value='Ninh / Hầm' {(recipe.Cooking_method == "Ninh / Hầm" ? "selected" : "")}>Ninh / Hầm</option>
                            <option value='Kho / Om / Rim' {(recipe.Cooking_method == "Kho / Om / Rim" ? "selected" : "")}>Kho / Om / Rim</option>
                            <option value='Nấu (Canh/Súp)' {(recipe.Cooking_method == "Nấu (Canh/Súp)" ? "selected" : "")}>Nấu (Canh/Súp)</option>
                            <option value='Nướng' {(recipe.Cooking_method == "Nướng" ? "selected" : "")}>Nướng</option>
                            <option value='Quay' {(recipe.Cooking_method == "Quay" ? "selected" : "")}>Quay</option>
                            <option value='Lẩu' {(recipe.Cooking_method == "Lẩu" ? "selected" : "")}>Lẩu</option>
                            <option value='Trộn' {(recipe.Cooking_method == "Trộn" ? "selected" : "")}>Trộn</option>
                            <option value='Muối / Ngâm' {(recipe.Cooking_method == "Muối / Ngâm" ? "selected" : "")}>Muối / Ngâm</option>
                        </select>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Loại món:</label>
                        <select name='Type_of_dish' class='form-control' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>
                            <option value=''>-- Chọn loại món --</option>
                            <option value='Món chính' {(recipe.Type_of_dish == "Món chính" ? "selected" : "")}>Món chính</option>
                            <option value='Món phụ/Món ăn kèm' {(recipe.Type_of_dish == "Món phụ/Món ăn kèm" ? "selected" : "")}>Món phụ/Món ăn kèm</option>
                            <option value='Món Canh / Súp' {(recipe.Type_of_dish == "Món Canh / Súp" ? "selected" : "")}>Món Canh / Súp</option>
                            <option value='Món Khai Vị' {(recipe.Type_of_dish == "Món Khai Vị" ? "selected" : "")}>Món Khai Vị</option>
                            <option value='Món Tráng Miệng' {(recipe.Type_of_dish == "Món Tráng Miệng" ? "selected" : "")}>Món Tráng Miệng</option>
                            <option value='Nước Chấm / Nước Sốt' {(recipe.Type_of_dish == "Nước Chấm / Nước Sốt" ? "selected" : "")}>Nước Chấm / Nước Sốt</option>
                        </select>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Thời gian chuẩn bị (phút):</label>
                        <input name='PrepTime' type='number' class='form-control' value='{recipe.PrepTime}' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Thời gian nấu (phút):</label>
                        <input name='CookTime' type='number' class='form-control' value='{recipe.CookTime}' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Khẩu phần:</label>
                        <input name='Servings' type='number' class='form-control' value='{recipe.Servings}' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Nguyên liệu:</label>
                        <div id='ingredientsContainer'>
                            {ingredientsHtml}
                        </div>
                        <button type='button' id='addIngredientBtn' class='btn' style='background: #28a745; color: white; border: none; padding: 8px 16px; border-radius: 4px; margin-top: 10px;'>Thêm nguyên liệu</button>
                        </div>
                        <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Các bước thực hiện:</label>
                        <div id='stepsContainer'>
                        {stepsHtml}
                        </div>
                        <button type='button' id='addStepBtn' class='btn' style='background: #28a745; color: white; border: none; padding: 8px 16px; border-radius: 4px; margin-top: 10px;'>Thêm bước</button>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Ghi chú:</label>
                        <textarea name='Note' class='form-control' rows='2' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>{encoder.Encode(recipe.Note ?? "")}</textarea>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Hình ảnh mới (tùy chọn):</label>
                        <input type='file' name='imageFile' accept='image/*' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; padding: 8px; border-radius: 4px; width: 100%;' />
                        {imageHtml}
                    </div>
                </form>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi tải form: " + ex.Message);
            }
        }

        public IActionResult GetRecipeDetails(int id)
        {
            try
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
                            <div class='ingredients-list' style='display: flex; flex-wrap: wrap; gap: 15px;'>
                                {(recipe.RecipeIngredients != null ? string.Join("", recipe.RecipeIngredients.Where(ri => ri.Ingredient != null).Select(ri => $"<div class='ingredient-item' style='background: #2A3441; padding: 10px; border-radius: 6px; border: 1px solid #3A475C; min-width: 200px;'>{ri.Quantity} {ri.Unit} {ri.Ingredient!.Name}{(string.IsNullOrEmpty(ri.Notes) ? "" : $" ({ri.Notes})")}</div>")) : "")}
                            </div>
                        </div>

                        <div class='recipe-steps'>
                            <h5>Các bước thực hiện:</h5>
                            <div class='steps-list' style='display: flex; flex-direction: column; gap: 15px;'>
                                {(recipe.Steps != null ? string.Join("", recipe.Steps.OrderBy(s => s.StepNumber).Select(s => $"<div class='step-item' style='background: #2A3441; padding: 15px; border-radius: 8px; border: 1px solid #3A475C; width: 100%;'><div class='step-header' style='display: flex; align-items: center; gap: 10px; margin-bottom: 10px;'><div class='step-number' style='background: #68C69F; color: #1B263B; padding: 5px 10px; border-radius: 4px; font-weight: bold;'>Bước {s.StepNumber}</div></div><div class='step-content'><p>{s.Description}{(s.TimerInMinutes.HasValue ? $" <em>({s.TimerInMinutes} phút)</em>" : "")}</p>{(string.IsNullOrEmpty(s.ImageUrl) ? "" : $"<div class='step-image' style='margin-top: 10px;'><img src='{s.ImageUrl}' alt='Bước {s.StepNumber}' title='Ảnh bước {s.StepNumber}' style='max-width: 200px; height: auto;' /></div>")}</div></div>")) : "")}
                            </div>
                        </div>

                        {(string.IsNullOrEmpty(recipe.Note) ? "" : $"<div class='recipe-note'><h5>Ghi chú:</h5><p>{recipe.Note}</p></div>")}

                        {(string.IsNullOrEmpty(recipe.ImageUrl) ? "" : $"<div class='recipe-image'><img src='{recipe.ImageUrl}' alt='{recipe.Title}' style='max-width: 100%; height: auto;' /></div>")}
                    </div>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi tải thông tin công thức: " + ex.Message);
            }
        }

        public IActionResult GetIngredientDetails(int id)
        {
            try
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

                var encoder = HtmlEncoder.Default;
                var imageHtml = string.IsNullOrEmpty(ingredient.ImagePath) ? "" : $"<div class='ingredient-image'><img src='{encoder.Encode(ingredient.ImagePath)}' alt='{encoder.Encode(ingredient.Name)}' /></div>";
                var enNameHtml = string.IsNullOrEmpty(ingredient.EnName) ? "" : $"<div class='info-row'><strong>Tên tiếng Anh:</strong><span>{encoder.Encode(ingredient.EnName)}</span></div>";
                var typeHtml = string.IsNullOrEmpty(ingredient.Type) ? "" : $"<div class='info-row'><strong>Phân loại:</strong><span>{encoder.Encode(ingredient.Type)}</span></div>";
                var descriptionHtml = string.IsNullOrEmpty(ingredient.Description) ? "" : $"<div class='info-row description-row'><strong>Mô tả:</strong><span>{encoder.Encode(ingredient.Description)}</span></div>";

                var html = $@"
                <div class='ingredient-details-content'>
                {imageHtml}

                    <div class='ingredient-info'>
                        <div class='info-row'>
                            <strong>Tên tiếng Việt:</strong>
                            <span>{encoder.Encode(ingredient.Name)}</span>
                        </div>

                        {enNameHtml}
                        {typeHtml}
                        {descriptionHtml}
                    </div>
                </div>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi tải thông tin: " + ex.Message);
            }
        }

        public IActionResult GetIngredientEditForm(int id)
        {
            try
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

                var encoder = HtmlEncoder.Default;
                var imageHtml = !string.IsNullOrEmpty(ingredient.ImagePath) ? $"<div class='current-image' style='margin-bottom: 15px;'><small style='color: #E0E1DD;'>Hình ảnh hiện tại:</small><br><div style='display: flex; align-items: center; justify-content: center; width: 100px; height: 100px; background: #2A3441; border-radius: 6px; border: 1px solid #3A475C; margin-top: 5px;'><img src='{encoder.Encode(ingredient.ImagePath)}' alt='Current' style='width: 90px; height: 90px; object-fit: cover; border-radius: 4px;'></div></div>" : "";

                var html = $@"
                <form id='editIngredientForm' enctype='multipart/form-data'>
                    <input type='hidden' name='IngredientId' value='{ingredient.IngredientId}' />
                    <input type='hidden' name='ViNameWithoutAccents' id='viNameWithoutAccents' value='{encoder.Encode(ingredient.ViNameWithoutAccents ?? "")}' />
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Tên nguyên liệu:</label>
                        <input name='Name' id='ingredientName' class='form-control' required value='{encoder.Encode(ingredient.Name)}' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Loại nguyên liệu:</label>
                        <select name='Type' class='form-control' required style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>
                            <option value=''>-- Chọn loại --</option>
                            <option value='Thịt' {(ingredient.Type == "Thịt" ? "selected" : "")}>Thịt</option>
                            <option value='Rau' {(ingredient.Type == "Rau" ? "selected" : "")}>Rau</option>
                            <option value='Củ' {(ingredient.Type == "Củ" ? "selected" : "")}>Củ</option>
                            <option value='Trái cây' {(ingredient.Type == "Trái cây" ? "selected" : "")}>Trái cây</option>
                            <option value='Gia vị' {(ingredient.Type == "Gia vị" ? "selected" : "")}>Gia vị</option>
                            <option value='Ngũ cốc' {(ingredient.Type == "Ngũ cốc" ? "selected" : "")}>Ngũ cốc</option>
                            <option value='Sữa' {(ingredient.Type == "Sữa" ? "selected" : "")}>Sữa</option>
                            <option value='Đồ uống' {(ingredient.Type == "Đồ uống" ? "selected" : "")}>Đồ uống</option>
                            <option value='Khác' {(ingredient.Type == "Khác" ? "selected" : "")}>Khác</option>
                        </select>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Tên tiếng Anh:</label>
                        <input name='EnName' class='form-control' value='{encoder.Encode(ingredient.EnName ?? "")}' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;' />
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Mô tả:</label>
                        <textarea name='Description' class='form-control' rows='3' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C;'>{encoder.Encode(ingredient.Description ?? "")}</textarea>
                    </div>
                    <div class='form-group' style='margin-bottom: 15px;'>
                        <label style='color: #E0E1DD; font-weight: 500;'>Hình ảnh mới (tùy chọn):</label>
                        <input type='file' name='imageFile' accept='image/*' style='background: #2A3441; color: #E0E1DD; border: 1px solid #3A475C; padding: 8px; border-radius: 4px; width: 100%;' />
                        {imageHtml}
                    </div>
                </form>";

                return Content(html, "text/html");
            }
            catch (Exception ex)
            {
                return Content("Lỗi khi tải form: " + ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditIngredientAjax(Ingredient model, IFormFile imageFile)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "ingredients");
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        model.ImagePath = $"/assets/ingredients/{fileName}";
                    }

                    _context.Ingredient.Update(model);
                    _context.SaveChanges();
                    return Json(new { success = true, message = "Cập nhật thành công." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        [HttpPost]
        [Route("[controller]/[action]")]
        public IActionResult DeleteIngredientAjax([FromForm] int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            try
            {
                var ingredient = _context.Ingredient.Find(id);
                if (ingredient == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy nguyên liệu với ID: " + id });
                }

                // Delete associated images
                if (!string.IsNullOrEmpty(ingredient.ImagePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", ingredient.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Ingredient.Remove(ingredient);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xoá thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xoá: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecipeAjax([FromForm] Recipe model, IFormFile imageFile, IFormFile[] stepImages, List<RecipeIngredient> Ingredients, List<RecipeStep> Steps)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            // Remove validation errors for navigation properties and parameters
            ModelState.Remove("User");
            ModelState.Remove("Reviews");
            ModelState.Remove("Favorites");
            ModelState.Remove("RecipeIngredients");
            ModelState.Remove("Steps");
            ModelState.Remove("imageFile");
            ModelState.Remove("stepImages");

            if (ModelState.IsValid)
            {
                try
                {
                    model.CreatedAt = DateTime.Now;
                    var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
                    {
                        model.UserId = userId;
                    }
                    else
                    {
                        model.UserId = 1; // Default admin
                    }

                    // Handle image upload
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "dishes");
                        Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        model.ImageUrl = $"/assets/dishes/{fileName}";
                    }

                    _context.Recipe.Add(model);
                    _context.SaveChanges(); // Save to generate RecipeId

                    // Handle ingredients
                    var addedIngredientIds = new HashSet<int>();
                    if (Ingredients != null && Ingredients.Any())
                    {
                    foreach (var ri in Ingredients)
                    {
                    Console.WriteLine($"Ingredient: Name={ri.IngredientName}, Qty={ri.Quantity}");
                    if (!string.IsNullOrEmpty(ri.IngredientName) && ri.Quantity > 0)
                    {
                    Ingredient ingredient = null;
                    if (int.TryParse(ri.IngredientName, out int id))
                    {
                        ingredient = _context.Ingredient.Find(id);
                    }
                    else
                    {
                        ingredient = _context.Ingredient.FirstOrDefault(i => i.Name == ri.IngredientName);
                    }
                    if (ingredient != null && !addedIngredientIds.Contains(ingredient.IngredientId))
                    {
                    ri.IngredientId = ingredient.IngredientId;
                    ri.RecipeId = model.RecipeId;
                    ri.Unit = ri.Unit ?? ""; // Ensure Unit is not null
                        _context.RecipeIngredients.Add(ri);
                        addedIngredientIds.Add(ingredient.IngredientId);
                        Console.WriteLine("Added ingredient");
                    }
                    else if (addedIngredientIds.Contains(ingredient?.IngredientId ?? 0))
                    {
                        Console.WriteLine("Skipped duplicate ingredient");
                    }
                    else
                        {
                                Console.WriteLine("Ingredient not found in DB");
                                }
                        }
                        }
                    }

                    // Handle steps
                    if (Steps != null && Steps.Any())
                    {
                        foreach (var step in Steps.OrderBy(s => s.StepNumber))
                    {
                        Console.WriteLine($"Step: Description={step.Description}, StepNumber={step.StepNumber}");
                    if (!string.IsNullOrEmpty(step.Description))
                    {
                        // Check if step already exists in DB
                    var existingStep = _context.RecipeSteps.FirstOrDefault(s => s.RecipeId == model.RecipeId && s.StepNumber == step.StepNumber);
                    if (existingStep == null)
                    {
                        step.RecipeId = model.RecipeId;
                            _context.RecipeSteps.Add(step);
                            Console.WriteLine("Added step");
                        }
                    else
                        {
                                Console.WriteLine("Step already exists, skipped");
                                }
                        }
                    }
                }

                // Handle step images
                if (stepImages != null && stepImages.Length > 0 && Steps != null)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "steps");
                    Directory.CreateDirectory(uploadsFolder);

                    for (int i = 0; i < stepImages.Length; i++)
                    {
                        if (stepImages[i] != null && stepImages[i].Length > 0 && i < Steps.Count)
                        {
                            var step = Steps[i];
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(stepImages[i].FileName);
                            var filePath = Path.Combine(uploadsFolder, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await stepImages[i].CopyToAsync(stream);
                            }

                            step.ImageUrl = "/assets/steps/" + fileName;
                        }
                    }
                }

                _context.SaveChanges();
                    return Json(new { success = true, message = "Thêm thành công." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = "Lỗi khi thêm: " + ex.Message });
                }
            }
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        [HttpPost]
        public async Task<IActionResult> EditRecipeAjax(Recipe model, IFormFile imageFile, IFormFile[] stepImages, [FromForm] List<RecipeIngredient> Ingredients, [FromForm] List<RecipeStep> Steps, bool deleteImage = false)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            // Remove validation errors for navigation properties
            ModelState.Remove("User");
            ModelState.Remove("Reviews");
            ModelState.Remove("Favorites");
            ModelState.Remove("RecipeIngredients");
            ModelState.Remove("stepImages"); // parameter
            ModelState.Remove("imageFile"); // parameter
            // Remove for Ingredients array
            var ingredientKeys = ModelState.Keys.Where(k => k.StartsWith("Ingredients[")).ToList();
            foreach (var key in ingredientKeys)
            {
                ModelState.Remove(key);
            }
            // Remove for Steps navigation
            var stepKeys = ModelState.Keys.Where(k => k.StartsWith("Steps[")).ToList();
            foreach (var key in stepKeys)
            {
                ModelState.Remove(key);
            }

            if (ModelState.IsValid)
            {
                // Model is valid
            }
            else
            {
                // Log remaining ModelState errors
                foreach (var ms in ModelState)
                {
                    if (ms.Value.Errors.Any())
                    {
                        var errorMessages = ms.Value.Errors.Select(e => e.ErrorMessage);
                        Console.WriteLine($"ModelState Error - Field: {ms.Key}, Errors: {string.Join(", ", errorMessages)}");
                    }
                }
                return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
            }

            try
            {
                Console.WriteLine("Starting EditRecipeAjax for RecipeId: " + model.RecipeId);
                model.UpdatedAt = DateTime.Now;

                // Handle image delete
                if (deleteImage && !string.IsNullOrEmpty(model.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", model.ImageUrl.TrimStart('/'));
                    Console.WriteLine("Deleting recipe image at: " + imagePath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                        Console.WriteLine("Recipe image deleted successfully");
                    }
                    else
                    {
                        Console.WriteLine("Recipe image file not found");
                    }
                    model.ImageUrl = null;
                }
                // Handle image upload
                else if (imageFile != null && imageFile.Length > 0)
                {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "dishes");
                Directory.CreateDirectory(uploadsFolder);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        model.ImageUrl = $"/assets/dishes/{fileName}";
                    }

                    _context.Recipe.Update(model);
                    _context.SaveChanges();

                    // Handle ingredients
                    var addedIngredientIds = new HashSet<int>();
                    if (Ingredients != null && Ingredients.Any())
                    {
                    foreach (var ri in Ingredients)
                    {
                    Console.WriteLine($"Edit Ingredient: Name={ri.IngredientName}, Qty={ri.Quantity}, IsDeleted={ri.IsDeleted}");
                    if (ri.IsDeleted)
                    {
                    // Remove deleted ingredient
                    var existingRi = _context.RecipeIngredients.FirstOrDefault(ri2 => ri2.RecipeId == model.RecipeId && ri2.IngredientId == ri.IngredientId);
                    if (existingRi != null)
                    {
                        _context.RecipeIngredients.Remove(existingRi);
                        Console.WriteLine("Removed ingredient in edit");
                    }
                    }
                    else if (!string.IsNullOrEmpty(ri.IngredientName) && ri.Quantity > 0)
                    {
                    Ingredient ingredient = null;
                    if (int.TryParse(ri.IngredientName, out int id))
                    {
                    ingredient = _context.Ingredient.Find(id);
                    }
                    else
                    {
                    ingredient = _context.Ingredient.FirstOrDefault(i => i.Name == ri.IngredientName);
                    }
                    if (ingredient != null && !addedIngredientIds.Contains(ingredient.IngredientId))
                    {
                    var existingRi = _context.RecipeIngredients.FirstOrDefault(ri2 => ri2.RecipeId == model.RecipeId && ri2.IngredientId == ingredient.IngredientId);
                    if (existingRi == null)
                    {
                        ri.IngredientId = ingredient.IngredientId;
                            ri.RecipeId = model.RecipeId;
                            ri.Unit = ri.Unit ?? "";
                            _context.RecipeIngredients.Add(ri);
                        Console.WriteLine("Added ingredient in edit");
                        }
                        else
                        {
                        Console.WriteLine("Ingredient already exists in edit");
                        }
                            addedIngredientIds.Add(ingredient.IngredientId);
                            }
                                else if (addedIngredientIds.Contains(ingredient?.IngredientId ?? 0))
                                {
                                    Console.WriteLine("Skipped duplicate ingredient in edit");
                                }
                                else
                            {
                                Console.WriteLine("Ingredient not found in DB for edit");
                        }
                    }
                    }
                    }

                    // Handle steps
                    if (Steps != null && Steps.Any())
                    {
                    foreach (var step in Steps.OrderBy(s => s.StepNumber))
                    {
                    Console.WriteLine($"Edit Step: Description={step.Description}, StepNumber={step.StepNumber}, IsDeleted={step.IsDeleted}");
                    if (step.IsDeleted)
                    {
                    // Remove deleted step
                    var existingStep = _context.RecipeSteps.FirstOrDefault(s => s.RecipeId == model.RecipeId && s.StepNumber == step.StepNumber);
                        if (existingStep != null)
                            {
                                    _context.RecipeSteps.Remove(existingStep);
                                    Console.WriteLine("Removed step in edit");
                                }
                            }
                            else if (!string.IsNullOrEmpty(step.Description))
                            {
                                // Check if step already exists in DB
                                var existingStep = _context.RecipeSteps.FirstOrDefault(s => s.RecipeId == model.RecipeId && s.StepNumber == step.StepNumber);
                                if (existingStep == null)
                                {
                                    step.RecipeId = model.RecipeId;
                                    _context.RecipeSteps.Add(step);
                                    Console.WriteLine("Added step in edit");
                                }
                                else
                                {
                                    Console.WriteLine("Step already exists, skipped in edit");
                                }
                            }
                        }
                    }

                    // Handle step images
                    if (stepImages != null && stepImages.Length > 0 && Steps != null)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "steps");
                        Directory.CreateDirectory(uploadsFolder);

                        for (int i = 0; i < stepImages.Length; i++)
                        {
                            if (stepImages[i] != null && stepImages[i].Length > 0 && i < Steps.Count)
                            {
                                var step = Steps[i];
                                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(stepImages[i].FileName);
                                var filePath = Path.Combine(uploadsFolder, fileName);

                                using (var stream = new FileStream(filePath, FileMode.Create))
                                {
                                    await stepImages[i].CopyToAsync(stream);
                                }

                                step.ImageUrl = $"/assets/steps/{fileName}";

                                // Update existing step if it exists
                                var existingStep = _context.RecipeSteps.FirstOrDefault(s => s.RecipeId == model.RecipeId && s.StepNumber == step.StepNumber);
                                if (existingStep != null)
                                {
                                    existingStep.ImageUrl = step.ImageUrl;
                                    _context.RecipeSteps.Update(existingStep);
                                    Console.WriteLine($"Updated step image for StepNumber {step.StepNumber} in edit");
                                }
                                else
                                {
                                    Console.WriteLine($"No existing step found for StepNumber {step.StepNumber} to update image");
                                }
                            }
                        }
                    }

                    Console.WriteLine("Saving changes...");
                    _context.SaveChanges();
                    Console.WriteLine("Save successful.");
                    return Json(new { success = true, message = "Cập nhật thành công." });
                }
                catch (Exception ex)
                {
                    // Log the error for debugging
                    Console.WriteLine("Error in EditRecipeAjax: " + ex.Message);
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                    }
                    Console.WriteLine("Stack Trace: " + ex.StackTrace);
                    return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
                }
            }

        [HttpPost]
        public IActionResult DeleteRecipeAjax([FromForm] int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            try
            {
            var recipe = _context.Recipe
                .Include(r => r.RecipeIngredients)
                .Include(r => r.Steps)
            .Include(r => r.Reviews)
                .Include(r => r.Favorites)
                    .FirstOrDefault(r => r.RecipeId == id);
            if (recipe == null)
            {
            return Json(new { success = false, message = "Không tìm thấy công thức với ID: " + id });
            }

            // Delete associated entities
            if (recipe.RecipeIngredients != null)
                _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
            if (recipe.Steps != null)
                    _context.RecipeSteps.RemoveRange(recipe.Steps);
                if (recipe.Reviews != null)
                    _context.Reviews.RemoveRange(recipe.Reviews);
                if (recipe.Favorites != null)
                    _context.Favorites.RemoveRange(recipe.Favorites);

                // Delete associated images
                if (!string.IsNullOrEmpty(recipe.ImageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", recipe.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Recipe.Remove(recipe);
                _context.SaveChanges();

                return Json(new { success = true, message = "Xoá thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xoá: " + ex.Message });
            }
        }

        [HttpPost]
        public IActionResult DeleteStepImage([FromForm] int stepId, [FromForm] string imageUrl)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Không có quyền truy cập." });
            }

            try
            {
                // Update DB to set ImageUrl = null
                var step = _context.RecipeSteps.Find(stepId);
                if (step != null)
                {
                    step.ImageUrl = null;
                    _context.RecipeSteps.Update(step);
                    _context.SaveChanges();
                }

                // Delete file
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageUrl.TrimStart('/'));
                    Console.WriteLine("Deleting step image at: " + imagePath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                        Console.WriteLine("Step image deleted successfully");
                    }
                    else
                    {
                        Console.WriteLine("Step image file not found");
                    }
                }

                return Json(new { success = true, message = "Xóa ảnh bước thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
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