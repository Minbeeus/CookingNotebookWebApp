using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookingNotebookWebApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string? Role { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}