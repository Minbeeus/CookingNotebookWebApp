using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
        
        [Required]
        public decimal Quantity { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Unit { get; set; }
        
        public string? Notes { get; set; }
    }
}