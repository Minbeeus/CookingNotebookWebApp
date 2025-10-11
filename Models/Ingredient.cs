using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CookingNotebookWebApp.Models
{
    public class Ingredient
    {
        [Key]
        public int IngredientId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Tên tiếng Việt")]
        public string Name { get; set; }

        [MaxLength(100)]
        [Display(Name = "Tên tiếng Anh")]
        public string? EnName { get; set; }

        [MaxLength(100)]
        [Display(Name = "Tên không dấu (viết thường)")]
        public string? ViNameWithoutAccents { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [MaxLength(50)]
        [Display(Name = "Phân loại")]
        public string? Type { get; set; }

        [MaxLength(255)]
        [Display(Name = "Đường dẫn ảnh")]
        public string? ImagePath { get; set; }

        // Navigation property
        public ICollection<RecipeIngredient>? RecipeIngredients { get; set; }
    }
}