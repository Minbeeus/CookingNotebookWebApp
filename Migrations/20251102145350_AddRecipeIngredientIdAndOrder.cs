using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookingNotebookWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddRecipeIngredientIdAndOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeIngredients",
                table: "RecipeIngredients");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "RecipeIngredients",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "RecipeIngredientId",
                table: "RecipeIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "RecipeIngredients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeIngredients",
                table: "RecipeIngredients",
                column: "RecipeIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeId",
                table: "RecipeIngredients",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeIngredients",
                table: "RecipeIngredients");

            migrationBuilder.DropIndex(
                name: "IX_RecipeIngredients_RecipeId",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "RecipeIngredientId",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "Cooking_method",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Recipe");

            migrationBuilder.DropColumn(
                name: "Type_of_dish",
                table: "Recipe");

            migrationBuilder.AlterColumn<string>(
                name: "Unit",
                table: "RecipeIngredients",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeIngredients",
                table: "RecipeIngredients",
                columns: new[] { "RecipeId", "IngredientId" });
        }
    }
}
