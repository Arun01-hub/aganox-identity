using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Aganox.IdentityServer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationRegistry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegisteredApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClientId = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ApplicationType = table.Column<int>(type: "INTEGER", nullable: false),
                    Environment = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    OpenIddictApplicationId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    ConsentType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegisteredApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    ActorUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Summary = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    DetailsJson = table.Column<string>(type: "TEXT", maxLength: 8000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationAuditEntries_RegisteredApplications_RegisteredApplicationId",
                        column: x => x.RegisteredApplicationId,
                        principalTable: "RegisteredApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClientCredentials",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegisteredApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SecretHash = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    SecretPrefix = table.Column<string>(type: "TEXT", maxLength: 8, nullable: true),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    RevokedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    RevokedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientCredentials_RegisteredApplications_RegisteredApplicationId",
                        column: x => x.RegisteredApplicationId,
                        principalTable: "RegisteredApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredAllowedOrigins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegisteredApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Origin = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredAllowedOrigins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisteredAllowedOrigins_RegisteredApplications_RegisteredApplicationId",
                        column: x => x.RegisteredApplicationId,
                        principalTable: "RegisteredApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredApplicationScopes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegisteredApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Scope = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredApplicationScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisteredApplicationScopes_RegisteredApplications_RegisteredApplicationId",
                        column: x => x.RegisteredApplicationId,
                        principalTable: "RegisteredApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegisteredRedirectUris",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegisteredApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Uri = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Kind = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegisteredRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegisteredRedirectUris_RegisteredApplications_RegisteredApplicationId",
                        column: x => x.RegisteredApplicationId,
                        principalTable: "RegisteredApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAuditEntries_RegisteredApplicationId_OccurredAt",
                table: "ApplicationAuditEntries",
                columns: new[] { "RegisteredApplicationId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClientCredentials_RegisteredApplicationId_IsPrimary",
                table: "ClientCredentials",
                columns: new[] { "RegisteredApplicationId", "IsPrimary" });

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredAllowedOrigins_RegisteredApplicationId_Origin",
                table: "RegisteredAllowedOrigins",
                columns: new[] { "RegisteredApplicationId", "Origin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredApplications_ClientId",
                table: "RegisteredApplications",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredApplications_Status_Environment",
                table: "RegisteredApplications",
                columns: new[] { "Status", "Environment" });

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredApplicationScopes_RegisteredApplicationId_Scope",
                table: "RegisteredApplicationScopes",
                columns: new[] { "RegisteredApplicationId", "Scope" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegisteredRedirectUris_RegisteredApplicationId_Kind_Uri",
                table: "RegisteredRedirectUris",
                columns: new[] { "RegisteredApplicationId", "Kind", "Uri" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAuditEntries");

            migrationBuilder.DropTable(
                name: "ClientCredentials");

            migrationBuilder.DropTable(
                name: "RegisteredAllowedOrigins");

            migrationBuilder.DropTable(
                name: "RegisteredApplicationScopes");

            migrationBuilder.DropTable(
                name: "RegisteredRedirectUris");

            migrationBuilder.DropTable(
                name: "RegisteredApplications");
        }
    }
}
