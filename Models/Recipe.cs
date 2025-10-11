using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models
{
    public class Recipe
    {
        public int RecipeId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        
        public string Description { get; set; }
        
        [Required]
        public int PrepTime { get; set; } // in minutes
        
        [Required]
        public int CookTime { get; set; } // in minutes
        
        [Required]
        public int Servings { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public ICollection<RecipeIngredient> RecipeIngredients { get; set; }
        public ICollection<RecipeStep> Steps { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
    }
}