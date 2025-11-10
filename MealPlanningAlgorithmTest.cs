using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Services;
using Microsoft.EntityFrameworkCore;

namespace CookingNotebookWebApp
{
    /// <summary>
    /// File test thuật toán Meal Planning
    /// Chạy: dotnet run --project CookingNotebookWebApp.csproj
    /// Sau đó gọi các hàm test từ Program.cs hoặc Controller
    /// </summary>
    public class MealPlanningAlgorithmTest
    {
        private readonly AppDbContext _context;
        private readonly MealPlanningService _mealPlanningService;

        public MealPlanningAlgorithmTest(AppDbContext context, MealPlanningService mealPlanningService)
        {
            _context = context;
            _mealPlanningService = mealPlanningService;
        }

        /// <summary>
        /// Test 1: Kiểm tra dữ liệu cơ bản trong database
        /// </summary>
        public async Task Test1_CheckDatabaseData()
        {
            Console.WriteLine("=== TEST 1: Kiểm tra dữ liệu cơ bản ===\n");

            try
            {
                var totalRecipes = await _context.Recipe.CountAsync();
                var totalUsers = await _context.Users.CountAsync();
                var totalIngredients = await _context.Ingredient.CountAsync();
                var totalMealTimes = await _context.MealTimes.CountAsync();
                var totalFavorites = await _context.Favorites.CountAsync();
                var totalRecipeIngredients = await _context.RecipeIngredients.CountAsync();

                Console.WriteLine($"Tổng số công thức (Recipes): {totalRecipes}");
                Console.WriteLine($"Tổng số người dùng (Users): {totalUsers}");
                Console.WriteLine($"Tổng số nguyên liệu (Ingredients): {totalIngredients}");
                Console.WriteLine($"Tổng số bữa ăn (MealTimes): {totalMealTimes}");
                Console.WriteLine($"Tổng số công thức yêu thích (Favorites): {totalFavorites}");
                Console.WriteLine($"Tổng số công thức-nguyên liệu (RecipeIngredients): {totalRecipeIngredients}");

                // Liệt kê các bữa ăn
                var mealTimes = await _context.MealTimes.ToListAsync();
                Console.WriteLine("\nDanh sách bữa ăn:");
                foreach (var meal in mealTimes)
                {
                    Console.WriteLine($"  ID: {meal.MealTimeId}, Tên: {meal.Name}");
                }

                // Liệt kê một số công thức với đánh giá
                var recipesWithRatings = await _context.Recipe
                    .Take(5)
                    .Select(r => new { r.RecipeId, r.Title, r.AverageRating, r.ReviewCount, r.Servings })
                    .ToListAsync();

                Console.WriteLine("\nMột số công thức (5 cái đầu):");
                foreach (var recipe in recipesWithRatings)
                {
                    Console.WriteLine($"  ID: {recipe.RecipeId}, Tên: {recipe.Title}, Đánh giá: {recipe.AverageRating}/5, Reviews: {recipe.ReviewCount}, Servings: {recipe.Servings}");
                }

                Console.WriteLine("\n✓ Test 1 hoàn tất\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Test 2: Lập kế hoạch cơ bản (7 ngày, 3 bữa/ngày, 2 người)
        /// </summary>
        public async Task Test2_BasicMealPlan()
        {
            Console.WriteLine("=== TEST 2: Lập kế hoạch cơ bản ===\n");

            try
            {
                // Kiểm tra có user nào không
                var firstUser = await _context.Users.FirstOrDefaultAsync();
                if (firstUser == null)
                {
                    Console.WriteLine("✗ Không có người dùng nào trong database\n");
                    return;
                }

                // Kiểm tra có recipe nào không
                var totalRecipes = await _context.Recipe.CountAsync();
                if (totalRecipes < 21)  // 7 ngày × 3 bữa = 21 công thức
                {
                    Console.WriteLine($"✗ Không đủ công thức: chỉ có {totalRecipes}, cần ít nhất 21\n");
                    return;
                }

                var input = new MealPlanningService.MealPlanningInput
                {
                    UserId = firstUser.UserId,
                    NumDays = 7,
                    NumPeople = 2,
                    MealTimeIds = new List<int> { 1, 2, 3 },  // Bữa Sáng, Bữa Trưa, Bữa Tối
                    Restrictions = new List<string>()
                };

                var result = await _mealPlanningService.GenerateMealPlanAsync(input);

                Console.WriteLine($"Thành công: {result.Success}");
                Console.WriteLine($"Thông báo: {result.Message}");
                Console.WriteLine($"Số công thức được lập: {result.MealPlan.Count}");
                Console.WriteLine($"Số nguyên liệu: {result.ShoppingList.Count}\n");

                if (result.MealPlan.Count > 0)
                {
                    Console.WriteLine("5 công thức đầu tiên:");
                    foreach (var meal in result.MealPlan.Take(5))
                    {
                        Console.WriteLine($"  Ngày {meal.Day} - {meal.MealName}: {meal.RecipeTitle}");
                        Console.WriteLine($"    Prep: {meal.PrepTime} phút, Cook: {meal.CookTime} phút");
                    }
                }

                if (result.ShoppingList.Count > 0)
                {
                    Console.WriteLine("\n5 nguyên liệu đầu tiên:");
                    foreach (var item in result.ShoppingList.Take(5))
                    {
                        Console.WriteLine($"  {item.IngredientName}: {item.TotalQuantity} {item.Unit}");
                    }
                }

                Console.WriteLine("\n✓ Test 2 hoàn tất\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Test 3: Lập kế hoạch với restrictions
        /// </summary>
        public async Task Test3_MealPlanWithRestrictions()
        {
            Console.WriteLine("=== TEST 3: Lập kế hoạch với restrictions ===\n");

            try
            {
                var firstUser = await _context.Users.FirstOrDefaultAsync();
                if (firstUser == null)
                {
                    Console.WriteLine("✗ Không có người dùng nào\n");
                    return;
                }

                var input = new MealPlanningService.MealPlanningInput
                {
                    UserId = firstUser.UserId,
                    NumDays = 3,
                    NumPeople = 4,
                    MealTimeIds = new List<int> { 1, 2 },  // Bữa Sáng, Bữa Trưa
                    Restrictions = new List<string> { "Chay" }
                };

                var result = await _mealPlanningService.GenerateMealPlanAsync(input);

                Console.WriteLine($"Thành công: {result.Success}");
                Console.WriteLine($"Thông báo: {result.Message}");
                Console.WriteLine($"Số công thức được lập: {result.MealPlan.Count}");
                Console.WriteLine($"Restrictions: Chay\n");

                if (result.MealPlan.Count > 0)
                {
                    Console.WriteLine("Kế hoạch bữa ăn:");
                    foreach (var meal in result.MealPlan)
                    {
                        Console.WriteLine($"  Ngày {meal.Day} - {meal.MealName}: {meal.RecipeTitle}");
                    }
                }

                Console.WriteLine("\n✓ Test 3 hoàn tất\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Test 4: Kiểm tra tính toán danh sách mua sắm (tỉ lệ nguyên liệu)
        /// </summary>
        public async Task Test4_ShoppingListCalculation()
        {
            Console.WriteLine("=== TEST 4: Kiểm tra tính toán danh sách mua sắm ===\n");

            try
            {
                var firstUser = await _context.Users.FirstOrDefaultAsync();
                if (firstUser == null)
                {
                    Console.WriteLine("✗ Không có người dùng nào\n");
                    return;
                }

                // Lập kế hoạch cho 2 người
                var input2People = new MealPlanningService.MealPlanningInput
                {
                    UserId = firstUser.UserId,
                    NumDays = 1,
                    NumPeople = 2,
                    MealTimeIds = new List<int> { 1 },
                    Restrictions = new List<string>()
                };

                var result2People = await _mealPlanningService.GenerateMealPlanAsync(input2People);

                Console.WriteLine("Kế hoạch cho 2 người:");
                if (result2People.ShoppingList.Count > 0)
                {
                    foreach (var item in result2People.ShoppingList.Take(3))
                    {
                        Console.WriteLine($"  {item.IngredientName}: {item.TotalQuantity} {item.Unit}");
                    }
                }

                // Lập kế hoạch cho 4 người
                var input4People = new MealPlanningService.MealPlanningInput
                {
                    UserId = firstUser.UserId,
                    NumDays = 1,
                    NumPeople = 4,
                    MealTimeIds = new List<int> { 1 },
                    Restrictions = new List<string>()
                };

                var result4People = await _mealPlanningService.GenerateMealPlanAsync(input4People);

                Console.WriteLine("\nKế hoạch cho 4 người:");
                if (result4People.ShoppingList.Count > 0)
                {
                    foreach (var item in result4People.ShoppingList.Take(3))
                    {
                        Console.WriteLine($"  {item.IngredientName}: {item.TotalQuantity} {item.Unit}");
                    }
                }

                Console.WriteLine("\n(Lưu ý: Số lượng cho 4 người phải gấp đôi so với 2 người)");
                Console.WriteLine("\n✓ Test 4 hoàn tất\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Test 5: Output JSON
        /// </summary>
        public async Task Test5_JsonOutput()
        {
            Console.WriteLine("=== TEST 5: Output JSON ===\n");

            try
            {
                var firstUser = await _context.Users.FirstOrDefaultAsync();
                if (firstUser == null)
                {
                    Console.WriteLine("✗ Không có người dùng nào\n");
                    return;
                }

                var input = new MealPlanningService.MealPlanningInput
                {
                    UserId = firstUser.UserId,
                    NumDays = 2,
                    NumPeople = 2,
                    MealTimeIds = new List<int> { 1, 2 },
                    Restrictions = new List<string>()
                };

                var result = await _mealPlanningService.GenerateMealPlanAsync(input);

                // Chuyển đổi sang JSON
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var mealPlanJson = JsonSerializer.Serialize(result.MealPlan, jsonOptions);
                var shoppingListJson = JsonSerializer.Serialize(result.ShoppingList, jsonOptions);

                Console.WriteLine("MealPlan JSON:");
                Console.WriteLine(mealPlanJson);

                Console.WriteLine("\nShoppingList JSON:");
                Console.WriteLine(shoppingListJson);

                Console.WriteLine("\n✓ Test 5 hoàn tất\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Lỗi: {ex.Message}\n");
            }
        }

        /// <summary>
        /// Chạy tất cả test
        /// </summary>
        public async Task RunAllTests()
        {
            Console.WriteLine("\n═══════════════════════════════════════════════════════");
            Console.WriteLine("  MEAL PLANNING ALGORITHM TEST SUITE");
            Console.WriteLine("═══════════════════════════════════════════════════════\n");

            await Test1_CheckDatabaseData();
            await Test2_BasicMealPlan();
            await Test3_MealPlanWithRestrictions();
            await Test4_ShoppingListCalculation();
            await Test5_JsonOutput();

            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("  TẤT CẢ TEST HOÀN TẤT");
            Console.WriteLine("═══════════════════════════════════════════════════════\n");
        }
    }
}
