using CookingNotebookWebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models.ViewModels
{
    public class EditRecipeViewModel
    {
        public Recipe Recipe { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();
        public List<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
    }
}
