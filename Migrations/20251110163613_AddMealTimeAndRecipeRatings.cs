using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookingNotebookWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMealTimeAndRecipeRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                table: "Recipe",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "ReviewCount",
                table: "Recipe",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "MealTimes",
                columns: table => new
                {
                    MealTimeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealTimes", x => x.MealTimeId);
                });

            migrationBuilder.CreateTable(
                name: "Recipe_MealTimes",
                columns: table => new
                {
                    RecipeId = table.Column<int>(type: "int", nullable: false),
                    MealTimeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recipe_MealTimes", x => new { x.RecipeId, x.MealTimeId });
                    table.ForeignKey(
                        name: "FK_Recipe_MealTimes_MealTimes_MealTimeId",
                        column: x => x.MealTimeId,
                        principalTable: "MealTimes",
                        principalColumn: "MealTimeId");
                    table.ForeignKey(
                        name: "FK_Recipe_MealTimes_Recipe_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "Recipe",
                        principalColumn: "RecipeId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Recipe_MealTimes_MealTimeId",
                table: "Recipe_MealTimes",
                column: "MealTimeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Recipe_MealTimes");

            migrationBuilder.DropTable(
                name: "MealTimes");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "ReviewCount",
                table: "Recipe");
        }
    }
}
