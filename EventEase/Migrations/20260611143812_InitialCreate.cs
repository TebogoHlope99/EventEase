using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventEase.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ColorClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeId);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    VenueId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VenueName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.VenueId);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EventDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VenueId = table.Column<int>(type: "int", nullable: true),
                    EventTypeId = table.Column<int>(type: "int", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_Events_EventTypes_EventTypeId",
                        column: x => x.EventTypeId,
                        principalTable: "EventTypes",
                        principalColumn: "EventTypeId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Events_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<int>(type: "int", nullable: false),
                    VenueId = table.Column<int>(type: "int", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingId);
                    table.ForeignKey(
                        name: "FK_Bookings_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Bookings_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "EventTypes",
                columns: new[] { "EventTypeId", "ColorClass", "CreatedAt", "Description", "IconClass", "IsActive", "TypeName" },
                values: new object[,]
                {
                    { 1, "primary", new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4355), "Professional conferences and seminars", "bi-people-fill", true, "Conference" },
                    { 2, "danger", new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4357), "Wedding ceremonies and receptions", "bi-heart-fill", true, "Wedding" },
                    { 3, "success", new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4359), "Live music and performances", "bi-music-note-beamed", true, "Concert" },
                    { 4, "info", new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4361), "Business meetings and corporate events", "bi-briefcase-fill", true, "Corporate" },
                    { 5, "warning", new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4362), "Birthdays, anniversaries, private celebrations", "bi-gift-fill", true, "Private Party" },
                    { 6, "secondary", new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4364), "Educational workshops and training", "bi-mortarboard-fill", true, "Workshop" }
                });

            migrationBuilder.InsertData(
                table: "Venues",
                columns: new[] { "VenueId", "Capacity", "CreatedAt", "ImageUrl", "IsAvailable", "Location", "VenueName" },
                values: new object[,]
                {
                    { 1, 500, new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4469), "https://placehold.co/600x400/3b82f6/white?text=Grand+Ballroom", true, "123 Main St, City Center", "Grand Ballroom" },
                    { 2, 200, new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4471), "https://placehold.co/600x400/10b981/white?text=Conference+Hall+A", true, "45 Business Park", "Conference Hall A" },
                    { 3, 150, new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4472), "https://placehold.co/600x400/f59e0b/white?text=Garden+Pavilion", true, "789 Park Avenue", "Garden Pavilion" },
                    { 4, 30, new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4474), "https://placehold.co/600x400/ef4444/white?text=Executive+Boardroom", true, "45 Business Park", "Executive Boardroom" }
                });

            migrationBuilder.InsertData(
                table: "Events",
                columns: new[] { "EventId", "CreatedAt", "Description", "EventDate", "EventName", "EventTypeId", "ImageUrl", "VenueId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4500), "Technology innovation showcase with keynote speakers and workshops", new DateTime(2026, 6, 15, 9, 0, 0, 0, DateTimeKind.Unspecified), "Annual Tech Conference", 1, "https://placehold.co/600x400/8b5cf6/white?text=Tech+Conference", 1 },
                    { 2, new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4503), "Smith-Johnson wedding reception with dinner and dancing", new DateTime(2026, 7, 20, 18, 0, 0, 0, DateTimeKind.Unspecified), "Wedding Reception", 2, "https://placehold.co/600x400/ec4899/white?text=Wedding+Reception", 3 },
                    { 3, new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4505), "New product reveal for the latest tech innovation", new DateTime(2026, 5, 10, 14, 0, 0, 0, DateTimeKind.Unspecified), "Product Launch", 1, "https://placehold.co/600x400/14b8a6/white?text=Product+Launch", 2 },
                    { 4, new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4507), "Quarterly review and strategic planning session", new DateTime(2026, 5, 5, 10, 0, 0, 0, DateTimeKind.Unspecified), "Board Meeting", 4, "https://placehold.co/600x400/f97316/white?text=Board+Meeting", 4 }
                });

            migrationBuilder.InsertData(
                table: "Bookings",
                columns: new[] { "BookingId", "BookingDate", "CreatedAt", "EventId", "VenueId" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 6, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4530), 1, 1 },
                    { 2, new DateTime(2026, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4532), 2, 3 },
                    { 3, new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4534), 3, 2 },
                    { 4, new DateTime(2026, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 6, 11, 16, 38, 11, 891, DateTimeKind.Local).AddTicks(4535), 4, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_EventId",
                table: "Bookings",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "UQ_Booking_Venue_Date",
                table: "Bookings",
                columns: new[] { "VenueId", "BookingDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_EventTypeId",
                table: "Events",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_VenueId",
                table: "Events",
                column: "VenueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropTable(
                name: "Venues");
        }
    }
}
