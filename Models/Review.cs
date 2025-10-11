using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }
        
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }
        
        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }
        
        public string? Comment { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}