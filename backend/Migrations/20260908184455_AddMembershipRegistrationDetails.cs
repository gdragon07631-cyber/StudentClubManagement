using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentClubManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipRegistrationDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BirthYear",
                table: "membership_applications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Expectations",
                table: "membership_applications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Interests",
                table: "membership_applications",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JoinReason",
                table: "membership_applications",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BirthYear",
                table: "membership_applications");

            migrationBuilder.DropColumn(
                name: "Expectations",
                table: "membership_applications");

            migrationBuilder.DropColumn(
                name: "Interests",
                table: "membership_applications");

            migrationBuilder.DropColumn(
                name: "JoinReason",
                table: "membership_applications");
        }
    }
}
