using Microsoft.AspNetCore.Mvc;
using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CookingNotebookWebApp.Controllers
{
    /// <summary>
    /// Controller cho Meal Planning Service
    /// Endpoints:
    ///   POST /api/mealplanning/generate - Lập kế hoạch bữa ăn (API)
    ///   POST /api/mealplanning/test - Chạy test (API)
    ///   GET /MealPlanning - Trang giao diện (View)
    /// </summary>
    public class MealPlanningController : Controller
    {
        private readonly MealPlanningService _mealPlanningService;
        private readonly AppDbContext _context;

        public MealPlanningController(MealPlanningService mealPlanningService, AppDbContext context)
        {
            _mealPlanningService = mealPlanningService;
            _context = context;
        }

        /// <summary>
        /// Lập kế hoạch bữa ăn
        /// POST /api/mealplanning/generate
        /// </summary>
        [HttpPost]
        [Route("api/mealplanning/generate")]
        public async Task<IActionResult> GenerateMealPlan([FromBody] MealPlanningService.MealPlanningInput input)
        {
            if (input == null)
            {
                return BadRequest(new { success = false, message = "Input không hợp lệ" });
            }

            if (input.NumDays <= 0 || input.NumDays > 30)
            {
                return BadRequest(new { success = false, message = "NumDays phải từ 1 đến 30" });
            }

            if (input.NumPeople <= 0)
            {
                return BadRequest(new { success = false, message = "NumPeople phải lớn hơn 0" });
            }

            if (input.MealTimeIds == null || input.MealTimeIds.Count == 0)
            {
                return BadRequest(new { success = false, message = "MealTimeIds không được để trống" });
            }

            try
            {
                var result = await _mealPlanningService.GenerateMealPlanAsync(input);
                
                if (result.Success)
                {
                    return Ok(new 
                    { 
                        success = true, 
                        message = result.Message,
                        mealPlan = result.MealPlan,
                        shoppingList = result.ShoppingList
                    });
                }
                else
                {
                    return BadRequest(new { success = false, message = result.Message });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new 
                { 
                    success = false, 
                    message = "Lỗi server: " + ex.Message 
                });
            }
        }

        /// <summary>
        /// Endpoint test - chạy test suite
        /// POST /api/mealplanning/test
        /// </summary>
        [HttpPost]
        [Route("api/mealplanning/test")]
        public async Task<IActionResult> RunTest()
        {
            try
            {
                var testRunner = new MealPlanningAlgorithmTest(_context, _mealPlanningService);
                
                // Chạy tất cả test
                await testRunner.RunAllTests();
                
                return Ok(new { success = true, message = "Test suite chạy xong. Xem console output." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET /api/mealplanning/health - Kiểm tra trạng thái service
        /// </summary>
        [HttpGet]
        [Route("api/mealplanning/health")]
        public IActionResult Health()
        {
            return Ok(new { success = true, message = "Meal Planning Service is running" });
        }

        /// <summary>
        /// GET /api/mealplanning/mealtimes - Lấy danh sách bữa ăn
        /// </summary>
        [HttpGet]
        [Route("api/mealplanning/mealtimes")]
        public async Task<IActionResult> GetMealTimes()
        {
            try
            {
                var mealTimes = await _context.MealTimes.ToListAsync();
                return Ok(new { success = true, mealTimes });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET /MealPlanning/Index - Trang giao diện meal planning
        /// </summary>
        [HttpGet]
        [Route("MealPlanning")]
        [Route("mealplanning/index")]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy danh sách bữa ăn
                var mealTimes = await _context.MealTimes.ToListAsync();
                
                // Truyền sang View
                return View(mealTimes);
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }
    }
}
