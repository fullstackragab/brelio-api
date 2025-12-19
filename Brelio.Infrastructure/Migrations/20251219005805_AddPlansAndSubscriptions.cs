using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Brelio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlansAndSubscriptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PriceMonthly = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    PriceYearly = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "USDC"),
                    InvoiceLimit = table.Column<int>(type: "integer", nullable: false),
                    IsUnlimited = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCustom = table.Column<bool>(type: "boolean", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TargetAudience = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FeatureUsdc = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureSol = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureCustomBranding = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureAutoConfirmation = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureEmailNotifications = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureCsvExport = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureMultiWallet = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureWebhooksApi = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureInvoiceMetadata = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureAdvancedAnalytics = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureMultiUser = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureMultiOrg = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureTeamRoles = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureAutoReminders = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureSla = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureCustomIntegrations = table.Column<bool>(type: "boolean", nullable: false),
                    FeatureDedicatedSupport = table.Column<bool>(type: "boolean", nullable: false),
                    SupportLevel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "community")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BillingCycle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "USDC"),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentPeriodStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrentPeriodEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PaymentTxSignature = table.Column<string>(type: "character varying(88)", maxLength: 88, nullable: true),
                    PaymentWalletAddress = table.Column<string>(type: "character varying(44)", maxLength: 44, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Slug",
                table: "Plans",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscriptions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Plans");
        }
    }
}
