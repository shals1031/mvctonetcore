using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

namespace TeliconLatest.Migrations
{
    public partial class initailDB2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "adm01120",
                columns: table => new
                {
                    splitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
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
                });

            migrationBuilder.CreateTable(
                name: "adm01300",
                columns: table => new
                {
                    accountID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    accountName = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: true),
                    address1 = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: true),
                    address2 = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: true),
                    address3 = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: true),
                    accType = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    accountNo = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.accountID);
                });

            migrationBuilder.CreateTable(
                name: "adm02100",
                columns: table => new
                {
                    BankId = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    BankName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.BankId);
                });

            migrationBuilder.CreateTable(
                name: "adm02300",
                columns: table => new
                {
                    BatchID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BatchDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.BatchID);
                });

            migrationBuilder.CreateTable(
                name: "adm03200",
                columns: table => new
                {
                    CustID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    CustName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Street = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    City = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Parish = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Phone1 = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    Phone2 = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    Fax = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    Email = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    CustClass = table.Column<int>(type: "int", nullable: false),
                    ClientCode = table.Column<string>(type: "varchar(6)", maxLength: 6, nullable: false),
                    Currency = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: true),
                    StartDt = table.Column<DateTime>(type: "datetime", nullable: true),
                    EndDt = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.CustID);
                });

            migrationBuilder.CreateTable(
                name: "adm03500",
                columns: table => new
                {
                    ClassId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ClassName = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ClassId);
                });

            migrationBuilder.CreateTable(
                name: "adm04100",
                columns: table => new
                {
                    DeductionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DeductionID);
                });

            migrationBuilder.CreateTable(
                name: "adm04200",
                columns: table => new
                {
                    DepartmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.DepartmentID);
                });

            migrationBuilder.CreateTable(
                name: "adm07100",
                columns: table => new
                {
                    TaxId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    Percentage = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.TaxId);
                });

            migrationBuilder.CreateTable(
                name: "adm12100",
                columns: table => new
                {
                    locationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    locationName = table.Column<string>(type: "varchar(35)", maxLength: 35, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.locationID);
                });

            migrationBuilder.CreateTable(
                name: "adm13100",
                columns: table => new
                {
                    MaterialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MaterialName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    MaterialUnit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    MaterialCode = table.Column<string>(type: "text", nullable: false),
                    MaxQty = table.Column<decimal>(type: "decimal(18, 2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.MaterialID);
                });

            migrationBuilder.CreateTable(
                name: "adm16100",
                columns: table => new
                {
                    PeriodID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Week = table.Column<int>(type: "int", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    PayDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    periodYear = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.PeriodID);
                });

            migrationBuilder.CreateTable(
                name: "adm16200",
                columns: table => new
                {
                    POID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PONUM = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: false),
                    TOTAL = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    BALANCE = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    Description = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                    IsClosed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PODate = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.POID);
                });

            migrationBuilder.CreateTable(
                name: "adm18100",
                columns: table => new
                {
                    remarkID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    remarkText = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.remarkID);
                });

            migrationBuilder.CreateTable(
                name: "adm22100",
                columns: table => new
                {
                    VehicleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PlateNo = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    FleetNo = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false),
                    OwnedBy = table.Column<int>(type: "int", nullable: false),
                    ConID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.VehicleID);
                });

            migrationBuilder.CreateTable(
                name: "adm26100",
                columns: table => new
                {
                    ZoneID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: false),
                    SupervisorName = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                    Addr = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true),
                    Email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Place = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.ZoneID);
                });

            migrationBuilder.CreateTable(
                name: "trn09101",
                columns: table => new
                {
                    newnum = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    InvoiceTitle = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    NewInvNo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.newnum);
                });

            migrationBuilder.CreateTable(
                name: "trn17100",
                columns: table => new
                {
                    QuotationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Quot_ref = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Quot_title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Requestby = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Requestdt = table.Column<DateTime>(type: "datetime", nullable: false),
                    RequestStreet = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true),
                    RequestCity = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true),
                    RequestCountry = table.Column<string>(type: "varchar(75)", maxLength: 75, nullable: true),
                    CreateBy = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "adm02200",
                columns: table => new
                {
                    RecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BankId = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: false),
                    BranchId = table.Column<string>(type: "varchar(11)", maxLength: 11, nullable: false),
                    BranchName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "trn09100",
                columns: table => new
                {
                    InvoiceNum = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    InvoiceTitle = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    GeneratedDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    InvoiceTotal = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    NewInvNo = table.Column<int>(type: "int", nullable: true),
                    batchTMP = table.Column<int>(type: "int", nullable: true),
                    statusTMP = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    InvNewNum = table.Column<int>(type: "int", nullable: false),
                    IsNewFormat = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ADM02300BatchID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.InvoiceNum);
                    table.ForeignKey(
                        name: "FK_trn09100_adm02300_ADM02300BatchID",
                        column: x => x.ADM02300BatchID,
                        principalTable: "adm02300",
                        principalColumn: "BatchID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "adm01100",
                columns: table => new
                {
                    RateID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RateDescr = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    RateUnit = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: false),
                    RateClass = table.Column<int>(type: "int", nullable: false),
                    Active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ClientID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    MaxQty = table.Column<decimal>(type: "decimal(18, 2)", nullable: true),
                    HasMaterials = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaterialsRequired = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AltCode = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "adm01400",
                columns: table => new
                {
                    areaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    zoneID = table.Column<int>(type: "int", nullable: false),
                    areaName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "trn19100",
                columns: table => new
                {
                    SInvoiceId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SInv_ref = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    SInv_title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    ZoneId = table.Column<int>(type: "int", nullable: false),
                    Requestdt = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreateBy = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "trn17110",
                columns: table => new
                {
                    RecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    QuotationId = table.Column<int>(type: "int", nullable: false),
                    ActivityDesc = table.Column<string>(type: "text", nullable: true),
                    ActQty = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "adm03300",
                columns: table => new
                {
                    ConID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    EmployeeID = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    FirstName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    LastName = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Street = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Parish = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Phone1 = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                    Phone2 = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                    Email = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    TRN = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime", nullable: true),
                    ConClass = table.Column<int>(type: "int", nullable: false),
                    EmergencyCon = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    EmerPhone1 = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    EmerPhone2 = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    EmerRelation = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    DLicence = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true),
                    Bankacc = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    Branch = table.Column<int>(type: "int", nullable: true),
                    DepartmentID = table.Column<int>(type: "int", nullable: false),
                    TechNo = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                    PlateNo = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: true),
                    AreaID = table.Column<int>(type: "int", nullable: true),
                    LocationID = table.Column<int>(type: "int", nullable: true),
                    payScale = table.Column<int>(type: "int", nullable: true),
                    isActive = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    EngagementDt = table.Column<DateTime>(type: "datetime", nullable: true),
                    NIS = table.Column<string>(type: "varchar(15)", maxLength: 15, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "adm01110",
                columns: table => new
                {
                    ActMatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
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
                });

            migrationBuilder.CreateTable(
                name: "adm01150",
                columns: table => new
                {
                    RateHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RateID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "double", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "adm01250",
                columns: table => new
                {
                    RateHistoryID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    RateID = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<double>(type: "double", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "adm04210",
                columns: table => new
                {
                    DepActID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
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
                });

            migrationBuilder.CreateTable(
                name: "trn23100",
                columns: table => new
                {
                    Workid = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Wo_ref = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Wo_title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Requestby = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true),
                    Requestdt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Wo_client = table.Column<int>(type: "int", nullable: false),
                    Wo_split = table.Column<double>(type: "double", nullable: false),
                    Wo_split2 = table.Column<double>(type: "double", nullable: false),
                    Dispatchdt = table.Column<DateTime>(type: "datetime", nullable: false),
                    Status = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    Submitted = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DateSubmitted = table.Column<DateTime>(type: "datetime", nullable: true),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    SpliceDocs = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreateBy = table.Column<string>(type: "varchar(36)", maxLength: 36, nullable: true),
                    LocationID = table.Column<int>(type: "int", nullable: true),
                    AreaID = table.Column<int>(type: "int", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    dateVerified = table.Column<DateTime>(type: "datetime", nullable: true),
                    RollbackDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    PONum = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    tempValue = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: true),
                    CompletionDt = table.Column<DateTime>(type: "datetime", nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "trn19110",
                columns: table => new
                {
                    RecId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    SInvoiceId = table.Column<int>(type: "int", nullable: false),
                    ActivityId = table.Column<int>(type: "int", nullable: false),
                    ActQty = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "trn04100",
                columns: table => new
                {
                    DeductionConductorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    ConductorID = table.Column<int>(type: "int", nullable: false),
                    DeductionID = table.Column<int>(type: "int", nullable: false),
                    Recurring = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Amount = table.Column<double>(type: "double", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime", nullable: true),
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
                });

            migrationBuilder.CreateTable(
                name: "adm03400",
                columns: table => new
                {
                    RecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
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
                });

            migrationBuilder.CreateTable(
                name: "trn13120",
                columns: table => new
                {
                    MergedOrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    MergerdTitle = table.Column<string>(type: "text", nullable: true),
                    MergerdDate = table.Column<DateTime>(type: "datetime", nullable: true),
                    MergedRefNum = table.Column<string>(type: "text", nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "trn23110",
                columns: table => new
                {
                    RecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    WorkOID = table.Column<int>(type: "int", nullable: false),
                    ActivityID = table.Column<int>(type: "int", nullable: false),
                    ActQty = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    OActQty = table.Column<decimal>(type: "decimal(18, 2)", nullable: false),
                    Location = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    ActDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    InvFlag = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AddMaterial = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ActPaid = table.Column<DateTime>(type: "datetime", nullable: true),
                    AdtnlDetails = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: true)
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
                });

            migrationBuilder.CreateTable(
                name: "trn13110",
                columns: table => new
                {
                    MergedSubOrderId = table.Column<int>(type: "int", nullable: false),
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
                });

            migrationBuilder.CreateTable(
                name: "trn09110",
                columns: table => new
                {
                    WoActID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    InvoiceNum = table.Column<int>(type: "int", nullable: false),
                    InvoicedAmount = table.Column<decimal>(type: "decimal(18, 2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.WoActID, x.InvoiceNum });
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
                });

            migrationBuilder.CreateTable(
                name: "trn23120",
                columns: table => new
                {
                    WoMatRecID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
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
                });

            migrationBuilder.CreateIndex(
                name: "FK_ADM01100_ADM03500",
                table: "adm01100",
                column: "RateClass");

            migrationBuilder.CreateIndex(
                name: "FK_ADM01100_ADM04200",
                table: "adm01100",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "FK_ADM01110_ADM01100",
                table: "adm01110",
                column: "ActivID");

            migrationBuilder.CreateIndex(
                name: "FK_ADM01110_ADM13100",
                table: "adm01110",
                column: "MaterID");

            migrationBuilder.CreateIndex(
                name: "FK_ADM01150_ADM01100",
                table: "adm01150",
                column: "RateID");

            migrationBuilder.CreateIndex(
                name: "FK_ADM01250_ADM01100",
                table: "adm01250",
                column: "RateID");

            migrationBuilder.CreateIndex(
                name: "FK_ADM01400_ADM26100",
                table: "adm01400",
                column: "zoneID");

            migrationBuilder.CreateIndex(
                name: "FK_ADM02200_ADM02100",
                table: "adm02200",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "FK_ADM03300_ADM02200",
                table: "adm03300",
                column: "Branch");

            migrationBuilder.CreateIndex(
                name: "FK_ADM03300_ADM04200",
                table: "adm03300",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "FK_ADM03400_ADM03300",
                table: "adm03400",
                column: "ContractorID");

            migrationBuilder.CreateIndex(
                name: "FK_ADM03400_TRN23100",
                table: "adm03400",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "FK_ADM04210_ADM01100",
                table: "adm04210",
                column: "ActivityID");

            migrationBuilder.CreateIndex(
                name: "FK_ADM04210_ADM04200",
                table: "adm04210",
                column: "DepartmentID");

            migrationBuilder.CreateIndex(
                name: "FK_TRN04100_ADM03300",
                table: "trn04100",
                column: "ConductorID");

            migrationBuilder.CreateIndex(
                name: "FK_TRN04100_ADM04100",
                table: "trn04100",
                column: "DeductionID");

            migrationBuilder.CreateIndex(
                name: "IX_trn09100_ADM02300BatchID",
                table: "trn09100",
                column: "ADM02300BatchID");

            migrationBuilder.CreateIndex(
                name: "FK_TRN09110_TRN09100",
                table: "trn09110",
                column: "InvoiceNum");

            migrationBuilder.CreateIndex(
                name: "FK_TRN13120_TRN13110",
                table: "trn13110",
                column: "MergedOrderId");

            migrationBuilder.CreateIndex(
                name: "FK_TRN23100_TRN13110",
                table: "trn13110",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "FK_TRN13120_TRN23100",
                table: "trn13120",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "FK_TRN17100_Users",
                table: "trn17100",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "FK_TRN17110_TRN17100",
                table: "trn17110",
                column: "QuotationId");

            migrationBuilder.CreateIndex(
                name: "FK_ADM26100_TRN19100",
                table: "trn19100",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "FK_TRN19100_Users",
                table: "trn19100",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "FK_TRN19110_ADM01100",
                table: "trn19110",
                column: "ActivityId");

            migrationBuilder.CreateIndex(
                name: "FK_TRN19110_TRN19100",
                table: "trn19110",
                column: "SInvoiceId");

            migrationBuilder.CreateIndex(
                name: "FK_TRN23100_ADM01400",
                table: "trn23100",
                column: "AreaID");

            migrationBuilder.CreateIndex(
                name: "FK_TRN23100_ADM03200",
                table: "trn23100",
                column: "Wo_client");

            migrationBuilder.CreateIndex(
                name: "FK_TRN23100_ADM03500",
                table: "trn23100",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "FK_TRN23100_Users",
                table: "trn23100",
                column: "CreateBy");

            migrationBuilder.CreateIndex(
                name: "FK_TRN23110_ADM01100",
                table: "trn23110",
                column: "ActivityID");

            migrationBuilder.CreateIndex(
                name: "FK_TRN23110_TRN23100",
                table: "trn23110",
                column: "WorkOID");

            migrationBuilder.CreateIndex(
                name: "FK_TRN23110_TRN23120",
                table: "trn23120",
                column: "WoActID");
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
                name: "adm13100");

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
                name: "adm02200");

            migrationBuilder.DropTable(
                name: "adm02300");

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
                name: "adm26100");
        }
    }
}
