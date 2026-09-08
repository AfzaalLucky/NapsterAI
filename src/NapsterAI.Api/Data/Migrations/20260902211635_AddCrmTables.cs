using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NapsterAI.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCrmTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SalesAgentID",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesAgentID",
                schema: "dbo",
                table: "Inventory",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "dbo",
                columns: table => new
                {
                    CustomerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerID);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                schema: "dbo",
                columns: table => new
                {
                    OrganizationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Website = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.OrganizationID);
                });

            migrationBuilder.CreateTable(
                name: "PaymentPlanMilestones",
                schema: "dbo",
                columns: table => new
                {
                    MilestoneID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    MilestoneName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PercentDue = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TriggerEvent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DueDateOffsetDays = table.Column<int>(type: "int", nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentPlanMilestones", x => x.MilestoneID);
                    table.ForeignKey(
                        name: "FK_PaymentPlanMilestones_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesAgents",
                schema: "dbo",
                columns: table => new
                {
                    SalesAgentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrganizationID = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PhotoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LicenseNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesAgents", x => x.SalesAgentID);
                    table.ForeignKey(
                        name: "FK_SalesAgents_Organizations_OrganizationID",
                        column: x => x.OrganizationID,
                        principalSchema: "dbo",
                        principalTable: "Organizations",
                        principalColumn: "OrganizationID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                schema: "dbo",
                columns: table => new
                {
                    LeadID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    ProjectID = table.Column<int>(type: "int", nullable: true),
                    InventoryID = table.Column<int>(type: "int", nullable: true),
                    SalesAgentID = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Budget = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RequirementsNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    LastContactedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.LeadID);
                    table.ForeignKey(
                        name: "FK_Leads_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalSchema: "dbo",
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Inventory_InventoryID",
                        column: x => x.InventoryID,
                        principalSchema: "dbo",
                        principalTable: "Inventory",
                        principalColumn: "InventoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_SalesAgents_SalesAgentID",
                        column: x => x.SalesAgentID,
                        principalSchema: "dbo",
                        principalTable: "SalesAgents",
                        principalColumn: "SalesAgentID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Inquiries",
                schema: "dbo",
                columns: table => new
                {
                    InquiryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerID = table.Column<int>(type: "int", nullable: false),
                    ProjectID = table.Column<int>(type: "int", nullable: true),
                    InventoryID = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ConvertedToLeadID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquiries", x => x.InquiryID);
                    table.ForeignKey(
                        name: "FK_Inquiries_Customers_CustomerID",
                        column: x => x.CustomerID,
                        principalSchema: "dbo",
                        principalTable: "Customers",
                        principalColumn: "CustomerID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquiries_Inventory_InventoryID",
                        column: x => x.InventoryID,
                        principalSchema: "dbo",
                        principalTable: "Inventory",
                        principalColumn: "InventoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquiries_Leads_ConvertedToLeadID",
                        column: x => x.ConvertedToLeadID,
                        principalSchema: "dbo",
                        principalTable: "Leads",
                        principalColumn: "LeadID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inquiries_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalSchema: "dbo",
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadActivities",
                schema: "dbo",
                columns: table => new
                {
                    LeadActivityID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadID = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedByUserID = table.Column<int>(type: "int", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadActivities", x => x.LeadActivityID);
                    table.ForeignKey(
                        name: "FK_LeadActivities_Leads_LeadID",
                        column: x => x.LeadID,
                        principalSchema: "dbo",
                        principalTable: "Leads",
                        principalColumn: "LeadID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeadActivities_Users_CreatedByUserID",
                        column: x => x.CreatedByUserID,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Viewings",
                schema: "dbo",
                columns: table => new
                {
                    ViewingID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LeadID = table.Column<int>(type: "int", nullable: false),
                    InventoryID = table.Column<int>(type: "int", nullable: false),
                    ScheduledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    SalesAgentID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Viewings", x => x.ViewingID);
                    table.ForeignKey(
                        name: "FK_Viewings_Inventory_InventoryID",
                        column: x => x.InventoryID,
                        principalSchema: "dbo",
                        principalTable: "Inventory",
                        principalColumn: "InventoryID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Viewings_Leads_LeadID",
                        column: x => x.LeadID,
                        principalSchema: "dbo",
                        principalTable: "Leads",
                        principalColumn: "LeadID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Viewings_SalesAgents_SalesAgentID",
                        column: x => x.SalesAgentID,
                        principalSchema: "dbo",
                        principalTable: "SalesAgents",
                        principalColumn: "SalesAgentID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_SalesAgentID",
                schema: "dbo",
                table: "Users",
                column: "SalesAgentID");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_SalesAgentID",
                schema: "dbo",
                table: "Inventory",
                column: "SalesAgentID");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email",
                schema: "dbo",
                table: "Customers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_ConvertedToLeadID",
                schema: "dbo",
                table: "Inquiries",
                column: "ConvertedToLeadID");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_CreatedDate",
                schema: "dbo",
                table: "Inquiries",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_CustomerID",
                schema: "dbo",
                table: "Inquiries",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_InventoryID",
                schema: "dbo",
                table: "Inquiries",
                column: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_ProjectID",
                schema: "dbo",
                table: "Inquiries",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_CreatedByUserID",
                schema: "dbo",
                table: "LeadActivities",
                column: "CreatedByUserID");

            migrationBuilder.CreateIndex(
                name: "IX_LeadActivities_LeadID",
                schema: "dbo",
                table: "LeadActivities",
                column: "LeadID");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CreatedDate",
                schema: "dbo",
                table: "Leads",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CustomerID",
                schema: "dbo",
                table: "Leads",
                column: "CustomerID");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_InventoryID",
                schema: "dbo",
                table: "Leads",
                column: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ProjectID",
                schema: "dbo",
                table: "Leads",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_SalesAgentID",
                schema: "dbo",
                table: "Leads",
                column: "SalesAgentID");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_Status",
                schema: "dbo",
                table: "Leads",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentPlanMilestones_ProjectID",
                schema: "dbo",
                table: "PaymentPlanMilestones",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_SalesAgents_OrganizationID",
                schema: "dbo",
                table: "SalesAgents",
                column: "OrganizationID");

            migrationBuilder.CreateIndex(
                name: "IX_Viewings_InventoryID",
                schema: "dbo",
                table: "Viewings",
                column: "InventoryID");

            migrationBuilder.CreateIndex(
                name: "IX_Viewings_LeadID",
                schema: "dbo",
                table: "Viewings",
                column: "LeadID");

            migrationBuilder.CreateIndex(
                name: "IX_Viewings_SalesAgentID",
                schema: "dbo",
                table: "Viewings",
                column: "SalesAgentID");

            migrationBuilder.CreateIndex(
                name: "IX_Viewings_ScheduledDate",
                schema: "dbo",
                table: "Viewings",
                column: "ScheduledDate");

            migrationBuilder.CreateIndex(
                name: "IX_Viewings_Status",
                schema: "dbo",
                table: "Viewings",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_Inventory_SalesAgents_SalesAgentID",
                schema: "dbo",
                table: "Inventory",
                column: "SalesAgentID",
                principalSchema: "dbo",
                principalTable: "SalesAgents",
                principalColumn: "SalesAgentID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_SalesAgents_SalesAgentID",
                schema: "dbo",
                table: "Users",
                column: "SalesAgentID",
                principalSchema: "dbo",
                principalTable: "SalesAgents",
                principalColumn: "SalesAgentID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inventory_SalesAgents_SalesAgentID",
                schema: "dbo",
                table: "Inventory");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_SalesAgents_SalesAgentID",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Inquiries",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LeadActivities",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PaymentPlanMilestones",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Viewings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Leads",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SalesAgents",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Organizations",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Users_SalesAgentID",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Inventory_SalesAgentID",
                schema: "dbo",
                table: "Inventory");

            migrationBuilder.DropColumn(
                name: "SalesAgentID",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SalesAgentID",
                schema: "dbo",
                table: "Inventory");
        }
    }
}
