using Microsoft.EntityFrameworkCore;
using TeliconLatest.Models;

namespace TeliconLatest.DataEntities
{
    public partial class TeliconDbContext : DbContext
    {
        public TeliconDbContext(DbContextOptions<TeliconDbContext> options) : base(options)
        {
        }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<CustomerEquipments> CustomerEquipments { get; set; }
        public virtual DbSet<Applications> Applications { get; set; }
        public virtual DbSet<Memberships> Memberships { get; set; }
        public virtual DbSet<Profiles> Profiles { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<TasksInRoles> TasksInRoles { get; set; }
        public virtual DbSet<Users> Users { get; set; }
        public virtual DbSet<UsersInRoles> UsersInRoles { get; set; }
        public virtual DbSet<ADM01110> ADM01110 { get; set; }
        public virtual DbSet<ADM01120> ADM01120 { get; set; }
        public virtual DbSet<ADM01150> ADM01150 { get; set; }
        public virtual DbSet<ADM01250> ADM01250 { get; set; }
        public virtual DbSet<ADM01300> ADM01300 { get; set; }
        public virtual DbSet<ADM02100> ADM02100 { get; set; }
        public virtual DbSet<ADM02200> ADM02200 { get; set; }
        public virtual DbSet<ADM02300> ADM02300 { get; set; }
        public virtual DbSet<ADM03400> ADM03400 { get; set; }
        public virtual DbSet<ADM04100> ADM04100 { get; set; }
        public virtual DbSet<ADM04200> ADM04200 { get; set; }
        public virtual DbSet<ADM07100> ADM07100 { get; set; }
        public virtual DbSet<ADM12100> ADM12100 { get; set; }
        public virtual DbSet<ADM13100> ADM13100 { get; set; }
        public virtual DbSet<ADM16100> ADM16100 { get; set; }
        public virtual DbSet<ADM22100> ADM22100 { get; set; }
        public virtual DbSet<TRN04100> TRN04100 { get; set; }
        public virtual DbSet<TRN09101> TRN09101 { get; set; }
        public virtual DbSet<TRN09110> TRN09110 { get; set; }
        public virtual DbSet<TRN23110> TRN23110 { get; set; }
        public virtual DbSet<ADM18100> ADM18100 { get; set; }
        public virtual DbSet<TRN13110> TRN13110 { get; set; }
        public virtual DbSet<TRN13120> TRN13120 { get; set; }
        public virtual DbSet<TRN23120> TRN23120 { get; set; }
        public virtual DbSet<TRN17110> TRN17110 { get; set; }
        public virtual DbSet<TRN17100> TRN17100 { get; set; }
        public virtual DbSet<ADM03500> ADM03500 { get; set; }
        public virtual DbSet<ADM01400> ADM01400 { get; set; }
        public virtual DbSet<ADM26100> ADM26100 { get; set; }
        public virtual DbSet<ADM03200> ADM03200 { get; set; }
        public virtual DbSet<ADM03300> ADM03300 { get; set; }
        public virtual DbSet<ADM16200> ADM16200 { get; set; }
        public virtual DbSet<ADM04210> ADM04210 { get; set; }
        public virtual DbSet<TRN23100> TRN23100 { get; set; }
        public virtual DbSet<ADM01100> ADM01100 { get; set; }
        public virtual DbSet<TRN19110> TRN19110 { get; set; }
        public virtual DbSet<TRN19100> TRN19100 { get; set; }
        public virtual DbSet<TRN09100> TRN09100 { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Applications>(entity =>
            {
                entity.HasKey(e => e.ApplicationId)
                    .HasName("PRIMARY");

                entity.ToTable("applications");
            });

            modelBuilder.Entity<Memberships>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("memberships");

                entity.HasOne(d => d.Applications)
                    .WithMany(p => p.Memberships)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Memberships_Applications");

                entity.HasOne(d => d.Users)
                    .WithOne(p => p.Membership)
                    .HasForeignKey<Memberships>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Memberships_Users");
            });

