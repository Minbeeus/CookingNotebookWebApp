using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CookingNotebookWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddGoogleFieldsToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecipeSteps_RecipeId",
                table: "RecipeSteps");

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

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GoogleId",
                table: "User",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RecipeIngredients",
                table: "RecipeIngredients",
                columns: new[] { "RecipeId", "IngredientId" });

            migrationBuilder.CreateIndex(
                name: "IX_RecipeSteps_RecipeId_StepNumber",
                table: "RecipeSteps",
                columns: new[] { "RecipeId", "StepNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RecipeSteps_RecipeId_StepNumber",
                table: "RecipeSteps");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RecipeIngredients",
                table: "RecipeIngredients");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "User");

            migrationBuilder.DropColumn(
                name: "GoogleId",
                table: "User");

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
                name: "IX_RecipeSteps_RecipeId",
                table: "RecipeSteps",
                column: "RecipeId");

            migrationBuilder.CreateIndex(
                name: "IX_RecipeIngredients_RecipeId",
                table: "RecipeIngredients",
                column: "RecipeId");
        }
    }
}
