﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TeliconLatest.DataEntities;

namespace TeliconLatest.Migrations
{
    [DbContext(typeof(TeliconDbContext))]
    partial class TeliconDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 64)
                .HasAnnotation("ProductVersion", "5.0.7");

            modelBuilder.Entity("TeliconLatest.DataEntities.Customer", b =>
                {
                    b.Property<int>("CustomerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("AccountNo")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime");

                    b.Property<string>("CustAddress")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<string>("CustComment")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<DateTime>("CustDate")
                        .HasColumnType("datetime");

                    b.Property<string>("CustSignature")
                        .HasColumnType("text");

                    b.Property<bool>("IsSIClick")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsSITalk")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsSIWatch")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsWTAddOutlet")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsWTExisting")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsWTNew")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsWTNotDone")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsWTRewireCable")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsWTServiceCall")
                        .HasColumnType("tinyint(1)");

                    b.Property<bool>("IsWTServiceRepOrd")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("JobDate")
                        .HasColumnType("datetime");

                    b.Property<string>("Name")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("ServiceOrdNo")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("TechDate")
                        .HasColumnType("datetime");

                    b.Property<string>("TechId")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("TechName")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("TechSignature")
                        .HasColumnType("text");

                    b.Property<TimeSpan>("TimeIn")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("TimeOut")
                        .HasColumnType("time");

                    b.HasKey("CustomerId");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("TeliconLatest.DataEntities.CustomerEquipments", b =>
                {
                    b.Property<int>("CustomerEquipmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedDate")
                        .HasColumnType("datetime");

                    b.Property<int>("CustomerId")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("MacDetail")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("SerialNo")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("CustomerEquipmentId");

                    b.HasIndex("CustomerId");

                    b.ToTable("CustomerEquipments");
                });

            modelBuilder.Entity("TeliconLatest.DataEntities.CustomerEquipments", b =>
                {
                    b.HasOne("TeliconLatest.DataEntities.Customer", "Customer")
                        .WithMany("CustomerEquipments")
                        .HasForeignKey("CustomerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Customer");
                });

            modelBuilder.Entity("TeliconLatest.DataEntities.Customer", b =>
                {
                    b.Navigation("CustomerEquipments");
                });
#pragma warning restore 612, 618
        }
    }
}
