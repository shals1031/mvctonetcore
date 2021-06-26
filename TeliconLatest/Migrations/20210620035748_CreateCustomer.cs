using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace TeliconLatest.Migrations
{
    public partial class CreateCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    AccountNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    JobDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    TechName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    TechId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ServiceOrdNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    CustAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    IsWTNew = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsWTExisting = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsWTRewireCable = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsWTAddOutlet = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsWTServiceRepOrd = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsWTNotDone = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsWTServiceCall = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSIWatch = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSITalk = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsSIClick = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CustComment = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    TimeIn = table.Column<TimeSpan>(type: "time", nullable: false),
                    TimeOut = table.Column<TimeSpan>(type: "time", nullable: false),
                    CustSignature = table.Column<string>(type: "text", nullable: true),
                    CustDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    TechSignature = table.Column<string>(type: "text", nullable: true),
                    TechDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                });

            migrationBuilder.CreateTable(
                name: "CustomerEquipments",
                columns: table => new
                {
                    CustomerEquipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    MacDetail = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    SerialNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerEquipments", x => x.CustomerEquipmentId);
                    table.ForeignKey(
                        name: "FK_CustomerEquipments_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "CustomerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerEquipments_CustomerId",
                table: "CustomerEquipments",
                column: "CustomerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerEquipments");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
