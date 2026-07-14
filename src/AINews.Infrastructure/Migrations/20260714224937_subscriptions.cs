using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AINews.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class subscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Guid>>(
                name: "ArticleIds",
                table: "NewsletterIssues",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "NewsletterSubscribers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SubscribedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UnsubscribeToken = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsletterSubscribers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_Email",
                table: "NewsletterSubscribers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NewsletterSubscribers_UnsubscribeToken",
                table: "NewsletterSubscribers",
                column: "UnsubscribeToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NewsletterSubscribers");

            migrationBuilder.DropColumn(
                name: "ArticleIds",
                table: "NewsletterIssues");
        }
    }
}
