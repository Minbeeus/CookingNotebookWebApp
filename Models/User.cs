using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookingNotebookWebApp.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string? Role { get; set; }
        public bool Status { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<Recipe> Recipes { get; set; }
        public ICollection<Favorite> Favorites { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}