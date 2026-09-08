using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NapsterAI.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialRealEstateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Locations",
                schema: "dbo",
                columns: table => new
                {
                    LocationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.LocationID);
                });

            migrationBuilder.CreateTable(
                name: "Lookups",
                schema: "dbo",
                columns: table => new
                {
                    LookupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LookupType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lookups", x => x.LookupID);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                schema: "dbo",
                columns: table => new
                {
                    MediaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    MediaType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.MediaID);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                schema: "dbo",
                columns: table => new
                {
                    ProjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Developer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProjectType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    District = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    TotalBuildings = table.Column<int>(type: "int", nullable: true),
                    TotalFloors = table.Column<int>(type: "int", nullable: true),
                    TotalUnits = table.Column<int>(type: "int", nullable: true),
                    LaunchDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ConstructionStart = table.Column<DateOnly>(type: "date", nullable: true),
                    EstimatedCompletion = table.Column<DateOnly>(type: "date", nullable: true),
                    HandoverDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StartingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MaxPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PaymentPlan = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PermitNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ServiceCharge = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MasterPlanURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BrochureURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VideoURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectID);
                });

            migrationBuilder.CreateTable(
                name: "Amenities",
                schema: "dbo",
                columns: table => new
                {
                    AmenityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    AmenityName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IconURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImageURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsHighlighted = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.AmenityID);
                    table.ForeignKey(
                        name: "FK_Amenities_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnitTypes",
                schema: "dbo",
                columns: table => new
                {
                    UnitTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    TypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Bedrooms = table.Column<int>(type: "int", nullable: true),
                    Bathrooms = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    MinAreaSqFt = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MaxAreaSqFt = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    BasePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PricePerSqFt = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    TotalUnitsOfType = table.Column<int>(type: "int", nullable: true),
                    AvailableUnitsOfType = table.Column<int>(type: "int", nullable: true),
                    FloorPlanURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitTypes", x => x.UnitTypeID);
                    table.ForeignKey(
                        name: "FK_UnitTypes_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                schema: "dbo",
                columns: table => new
                {
                    InventoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    UnitTypeID = table.Column<int>(type: "int", nullable: false),
                    UnitNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuildingTower = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FloorNumber = table.Column<int>(type: "int", nullable: true),
                    ViewType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AreaSqFt = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Bedrooms = table.Column<int>(type: "int", nullable: true),
                    Bathrooms = table.Column<decimal>(type: "decimal(3,1)", nullable: true),
                    ParkingSpaces = table.Column<int>(type: "int", nullable: true),
                    HasBalcony = table.Column<bool>(type: "bit", nullable: true),
                    FurnishingStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ListPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PricePerSqFt = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ListingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    SoldOrLeasedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BuyerTenantName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AgentName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AgentContact = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.InventoryID);
                    table.ForeignKey(
                        name: "FK_Inventory_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inventory_UnitTypes_UnitTypeID",
                        column: x => x.UnitTypeID,
                        principalSchema: "dbo",
                        principalTable: "UnitTypes",
                        principalColumn: "UnitTypeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Category",
                schema: "dbo",
                table: "Amenities",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_ProjectID",
                schema: "dbo",
                table: "Amenities",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Bedrooms",
                schema: "dbo",
                table: "Inventory",
                column: "Bedrooms");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ListPrice",
                schema: "dbo",
                table: "Inventory",
                column: "ListPrice");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProjectID",
                schema: "dbo",
                table: "Inventory",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_ProjectID_UnitNumber",
                schema: "dbo",
                table: "Inventory",
                columns: new[] { "ProjectID", "UnitNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_Status",
                schema: "dbo",
                table: "Inventory",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_UnitTypeID",
                schema: "dbo",
                table: "Inventory",
                column: "UnitTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Locations_Country_City_District",
                schema: "dbo",
                table: "Locations",
                columns: new[] { "Country", "City", "District" },
                unique: true,
                filter: "[District] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Lookups_LookupType_Code",
                schema: "dbo",
                table: "Lookups",
                columns: new[] { "LookupType", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Media_EntityType_EntityId",
                schema: "dbo",
                table: "Media",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_City",
                schema: "dbo",
                table: "Projects",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsActive",
                schema: "dbo",
                table: "Projects",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsFeatured",
                schema: "dbo",
                table: "Projects",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectCode",
                schema: "dbo",
                table: "Projects",
                column: "ProjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectType",
                schema: "dbo",
                table: "Projects",
                column: "ProjectType");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Status",
                schema: "dbo",
                table: "Projects",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UnitTypes_ProjectID",
                schema: "dbo",
                table: "UnitTypes",
                column: "ProjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Amenities",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Inventory",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Locations",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Lookups",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Media",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "UnitTypes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Projects",
                schema: "dbo");
        }
    }
}
