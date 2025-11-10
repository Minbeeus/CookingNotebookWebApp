using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CookingNotebookWebApp.Services
{
    /// <summary>
    /// Service cho thuật toán lập kế hoạch bữa ăn (Meal Planning Algorithm)
    /// </summary>
    public class MealPlanningService
    {
        private readonly AppDbContext _context;
        private static readonly ThreadLocal<Random> _random = new(() => new Random());
        private Dictionary<int, MealTime>? _mealTimeCache;

        public MealPlanningService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Input class cho meal planning
        /// </summary>
        public class MealPlanningInput
        {
            public int UserId { get; set; }
            public int NumDays { get; set; }
            public int NumPeople { get; set; }
            public List<int> MealTimeIds { get; set; } = new();
            public List<string> Restrictions { get; set; } = new();
        }

        /// <summary>
        /// Output class cho meal plan item
        /// </summary>
        public class MealPlanItem
        {
            public int Day { get; set; }
            public string MealName { get; set; } = string.Empty;
            public int RecipeId { get; set; }
            public string RecipeTitle { get; set; } = string.Empty;
            public int PrepTime { get; set; }
            public int CookTime { get; set; }
            public string? ImageUrl { get; set; }
        }

        /// <summary>
        /// Output class cho shopping list item
        /// </summary>
        public class ShoppingListItem
        {
            public int IngredientId { get; set; }
            public string IngredientName { get; set; } = string.Empty;
            public decimal TotalQuantity { get; set; }
            public string? Unit { get; set; }
        }

        /// <summary>
        /// Output class cho kết quả cuối cùng
        /// </summary>
        public class MealPlanResult
        {
            public List<MealPlanItem> MealPlan { get; set; } = new();
            public List<ShoppingListItem> ShoppingList { get; set; } = new();
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
        }

        /// <summary>
        /// Hàm chính: Lập kế hoạch bữa ăn
        /// </summary>
        public async Task<MealPlanResult> GenerateMealPlanAsync(MealPlanningInput input)
        {
            var result = new MealPlanResult();

            try
            {
                // Bước 0: Cache dữ liệu MealTimes
                _mealTimeCache ??= await _context.MealTimes
                    .AsNoTracking()
                    .ToDictionaryAsync(m => m.MealTimeId);

                // Bước 1: Khởi tạo & Lấy dữ liệu người dùng
                var (favoriteRecipeIds, usedRecipeIds) = await Step1_InitializeAndGetUserData(input);
                var mealPlanItems = new List<MealPlanItem>();
                var usedRecipes = usedRecipeIds.ToHashSet();

                // Bước 2: Vòng lặp chính (Lập kế hoạch)
                for (int day = 1; day <= input.NumDays; day++)
                {
                    foreach (var mealTimeId in input.MealTimeIds)
                    {
                        // Bước 3: Xây dựng bể ứng viên (Candidate Pool)
                        var candidatePool = await Step3_BuildCandidatePool(input, mealTimeId, usedRecipes);

                        if (candidatePool.Count == 0)
                        {
                            result.Message += $"Không tìm thấy công thức phù hợp cho ngày {day}, bữa ăn ID {mealTimeId}. ";
                            continue;
                        }

                        // Bước 4: Chấm điểm ứng viên
                        Step4_ScoreCandidates(candidatePool, favoriteRecipeIds);

                        // Bước 5: Chọn công thức tốt nhất
                        var chosenRecipe = candidatePool.MaxBy(r => r.Score)!;
                        _mealTimeCache.TryGetValue(mealTimeId, out var mealTime);

                        mealPlanItems.Add(new MealPlanItem
                        {
                            Day = day,
                            MealName = mealTime?.Name ?? $"Bữa ăn {mealTimeId}",
                            RecipeId = chosenRecipe.RecipeId,
                            RecipeTitle = chosenRecipe.Title,
                            PrepTime = chosenRecipe.PrepTime,
                            CookTime = chosenRecipe.CookTime,
                            ImageUrl = chosenRecipe.ImageUrl
                        });

                        usedRecipes.Add(chosenRecipe.RecipeId);
                    }
                }

                // Bước 6: Tạo danh sách mua sắm
                var shoppingList = await Step6_GenerateShoppingList(mealPlanItems, input.NumPeople);

                result.MealPlan = mealPlanItems;
                result.ShoppingList = shoppingList;
                result.Success = true;
                result.Message = "Lập kế hoạch bữa ăn thành công.";

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Lỗi khi lập kế hoạch bữa ăn: {ex.Message}";
                return result;
            }
        }

        /// <summary>
        /// Bước 1: Khởi tạo & Lấy dữ liệu người dùng
        /// </summary>
        private async Task<(List<int>, List<int>)> Step1_InitializeAndGetUserData(MealPlanningInput input)
        {
            // Lấy danh sách công thức yêu thích
            var favoriteRecipeIds = await _context.Favorites
                .Where(f => f.UserId == input.UserId)
                .Select(f => f.RecipeId)
                .ToListAsync();

            // Khởi tạo UsedRecipeIds
            var usedRecipeIds = new List<int>();

            return (favoriteRecipeIds, usedRecipeIds);
        }

        /// <summary>
        /// Bước 3: Xây dựng bể ứng viên (Candidate Pool)
        /// </summary>
        private async Task<List<RecipeCandidate>> Step3_BuildCandidatePool(
            MealPlanningInput input,
            int mealTimeId,
            HashSet<int> usedRecipeIds)
        {
            // Lấy tất cả công thức theo bữa ăn + không tracking
            var recipes = await _context.Recipe
                .Where(r => r.Recipe_MealTimes!.Any(rmt => rmt.MealTimeId == mealTimeId))
                .Where(r => !usedRecipeIds.Contains(r.RecipeId))
                .AsNoTracking()
                .Select(r => new
                {
                    r.RecipeId,
                    r.Title,
                    r.AverageRating,
                    r.ReviewCount,
                    r.PrepTime,
                    r.CookTime,
                    r.ImageUrl,
                    r.Servings,
                    r.Type_of_dish,
                    r.Cooking_method
                })
                .ToListAsync();

            // Lọc theo restrictions
            if (input.Restrictions.Any())
            {
                recipes = recipes.Where(r =>
                    !input.Restrictions.Any(restriction =>
                        (r.Type_of_dish != null && r.Type_of_dish.Contains(restriction)) ||
                        (r.Cooking_method != null && r.Cooking_method.Contains(restriction))
                    )
                ).ToList();
            }

            // Chuyển đổi thành RecipeCandidate
            var candidates = recipes.Select(r => new RecipeCandidate
            {
                RecipeId = r.RecipeId,
                Title = r.Title,
                AverageRating = r.AverageRating,
                ReviewCount = r.ReviewCount,
                PrepTime = r.PrepTime,
                CookTime = r.CookTime,
                ImageUrl = r.ImageUrl,
                Servings = r.Servings,
                Score = 0
            }).ToList();

            return candidates;
        }

        /// <summary>
        /// Bước 4: Chấm điểm ứng viên
        /// </summary>
        private void Step4_ScoreCandidates(
            List<RecipeCandidate> candidatePool,
            List<int> favoriteRecipeIds)
        {
            var favoriteIds = favoriteRecipeIds.Count < 10
                ? new HashSet<int>(favoriteRecipeIds)
                : new HashSet<int>(favoriteRecipeIds);

            foreach (var recipe in candidatePool)
            {
                decimal score = 0;

                // Luật 1: Ưu tiên yêu thích (điểm cao nhất)
                if (favoriteIds.Contains(recipe.RecipeId))
                    score += 10;

                // Luật 2: Ưu tiên đánh giá cao
                if (recipe.AverageRating >= 4.5m)
                    score += 5;
                else if (recipe.AverageRating >= 4.0m)
                    score += 3;

                // Luật 3: Ưu tiên công thức đáng tin cậy (nhiều review)
                if (recipe.ReviewCount > 20)
                    score += 2;

                // Luật 4: Thêm yếu tố ngẫu nhiên (để kế hoạch đa dạng)
                score += _random.Value!.Next(0, 3);

                recipe.Score = score;
            }
        }

        /// <summary>
        /// Bước 6: Tạo danh sách mua sắm
        /// </summary>
        private async Task<List<ShoppingListItem>> Step6_GenerateShoppingList(
            List<MealPlanItem> mealPlanItems,
            int numPeople)
        {
            var shoppingListDict = new Dictionary<int, ShoppingListItem>();
            var recipeIds = mealPlanItems.Select(m => m.RecipeId).ToList();

            // Batch load tất cả recipes + ingredients cùng một lần
            var recipes = await _context.Recipe
                .Where(r => recipeIds.Contains(r.RecipeId))
                .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                .AsNoTracking()
                .ToDictionaryAsync(r => r.RecipeId);

            // Lặp qua từng công thức trong kế hoạch
            foreach (var mealItem in mealPlanItems)
            {
                if (!recipes.TryGetValue(mealItem.RecipeId, out var recipe) || recipe.RecipeIngredients == null)
                    continue;

                // Tính tỉ lệ điều chỉnh
                decimal ratio = (decimal)numPeople / recipe.Servings;

                // Lặp qua từng nguyên liệu
                foreach (var recipeIngredient in recipe.RecipeIngredients)
                {
                    var ingredientId = recipeIngredient.IngredientId;
                    var ingredientName = recipeIngredient.Ingredient?.Name ?? "Không xác định";
                    var unit = recipeIngredient.Unit;
                    var adjustedQuantity = recipeIngredient.Quantity * ratio;

                    if (shoppingListDict.TryGetValue(ingredientId, out var existing))
                    {
                        // Cộng dồn số lượng
                        existing.TotalQuantity += adjustedQuantity;
                    }
                    else
                    {
                        // Thêm mới
                        shoppingListDict[ingredientId] = new ShoppingListItem
                        {
                            IngredientId = ingredientId,
                            IngredientName = ingredientName,
                            TotalQuantity = adjustedQuantity,
                            Unit = unit
                        };
                    }
                }
            }

            return shoppingListDict.Values.OrderBy(s => s.IngredientName).ToList();
        }

        /// <summary>
        /// Internal class để lưu trữ thông tin ứng viên (trong quá trình chấm điểm)
        /// </summary>
        private class RecipeCandidate
        {
            public int RecipeId { get; set; }
            public string Title { get; set; } = string.Empty;
            public decimal AverageRating { get; set; }
            public int ReviewCount { get; set; }
            public int PrepTime { get; set; }
            public int CookTime { get; set; }
            public string? ImageUrl { get; set; }
            public int Servings { get; set; }
            public decimal Score { get; set; }
        }
    }
}
