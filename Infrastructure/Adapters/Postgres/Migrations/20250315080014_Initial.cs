using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Adapters.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "expiry_status",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expiry_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbox", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox", x => x.event_id);
                });

            migrationBuilder.CreateTable(
                name: "vehicle_documents",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_pts_added = table.Column<bool>(type: "boolean", nullable: false),
                    is_sts_added = table.Column<bool>(type: "boolean", nullable: false),
                    is_osago_added = table.Column<bool>(type: "boolean", nullable: false),
                    pts_front_photo_storage_bucket_and_key = table.Column<string>(type: "text", nullable: true),
                    pts_back_photo_storage_bucket_and_key = table.Column<string>(type: "text", nullable: true),
                    pts_year_of_manufacture = table.Column<DateOnly>(type: "date", nullable: true),
                    pts_color = table.Column<string>(type: "text", nullable: true),
                    pts_vin = table.Column<string>(type: "text", nullable: true),
                    sts_front_photo_storage_bucket_and_key = table.Column<string>(type: "text", nullable: true),
                    sts_back_photo_storage_bucket_and_key = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "osago",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    vehicle_documents_id = table.Column<Guid>(type: "uuid", nullable: false),
                    photo_storage_bucket_and_key = table.Column<string>(type: "text", nullable: false),
                    expiry_status_id = table.Column<int>(type: "integer", nullable: false),
                    date_of_issue = table.Column<DateOnly>(type: "date", nullable: false),
                    date_of_expiry = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_osago", x => x.id);
                    table.ForeignKey(
                        name: "FK_expiry_status_id",
                        column: x => x.expiry_status_id,
                        principalTable: "expiry_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_vehicle_documents",
                        column: x => x.vehicle_documents_id,
                        principalTable: "vehicle_documents",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "expiry_status",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { 1, "notexpired" },
                    { 2, "expired" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbox_messages_unprocessed",
                table: "inbox",
                columns: new[] { "occurred_on_utc", "processed_on_utc" },
                filter: "processed_on_utc IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "event_id", "type" });

            migrationBuilder.CreateIndex(
                name: "IX_osago_expiry_status_id",
                table: "osago",
                column: "expiry_status_id");

            migrationBuilder.CreateIndex(
                name: "IX_osago_vehicle_documents_id",
                table: "osago",
                column: "vehicle_documents_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_unprocessed",
                table: "outbox",
                columns: new[] { "occurred_on_utc", "processed_on_utc" },
                filter: "processed_on_utc IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "event_id", "type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbox");

            migrationBuilder.DropTable(
                name: "osago");

            migrationBuilder.DropTable(
                name: "outbox");

            migrationBuilder.DropTable(
                name: "expiry_status");

            migrationBuilder.DropTable(
                name: "vehicle_documents");
        }
    }
}
