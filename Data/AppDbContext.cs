using Microsoft.EntityFrameworkCore;
using CookingNotebookWebApp.Models;

namespace CookingNotebookWebApp.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Recipe> Recipe { get; set; }
        public DbSet<Ingredient> Ingredient { get; set; }
        public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
        public DbSet<RecipeStep> RecipeSteps { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        base.OnModelCreating(modelBuilder);

        // Unique index on RecipeId and StepNumber for RecipeStep
        modelBuilder.Entity<RecipeStep>()
            .HasIndex(rs => new { rs.RecipeId, rs.StepNumber })
            .IsUnique();

        // Ánh xạ bảng "User" cho entity User
        modelBuilder.Entity<User>().ToTable("User");

        // Khóa chính composite cho bảng RecipeIngredient
        modelBuilder.Entity<RecipeIngredient>()
        .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

        modelBuilder.Entity<RecipeIngredient>()
        .HasOne(ri => ri.Recipe)
        .WithMany(r => r.RecipeIngredients)
        .HasForeignKey(ri => ri.RecipeId)
                .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<RecipeIngredient>()
        .HasOne(ri => ri.Ingredient)
        .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId)
                .OnDelete(DeleteBehavior.NoAction);

            // Sửa foreign key constraints để tránh cascade paths
            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.User)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Recipe)
                .WithMany(r => r.Favorites)
                .HasForeignKey(f => f.RecipeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Recipe)
                .WithMany(r => r.Reviews)
                .HasForeignKey(r => r.RecipeId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RecipeStep>()
                .HasOne(rs => rs.Recipe)
                .WithMany(r => r.Steps)
                .HasForeignKey(rs => rs.RecipeId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}