            modelBuilder.Entity<Profiles>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("profiles");

                entity.Property(e => e.PropertyValueBinary)
                    .IsRequired()
                    .HasColumnType("longblob");

                entity.HasOne(d => d.Users)
                    .WithOne(p => p.Profiles)
                    .HasForeignKey<Profiles>(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Profiles_Users");
            });

            modelBuilder.Entity<Roles>(entity =>
            {
                entity.ToTable("roles");

                entity.HasOne(d => d.Applications)
                    .WithMany(p => p.Roles)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Roles_Applications");
            });

            modelBuilder.Entity<TasksInRoles>(entity =>
            {
                entity.HasKey(e => e.RoleTaskId)
                    .HasName("PRIMARY");

                entity.ToTable("tasksinrole");
            });

            modelBuilder.Entity<Users>(entity =>
            {
                entity.ToTable("users");

                entity.HasOne(d => d.Application)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.ApplicationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Users_Applications");
            });

            modelBuilder.Entity<UsersInRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId })
                    .HasName("PRIMARY");

                entity.ToTable("usersinroles");

                entity.HasOne(d => d.Roles)
                    .WithMany(p => p.UsersInRoles)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsersInRoles_Roles");

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.UsersInRoles)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_UsersInRoles_Users");
            });

            modelBuilder.Entity<ADM01100>(entity =>
            {
                entity.HasKey(e => e.RateID)
                    .HasName("PRIMARY");

                entity.ToTable("adm01100");

                entity.HasOne(d => d.ADM04200)
                    .WithMany(p => p.ADM01100)
                    .HasForeignKey(d => d.DepartmentId)
                    .HasConstraintName("FK_ADM01100_ADM04200");

                entity.HasOne(d => d.ADM03500)
                    .WithMany(p => p.ADM01100)
                    .HasForeignKey(d => d.RateClass)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM01100_ADM03500");
            });

            modelBuilder.Entity<ADM01110>(entity =>
            {
                entity.HasKey(e => e.ActMatID)
                    .HasName("PRIMARY");

                entity.ToTable("adm01110");

                entity.HasOne(d => d.ADM01100)
                    .WithMany(p => p.ADM01110)
                    .HasForeignKey(d => d.ActivID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM01110_ADM01100");

                entity.HasOne(d => d.ADM13100)
                    .WithMany(p => p.ADM01110)
                    .HasForeignKey(d => d.MaterID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM01110_ADM13100");
            });

            modelBuilder.Entity<ADM01120>(entity =>
            {
                entity.HasKey(e => e.splitID)
                    .HasName("PRIMARY");

                entity.ToTable("adm01120");
            });

            modelBuilder.Entity<ADM01150>(entity =>
            {
                entity.HasKey(e => e.RateHistoryID)
                    .HasName("PRIMARY");

                entity.ToTable("adm01150");

                entity.HasOne(d => d.ADM01100)
                    .WithMany(p => p.ADM01150)
                    .HasForeignKey(d => d.RateID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM01150_ADM01100");
            });

            modelBuilder.Entity<ADM01250>(entity =>
            {
                entity.HasKey(e => e.RateHistoryID)
                    .HasName("PRIMARY");

                entity.ToTable("adm01250");

                entity.HasOne(d => d.ADM01100)
                    .WithMany(p => p.ADM01250)
                    .HasForeignKey(d => d.RateID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM01250_ADM01100");
            });

            modelBuilder.Entity<ADM01300>(entity =>
            {
                entity.HasKey(e => e.accountID)
                    .HasName("PRIMARY");

                entity.ToTable("adm01300");
            });

            modelBuilder.Entity<ADM01400>(entity =>
            {
                entity.HasKey(e => e.areaID)
                    .HasName("PRIMARY");

                entity.ToTable("adm01400");

                entity.HasOne(d => d.ADM26100)
                    .WithMany(p => p.ADM01400)
                    .HasForeignKey(d => d.zoneID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM01400_ADM26100");
            });

            modelBuilder.Entity<ADM02100>(entity =>
            {
                entity.HasKey(e => e.BankId)
                    .HasName("PRIMARY");

                entity.ToTable("adm02100");
            });

            modelBuilder.Entity<ADM02200>(entity =>
            {
                entity.HasKey(e => e.RecID)
                    .HasName("PRIMARY");

                entity.ToTable("adm02200");

                entity.HasOne(d => d.ADM02100)
                    .WithMany(p => p.ADM02200)
                    .HasForeignKey(d => d.BankId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM02200_ADM02100");
            });

            modelBuilder.Entity<ADM02300>(entity =>
            {
                entity.HasKey(e => e.BatchID)
                    .HasName("PRIMARY");

                entity.ToTable("adm02300");
            });

            modelBuilder.Entity<ADM03200>(entity =>
            {
                entity.HasKey(e => e.CustID)
                    .HasName("PRIMARY");

                entity.ToTable("adm03200");
            });

            modelBuilder.Entity<ADM03300>(entity =>
            {
                entity.HasKey(e => e.ConID)
                    .HasName("PRIMARY");

                entity.ToTable("adm03300");

                entity.HasOne(d => d.ADM02200)
                    .WithMany(p => p.ADM03300)
                    .HasForeignKey(d => d.Branch)
                    .HasConstraintName("FK_ADM03300_ADM02200");

                entity.HasOne(d => d.ADM04200)
                    .WithMany(p => p.ADM03300)
                    .HasForeignKey(d => d.DepartmentID)
                    .HasConstraintName("FK_ADM03300_ADM04200");
            });

            modelBuilder.Entity<ADM03400>(entity =>
            {
                entity.HasKey(e => e.RecID)
                    .HasName("PRIMARY");

                entity.ToTable("adm03400");

                entity.HasOne(d => d.ADM03300)
                    .WithMany(p => p.ADM03400)
                    .HasForeignKey(d => d.ContractorID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM03400_ADM03300");

                entity.HasOne(d => d.TRN23100)
                    .WithMany(p => p.ADM03400)
                    .HasForeignKey(d => d.WorkOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM03400_TRN23100");
            });

            modelBuilder.Entity<ADM03500>(entity =>
            {
                entity.HasKey(e => e.ClassId)
                    .HasName("PRIMARY");

                entity.ToTable("adm03500");
            });

            modelBuilder.Entity<ADM04100>(entity =>
            {
                entity.HasKey(e => e.DeductionID)
                    .HasName("PRIMARY");

                entity.ToTable("adm04100");
            });

            modelBuilder.Entity<ADM04200>(entity =>
            {
                entity.HasKey(e => e.DepartmentID)
                    .HasName("PRIMARY");

                entity.ToTable("adm04200");
            });

            modelBuilder.Entity<ADM04210>(entity =>
            {
                entity.HasKey(e => e.DepActID)
                    .HasName("PRIMARY");

                entity.ToTable("adm04210");

                entity.HasOne(d => d.ADM01100)
                    .WithMany(p => p.ADM04210)
                    .HasForeignKey(d => d.ActivityID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM04210_ADM01100");

                entity.HasOne(d => d.ADM04200)
                    .WithMany(p => p.ADM04210)
                    .HasForeignKey(d => d.DepartmentID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM04210_ADM04200");
            });

            modelBuilder.Entity<ADM07100>(entity =>
            {
                entity.HasKey(e => e.TaxId)
                    .HasName("PRIMARY");

                entity.ToTable("adm07100");
            });

            modelBuilder.Entity<ADM12100>(entity =>
            {
                entity.HasKey(e => e.locationID)
                    .HasName("PRIMARY");

                entity.ToTable("adm12100");
            });

            modelBuilder.Entity<ADM13100>(entity =>
            {
                entity.HasKey(e => e.MaterialID)
                    .HasName("PRIMARY");

                entity.ToTable("adm13100");
            });

            modelBuilder.Entity<ADM16100>(entity =>
            {
                entity.HasKey(e => e.PeriodID)
                    .HasName("PRIMARY");

                entity.ToTable("adm16100");
            });

            modelBuilder.Entity<ADM16200>(entity =>
            {
                entity.HasKey(e => e.POID)
                    .HasName("PRIMARY");

                entity.ToTable("adm16200");
            });

            modelBuilder.Entity<ADM18100>(entity =>
            {
                entity.HasKey(e => e.remarkID)
                    .HasName("PRIMARY");

                entity.ToTable("adm18100");
            });

            modelBuilder.Entity<ADM22100>(entity =>
            {
                entity.HasKey(e => e.VehicleID)
                    .HasName("PRIMARY");

                entity.ToTable("adm22100");
            });

            modelBuilder.Entity<ADM26100>(entity =>
            {
                entity.HasKey(e => e.ZoneID)
                    .HasName("PRIMARY");

                entity.ToTable("adm26100");
            });

            modelBuilder.Entity<TRN04100>(entity =>
            {
                entity.HasKey(e => e.DeductionConductorID)
                    .HasName("PRIMARY");

                entity.ToTable("trn04100");

                entity.HasOne(d => d.ADM03300)
                    .WithMany(p => p.TRN04100)
                    .HasForeignKey(d => d.ConductorID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN04100_ADM03300");

                entity.HasOne(d => d.ADM04100)
                    .WithMany(p => p.TRN04100)
                    .HasForeignKey(d => d.DeductionID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN04100_ADM04100");
            });

            modelBuilder.Entity<TRN09100>(entity =>
            {
                entity.HasKey(e => e.InvoiceNum)
                    .HasName("PRIMARY");

                entity.ToTable("trn09100");
            });

            modelBuilder.Entity<TRN09101>(entity =>
            {
                entity.HasKey(e => e.newnum)
                    .HasName("PRIMARY");

                entity.ToTable("trn09101");
            });

            modelBuilder.Entity<TRN09110>(entity =>
            {
                entity.HasKey(e => new { e.WoActID, e.InvoiceNum })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("trn09110");

                entity.HasOne(d => d.TRN09100)
                    .WithMany(p => p.TRN09110)
                    .HasForeignKey(d => d.InvoiceNum)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN09110_TRN09100");

                entity.HasOne(d => d.TRN23110)
                    .WithMany(p => p.TRN09110)
                    .HasForeignKey(d => d.WoActID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN09110_TRN23110");
            });

            modelBuilder.Entity<TRN13110>(entity =>
            {
                entity.HasKey(e => e.MergedSubOrderId)
                    .HasName("PRIMARY");

                entity.ToTable("trn13110");

                entity.HasOne(d => d.TRN13120)
                    .WithMany(p => p.TRN13110)
                    .HasForeignKey(d => d.MergedOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN13120_TRN13110");

                entity.HasOne(d => d.TRN23100)
                    .WithMany(p => p.TRN13110)
                    .HasForeignKey(d => d.WorkOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN23100_TRN13110");
            });

            modelBuilder.Entity<TRN13120>(entity =>
            {
                entity.HasKey(e => e.MergedOrderId)
                    .HasName("PRIMARY");

                entity.ToTable("trn13120");

                entity.HasOne(d => d.TRN23100)
                    .WithMany(p => p.TRN13120)
                    .HasForeignKey(d => d.WorkOrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN13120_TRN23100");
            });

            modelBuilder.Entity<TRN17100>(entity =>
            {
                entity.HasKey(e => e.QuotationId)
                    .HasName("PRIMARY");

                entity.ToTable("trn17100");

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.TRN17100)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN17100_Users");
            });

            modelBuilder.Entity<TRN17110>(entity =>
            {
                entity.HasKey(e => e.RecId)
                    .HasName("PRIMARY");

                entity.ToTable("trn17110");

                entity.HasOne(d => d.TRN17100)
                    .WithMany(p => p.TRN17110)
                    .HasForeignKey(d => d.QuotationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN17110_TRN17100");
            });

            modelBuilder.Entity<TRN19100>(entity =>
            {
                entity.HasKey(e => e.SInvoiceId)
                    .HasName("PRIMARY");

                entity.ToTable("trn19100");

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.TRN19100)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN19100_Users");

                entity.HasOne(d => d.ADM26100)
                    .WithMany(p => p.TRN19100)
                    .HasForeignKey(d => d.ZoneId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ADM26100_TRN19100");
            });

            modelBuilder.Entity<TRN19110>(entity =>
            {
                entity.HasKey(e => e.RecId)
                    .HasName("PRIMARY");

                entity.ToTable("trn19110");

                entity.HasOne(d => d.ADM01100)
                    .WithMany(p => p.TRN19110)
                    .HasForeignKey(d => d.ActivityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN19110_ADM01100");

                entity.HasOne(d => d.TRN19100)
                    .WithMany(p => p.TRN19110)
                    .HasForeignKey(d => d.SInvoiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN19110_TRN19100");
            });

            modelBuilder.Entity<TRN23100>(entity =>
            {
                entity.HasKey(e => e.Workid)
                    .HasName("PRIMARY");

                entity.ToTable("trn23100");

                entity.HasOne(d => d.ADM01400)
                    .WithMany(p => p.TRN23100)
                    .HasForeignKey(d => d.AreaID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN23100_ADM01400");

                entity.HasOne(d => d.ADM03500)
                    .WithMany(p => p.TRN23100)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN23100_ADM03500");

                entity.HasOne(d => d.Users)
                    .WithMany(p => p.TRN23100)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN23100_Users");

                entity.HasOne(d => d.ADM03200)
                    .WithMany(p => p.TRN23100)
                    .HasForeignKey(d => d.Wo_client)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN23100_ADM03200");
            });

            modelBuilder.Entity<TRN23110>(entity =>
            {
                entity.HasKey(e => e.RecID)
                    .HasName("PRIMARY");

                entity.ToTable("trn23110");

                entity.HasOne(d => d.ADM01100)
                    .WithMany(p => p.TRN23110)
                    .HasForeignKey(d => d.ActivityID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN23110_ADM01100");

                entity.HasOne(d => d.TRN23100)
                    .WithMany(p => p.TRN23110)
                    .HasForeignKey(d => d.WorkOID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN23110_TRN23100");
            });

            modelBuilder.Entity<TRN23120>(entity =>
            {
                entity.HasKey(e => e.WoMatRecID)
                    .HasName("PRIMARY");

                entity.ToTable("trn23120");

                entity.HasOne(d => d.TRN23110)
                    .WithMany(p => p.TRN23120)
                    .HasForeignKey(d => d.WoActID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TRN23110_TRN23120");
            });

            modelBuilder.Entity<RevenueVsIncomeChartData>().HasNoKey();
            modelBuilder.Entity<TechnicianStatementDetail>().HasNoKey();
            modelBuilder.Entity<ContractorsStatement>().HasNoKey();
            modelBuilder.Entity<ContractorsStatementYTD>().HasNoKey();
            modelBuilder.Entity<PaymentDetailStatement>().HasNoKey();
            modelBuilder.Entity<CategoryInvoiceStatement>().HasNoKey();
            modelBuilder.Entity<CategoryInvoiceStatementInvoiceSummary>().HasNoKey();
            modelBuilder.Entity<DispatchWOStatementReport>().HasNoKey();
            modelBuilder.Entity<ContractorBankPaymentsReport>().HasNoKey();
            modelBuilder.Entity<POTracking>().HasNoKey();
            modelBuilder.Entity<POSummary>().HasNoKey();
            modelBuilder.Entity<MaterialUsage>().HasNoKey();
            modelBuilder.Entity<MaterialUsageDetail>().HasNoKey();
            modelBuilder.Entity<InvoiceSummaryData>().HasNoKey();

            OnModelCreatingPartial(modelBuilder);
        }
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}