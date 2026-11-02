using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GlowCare.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSinglePhoneNumberPerUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // A customer now has exactly ONE primary phone number. Before the
            // unique index can be created, legacy rows that would violate it
            // are removed: the default (or oldest) number of each user is kept.
            // The remaining rows all become the primary number of their owner.
            migrationBuilder.Sql(@"
                ;WITH dupes AS (
                    SELECT [Id],
                           ROW_NUMBER() OVER (
                               PARTITION BY [UserId]
                               ORDER BY CASE WHEN [IsDefault] = 1 THEN 0 ELSE 1 END, [Id]
                           ) AS rn
                    FROM [UserPhoneNumbers]
                )
                DELETE FROM dupes WHERE rn > 1;

                UPDATE [UserPhoneNumbers] SET [IsDefault] = 1;
            ");

            migrationBuilder.DropIndex(
                name: "IX_UserPhoneNumbers_UserId",
                table: "UserPhoneNumbers");

            migrationBuilder.CreateIndex(
                name: "IX_UserPhoneNumbers_UserId",
                table: "UserPhoneNumbers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserPhoneNumbers_UserId",
                table: "UserPhoneNumbers");

            migrationBuilder.CreateIndex(
                name: "IX_UserPhoneNumbers_UserId",
                table: "UserPhoneNumbers",
                column: "UserId");
        }
    }
}
