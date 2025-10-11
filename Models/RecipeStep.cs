using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models
{
    public class RecipeStep
    {
        public int RecipeStepId { get; set; }
        
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        
        [Required]
        public int StepNumber { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public int? TimerInMinutes { get; set; }
    }
}