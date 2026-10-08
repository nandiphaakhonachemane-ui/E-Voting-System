using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Voting_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVotingToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IssuedAt",
                table: "VotingTokens",
                newName: "CreatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "VotingTokens",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "VotingTokens",
                newName: "IssuedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "VotingTokens",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
