using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotesPOC.Migrations
{
    /// <inheritdoc />
    public partial class RenameNoteColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "Notes",
                newName: "LastModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastModifiedAt",
                table: "Notes",
                newName: "LastModified");
        }
    }
}
