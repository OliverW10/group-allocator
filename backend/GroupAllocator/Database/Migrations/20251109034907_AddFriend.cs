using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GroupAllocator.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddFriend : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StudentFriends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    StudentId = table.Column<int>(type: "integer", nullable: false),
                    FriendId = table.Column<int>(type: "integer", nullable: false),
                    StudentFriendModelId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentFriends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentFriends_StudentFriends_StudentFriendModelId",
                        column: x => x.StudentFriendModelId,
                        principalTable: "StudentFriends",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentFriends_Students_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentFriends_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentFriends_FriendId",
                table: "StudentFriends",
                column: "FriendId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFriends_StudentFriendModelId",
                table: "StudentFriends",
                column: "StudentFriendModelId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentFriends_StudentId",
                table: "StudentFriends",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentFriends");
        }
    }
}
