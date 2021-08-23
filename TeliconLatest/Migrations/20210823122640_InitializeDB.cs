using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TeliconLatest.Migrations
{
    public partial class InitializeDB : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm01120",
                columns: table => new
                {
                    splitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    activityID = table.Column<int>(type: "int", nullable: false),
                    OVSenior1 = table.Column<double>(type: "double", nullable: false),
                    OVSenior2 = table.Column<double>(type: "double", nullable: false),
                    OVJunior2 = table.Column<double>(type: "double", nullable: false),
                    CVSenior = table.Column<double>(type: "double", nullable: false),
                    CVJunior = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.splitID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm01300",
                columns: table => new
                {
                    accountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    accountName = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address1 = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address2 = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address3 = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    accType = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    accountNo = table.Column<string>(type: "varchar(16)", maxLength: 16, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.accountID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm02100",
                columns: table => new
                {
                    BankId = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BankName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.BankId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm02300",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BatchDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.BatchID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm03200",
                columns: table => new
                {
                    CustID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Street = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Parish = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone1 = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone2 = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fax = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustClass = table.Column<int>(type: "int", nullable: false),
                    ClientCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EndDt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.CustID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm03500",
                columns: table => new
                {
                    ClassId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClassName = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ClassId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm04100",
                columns: table => new
                {
                    DeductionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DeductionID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm04200",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DepartmentID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm07100",
                columns: table => new
                {
                    TaxId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Percentage = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TaxId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm12100",
                columns: table => new
                {
                    locationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    locationName = table.Column<string>(type: "varchar(70)", maxLength: 70, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.locationID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm13100",
                columns: table => new
                {
                    MaterialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MaterialName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaterialUnit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaterialCode = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MaxQty = table.Column<decimal>(type: "decimal(65,30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.MaterialID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm16100",
                columns: table => new
                {
                    PeriodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Week = table.Column<int>(type: "int", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PayDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    periodYear = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PeriodID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm16200",
                columns: table => new
                {
                    POID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PONUM = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TOTAL = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BALANCE = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PODate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.POID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm18100",
                columns: table => new
                {
                    remarkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    remarkText = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.remarkID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm22100",
                columns: table => new
                {
                    VehicleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlateNo = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FleetNo = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OwnedBy = table.Column<int>(type: "int", nullable: false),
                    ConID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.VehicleID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm26100",
                columns: table => new
                {
                    ZoneID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SupervisorName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Addr = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Place = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ZoneID);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "applications",
                columns: table => new
                {
                    ApplicationId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplicationName = table.Column<string>(type: "varchar(470)", maxLength: 470, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ApplicationId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AccountNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    JobDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TechName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TechId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ServiceOrdNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustAddress = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
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
                    CustComment = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TimeIn = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    TimeOut = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    CustSignature = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CustDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TechSignature = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TechDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tasksinrole",
                columns: table => new
                {
                    RoleTaskId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    CanRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CanWrite = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RoleTaskId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn09100",
                columns: table => new
                {
                    InvoiceNum = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InvoiceTitle = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewInvNo = table.Column<int>(type: "int", nullable: true),
                    batchTMP = table.Column<int>(type: "int", nullable: true),
                    statusTMP = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InvNewNum = table.Column<int>(type: "int", nullable: false),
                    IsNewFormat = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.InvoiceNum);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn09101",
                columns: table => new
                {
                    newnum = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InvoiceTitle = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GeneratedDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NewInvNo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.newnum);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm02200",
                columns: table => new
                {
                    RecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BankId = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BranchId = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BranchName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RecID);
                    table.ForeignKey(
                        name: "FK_ADM02200_ADM02100",
                        column: x => x.BankId,
                        principalTable: "adm02100",
                        principalColumn: "BankId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm01100",
                columns: table => new
                {
                    RateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RateDescr = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RateUnit = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RateClass = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ClientID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    MaxQty = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    HasMaterials = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaterialsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AltCode = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RateID);
                    table.ForeignKey(
                        name: "FK_ADM01100_ADM03500",
                        column: x => x.RateClass,
                        principalTable: "adm03500",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM01100_ADM04200",
                        column: x => x.DepartmentId,
                        principalTable: "adm04200",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm01400",
                columns: table => new
                {
                    areaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    zoneID = table.Column<int>(type: "int", nullable: false),
                    areaName = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.areaID);
                    table.ForeignKey(
                        name: "FK_ADM01400_ADM26100",
                        column: x => x.zoneID,
                        principalTable: "adm26100",
                        principalColumn: "ZoneID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    RoleId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplicationId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleName = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_Roles_Applications",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplicationId = table.Column<string>(type: "varchar(36)", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsAnonymous = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    LastActivityDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Applications",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CustomerEquipments",
                columns: table => new
                {
                    CustomerEquipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    MacDetail = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SerialNo = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
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
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm03300",
                columns: table => new
                {
                    ConID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmployeeID = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FirstName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Street = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    City = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Parish = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone1 = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Phone2 = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TRN = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DOB = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ConClass = table.Column<int>(type: "int", nullable: false),
                    EmergencyCon = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmerPhone1 = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmerPhone2 = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmerRelation = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DLicence = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Bankacc = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Branch = table.Column<int>(type: "int", nullable: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    TechNo = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlateNo = table.Column<string>(type: "varchar(14)", maxLength: 14, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AreaID = table.Column<int>(type: "int", nullable: true),
                    LocationID = table.Column<int>(type: "int", nullable: true),
                    payScale = table.Column<int>(type: "int", nullable: true),
                    isActive = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    EngagementDt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    NIS = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ConID);
                    table.ForeignKey(
                        name: "FK_ADM03300_ADM02200",
                        column: x => x.Branch,
                        principalTable: "adm02200",
                        principalColumn: "RecID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM03300_ADM04200",
                        column: x => x.DepartmentID,
                        principalTable: "adm04200",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm01110",
                columns: table => new
                {
                    ActMatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ActivID = table.Column<int>(type: "int", nullable: false),
                    MaterID = table.Column<int>(type: "int", nullable: false),
                    ActMatQty = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ActMatID);
                    table.ForeignKey(
                        name: "FK_ADM01110_ADM01100",
                        column: x => x.ActivID,
                        principalTable: "adm01100",
                        principalColumn: "RateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM01110_ADM13100",
                        column: x => x.MaterID,
                        principalTable: "adm13100",
                        principalColumn: "MaterialID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm01150",
                columns: table => new
                {
                    RateHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RateID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "double", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RateHistoryID);
                    table.ForeignKey(
                        name: "FK_ADM01150_ADM01100",
                        column: x => x.RateID,
                        principalTable: "adm01100",
                        principalColumn: "RateID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm01250",
                columns: table => new
                {
                    RateHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RateID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "double", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RateHistoryID);
                    table.ForeignKey(
                        name: "FK_ADM01250_ADM01100",
                        column: x => x.RateID,
                        principalTable: "adm01100",
                        principalColumn: "RateID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm04210",
                columns: table => new
                {
                    DepActID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    ActivityID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DepActID);
                    table.ForeignKey(
                        name: "FK_ADM04210_ADM01100",
                        column: x => x.ActivityID,
                        principalTable: "adm01100",
                        principalColumn: "RateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM04210_ADM04200",
                        column: x => x.DepartmentID,
                        principalTable: "adm04200",
                        principalColumn: "DepartmentID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "memberships",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ApplicationId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Password = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsApproved = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsLockedOut = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastPasswordChangedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastLockoutDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Comment = table.Column<string>(type: "varchar(512)", maxLength: 512, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Memberships_Applications",
                        column: x => x.ApplicationId,
                        principalTable: "applications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Memberships_Users",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "profiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyNames = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyValueStrings = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PropertyValueBinary = table.Column<byte[]>(type: "longblob", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Profiles_Users",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn17100",
                columns: table => new
                {
                    QuotationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Quot_ref = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quot_title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Requestby = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Requestdt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RequestStreet = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestCity = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestCountry = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreateBy = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.QuotationId);
                    table.ForeignKey(
                        name: "FK_TRN17100_Users",
                        column: x => x.CreateBy,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn19100",
                columns: table => new
                {
                    SInvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SInv_ref = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SInv_title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    Requestdt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreateBy = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.SInvoiceId);
                    table.ForeignKey(
                        name: "FK_ADM26100_TRN19100",
                        column: x => x.ZoneId,
                        principalTable: "adm26100",
                        principalColumn: "ZoneID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN19100_Users",
                        column: x => x.CreateBy,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn23100",
                columns: table => new
                {
                    Workid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Wo_ref = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Wo_title = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Requestby = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Requestdt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Wo_client = table.Column<int>(type: "int", nullable: false),
                    Wo_split = table.Column<double>(type: "double", nullable: false),
                    Wo_split2 = table.Column<double>(type: "double", nullable: false),
                    Dispatchdt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Submitted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateSubmitted = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    SpliceDocs = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateBy = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LocationID = table.Column<int>(type: "int", nullable: true),
                    AreaID = table.Column<int>(type: "int", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    dateVerified = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RollbackDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PONum = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tempValue = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CompletionDt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.Workid);
                    table.ForeignKey(
                        name: "FK_TRN23100_ADM01400",
                        column: x => x.AreaID,
                        principalTable: "adm01400",
                        principalColumn: "areaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN23100_ADM03200",
                        column: x => x.Wo_client,
                        principalTable: "adm03200",
                        principalColumn: "CustID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN23100_ADM03500",
                        column: x => x.ClassId,
                        principalTable: "adm03500",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN23100_Users",
                        column: x => x.CreateBy,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usersinroles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "varchar(36)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RoleId = table.Column<string>(type: "varchar(36)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UsersInRoles_Roles",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsersInRoles_Users",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn04100",
                columns: table => new
                {
                    DeductionConductorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConductorID = table.Column<int>(type: "int", nullable: false),
                    DeductionID = table.Column<int>(type: "int", nullable: false),
                    Recurring = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Amount = table.Column<double>(type: "double", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    HoldIt = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    YTDAmount = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DeductionConductorID);
                    table.ForeignKey(
                        name: "FK_TRN04100_ADM03300",
                        column: x => x.ConductorID,
                        principalTable: "adm03300",
                        principalColumn: "ConID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN04100_ADM04100",
                        column: x => x.DeductionID,
                        principalTable: "adm04100",
                        principalColumn: "DeductionID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn17110",
                columns: table => new
                {
                    RecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    ActivityDesc = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActQty = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ActivityRate = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RecId);
                    table.ForeignKey(
                        name: "FK_TRN17110_TRN17100",
                        column: x => x.QuotationId,
                        principalTable: "trn17100",
                        principalColumn: "QuotationId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn19110",
                columns: table => new
                {
                    RecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SInvoiceId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActQty = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ActivityRate = table.Column<double>(type: "double", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RecId);
                    table.ForeignKey(
                        name: "FK_TRN19110_ADM01100",
                        column: x => x.ActivityId,
                        principalTable: "adm01100",
                        principalColumn: "RateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN19110_TRN19100",
                        column: x => x.SInvoiceId,
                        principalTable: "trn19100",
                        principalColumn: "SInvoiceId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "adm03400",
                columns: table => new
                {
                    RecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    ContractorID = table.Column<int>(type: "int", nullable: false),
                    CrewLead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ContractorRate = table.Column<double>(type: "double", nullable: false),
                    IsHide = table.Column<bool>(type: "tinyint(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RecID);
                    table.ForeignKey(
                        name: "FK_ADM03400_ADM03300",
                        column: x => x.ContractorID,
                        principalTable: "adm03300",
                        principalColumn: "ConID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ADM03400_TRN23100",
                        column: x => x.WorkOrderId,
                        principalTable: "trn23100",
                        principalColumn: "Workid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn13120",
                columns: table => new
                {
                    MergedOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    MergerdTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MergerdDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MergedRefNum = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.MergedOrderId);
                    table.ForeignKey(
                        name: "FK_TRN13120_TRN23100",
                        column: x => x.WorkOrderId,
                        principalTable: "trn23100",
                        principalColumn: "Workid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn23110",
                columns: table => new
                {
                    RecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WorkOID = table.Column<int>(type: "int", nullable: false),
                    ActivityID = table.Column<int>(type: "int", nullable: false),
                    ActQty = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    OActQty = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Location = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ActDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    InvFlag = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AddMaterial = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ActPaid = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    AdtnlDetails = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.RecID);
                    table.ForeignKey(
                        name: "FK_TRN23110_ADM01100",
                        column: x => x.ActivityID,
                        principalTable: "adm01100",
                        principalColumn: "RateID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN23110_TRN23100",
                        column: x => x.WorkOID,
                        principalTable: "trn23100",
                        principalColumn: "Workid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn13110",
                columns: table => new
                {
                    MergedSubOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MergedOrderId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.MergedSubOrderId);
                    table.ForeignKey(
                        name: "FK_TRN13120_TRN13110",
                        column: x => x.MergedOrderId,
                        principalTable: "trn13120",
                        principalColumn: "MergedOrderId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN23100_TRN13110",
                        column: x => x.WorkOrderId,
                        principalTable: "trn23100",
                        principalColumn: "Workid",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn09110",
                columns: table => new
                {
                    WoActID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InvoiceNum = table.Column<int>(type: "int", nullable: false),
                    InvoicedAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.WoActID, x.InvoiceNum })
                        .Annotation("MySql:IndexPrefixLength", new[] { 0, 0 });
                    table.ForeignKey(
                        name: "FK_TRN09110_TRN09100",
                        column: x => x.InvoiceNum,
                        principalTable: "trn09100",
                        principalColumn: "InvoiceNum",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TRN09110_TRN23110",
                        column: x => x.WoActID,
                        principalTable: "trn23110",
                        principalColumn: "RecID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "trn23120",
                columns: table => new
                {
                    WoMatRecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    WoMatID = table.Column<int>(type: "int", nullable: false),
                    WoMatQty = table.Column<int>(type: "int", nullable: false),
                    WoActID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.WoMatRecID);
                    table.ForeignKey(
                        name: "FK_TRN23110_TRN23120",
                        column: x => x.WoActID,
                        principalTable: "trn23110",
                        principalColumn: "RecID",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_adm01100_DepartmentId",
                table: "adm01100",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_adm01100_RateClass",
                table: "adm01100",
                column: "RateClass");

            migrationBuilder.CreateIndex(
                name: "IX_adm01110_ActivID",
                table: "adm01110",
                column: "ActivID");

            migrationBuilder.CreateIndex(
                name: "IX_adm01110_MaterID",
                table: "adm01110",
                column: "MaterID");

            migrationBuilder.CreateIndex(
                name: "IX_adm01150_RateID",
                table: "adm01150",
                column: "RateID");

            migrationBuilder.CreateIndex(
                name: "IX_adm01250_RateID",
                table: "adm01250",
                column: "RateID");

            migrationBuilder.CreateIndex(
                name: "IX_adm01400_zoneID",
                table: "adm01400",
                column: "zoneID");

            migrationBuilder.CreateIndex(
                name: "IX_adm02200_BankId",
                table: "adm02200",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_adm03300_Branch",
                table: "adm03300",
                column: "Branch");

            migrationBuilder.CreateIndex(
                name: "IX_adm03300_DepartmentID",
                table: "adm03300",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_adm03400_ContractorID",
                table: "adm03400",
                column: "ContractorID");

            migrationBuilder.CreateIndex(
                name: "IX_adm03400_WorkOrderId",
                table: "adm03400",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_adm04210_ActivityID",
                table: "adm04210",
                column: "ActivityID");

            migrationBuilder.CreateIndex(
                name: "IX_adm04210_DepartmentID",
                table: "adm04210",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerEquipments_CustomerId",
                table: "CustomerEquipments",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_memberships_ApplicationId",
                table: "memberships",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_ApplicationId",
                table: "roles",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_trn04100_ConductorID",
                table: "trn04100",
                column: "ConductorID");

            migrationBuilder.CreateIndex(
                name: "IX_trn04100_DeductionID",
                table: "trn04100",
                column: "DeductionID");

            migrationBuilder.CreateIndex(
                name: "IX_trn09110_InvoiceNum",
                table: "trn09110",
                column: "InvoiceNum");

            migrationBuilder.CreateIndex(
                name: "IX_trn13110_MergedOrderId",
                table: "trn13110",
                column: "MergedOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_trn13110_WorkOrderId",
                table: "trn13110",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_trn13120_WorkOrderId",
                table: "trn13120",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_trn17100_CreateBy",
                table: "trn17100",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_trn17110_QuotationId",
                table: "trn17110",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_trn19100_CreateBy",
                table: "trn19100",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_trn19100_ZoneId",
                table: "trn19100",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_trn19110_ActivityId",
                table: "trn19110",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_trn19110_SInvoiceId",
                table: "trn19110",
                column: "SInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_trn23100_AreaID",
                table: "trn23100",
                column: "AreaID");

            migrationBuilder.CreateIndex(
                name: "IX_trn23100_ClassId",
                table: "trn23100",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_trn23100_CreateBy",
                table: "trn23100",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "IX_trn23100_Wo_client",
                table: "trn23100",
                column: "Wo_client");

            migrationBuilder.CreateIndex(
                name: "IX_trn23110_ActivityID",
                table: "trn23110",
                column: "ActivityID");

            migrationBuilder.CreateIndex(
                name: "IX_trn23110_WorkOID",
                table: "trn23110",
                column: "WorkOID");

            migrationBuilder.CreateIndex(
                name: "IX_trn23120_WoActID",
                table: "trn23120",
                column: "WoActID");

            migrationBuilder.CreateIndex(
                name: "IX_users_ApplicationId",
                table: "users",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_usersinroles_RoleId",
                table: "usersinroles",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "adm01110");

            migrationBuilder.DropTable(
                name: "adm01120");

            migrationBuilder.DropTable(
                name: "adm01150");

            migrationBuilder.DropTable(
                name: "adm01250");

            migrationBuilder.DropTable(
                name: "adm01300");

            migrationBuilder.DropTable(
                name: "adm02300");

            migrationBuilder.DropTable(
                name: "adm03400");

            migrationBuilder.DropTable(
                name: "adm04210");

            migrationBuilder.DropTable(
                name: "adm07100");

            migrationBuilder.DropTable(
                name: "adm12100");

            migrationBuilder.DropTable(
                name: "adm16100");

            migrationBuilder.DropTable(
                name: "adm16200");

            migrationBuilder.DropTable(
                name: "adm18100");

            migrationBuilder.DropTable(
                name: "adm22100");

            migrationBuilder.DropTable(
                name: "CustomerEquipments");

            migrationBuilder.DropTable(
                name: "memberships");

            migrationBuilder.DropTable(
                name: "profiles");

            migrationBuilder.DropTable(
                name: "tasksinrole");

            migrationBuilder.DropTable(
                name: "trn04100");

            migrationBuilder.DropTable(
                name: "trn09101");

            migrationBuilder.DropTable(
                name: "trn09110");

            migrationBuilder.DropTable(
                name: "trn13110");

            migrationBuilder.DropTable(
                name: "trn17110");

            migrationBuilder.DropTable(
                name: "trn19110");

            migrationBuilder.DropTable(
                name: "trn23120");

            migrationBuilder.DropTable(
                name: "usersinroles");

            migrationBuilder.DropTable(
                name: "adm13100");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "adm03300");

            migrationBuilder.DropTable(
                name: "adm04100");

            migrationBuilder.DropTable(
                name: "trn09100");

            migrationBuilder.DropTable(
                name: "trn13120");

            migrationBuilder.DropTable(
                name: "trn17100");

            migrationBuilder.DropTable(
                name: "trn19100");

            migrationBuilder.DropTable(
                name: "trn23110");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "adm02200");

            migrationBuilder.DropTable(
                name: "adm01100");

            migrationBuilder.DropTable(
                name: "trn23100");

            migrationBuilder.DropTable(
                name: "adm02100");

            migrationBuilder.DropTable(
                name: "adm04200");

            migrationBuilder.DropTable(
                name: "adm01400");

            migrationBuilder.DropTable(
                name: "adm03200");

            migrationBuilder.DropTable(
                name: "adm03500");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "adm26100");

            migrationBuilder.DropTable(
                name: "applications");
        }
    }
}
