using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models
{
    public class Favorite
    {
        public int FavoriteId { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; } = default!;

        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; } = default!;
        
        public DateTime CreatedAt { get; set; }
    }
}