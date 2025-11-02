using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models
{
    public class RecipeIngredient
    {
        public int RecipeId { get; set; }
        public Recipe? Recipe { get; set; }

        public int IngredientId { get; set; }
        public Ingredient? Ingredient { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? IngredientName { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        public string? Unit { get; set; }

        public string? Notes { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public bool IsDeleted { get; set; }
    }
}