using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardCategoryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Flashcards");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Flashcards",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FlashcardCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlashcardCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_CategoryId",
                table: "Flashcards",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FlashcardCategories_Name",
                table: "FlashcardCategories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Flashcards_FlashcardCategories_CategoryId",
                table: "Flashcards",
                column: "CategoryId",
                principalTable: "FlashcardCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flashcards_FlashcardCategories_CategoryId",
                table: "Flashcards");

            migrationBuilder.DropTable(
                name: "FlashcardCategories");

            migrationBuilder.DropIndex(
                name: "IX_Flashcards_CategoryId",
                table: "Flashcards");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Flashcards");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Flashcards",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
