namespace CookingNotebookWebApp.Models
{
    public class Recipe_MealTime
    {
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public int MealTimeId { get; set; }
        public MealTime? MealTime { get; set; }
    }
}
