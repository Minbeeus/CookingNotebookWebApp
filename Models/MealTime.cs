using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models
{
    public class MealTime
    {
        public int MealTimeId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = default!;

        // Navigation properties
        public ICollection<Recipe_MealTime>? Recipe_MealTimes { get; set; }
    }
}
