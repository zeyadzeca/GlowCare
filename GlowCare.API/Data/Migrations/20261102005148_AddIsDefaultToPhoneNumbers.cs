using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlowCare.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsDefaultToPhoneNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "UserPhoneNumbers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "UserPhoneNumbers");
        }
    }
}
