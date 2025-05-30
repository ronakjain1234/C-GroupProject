﻿// <auto-generated />
using System;
using DatabaseHandler;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DatabaseHandler.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250308085512_added companyRoles table")]
    partial class addedcompanyRolestable
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("DatabaseHandler.Data.Models.Database.Company", b =>
                {
                    b.Property<int>("CompanyID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CompanyName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LastChange")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.HasKey("CompanyID");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("DatabaseHandler.Data.Models.Database.EndPoint", b =>
                {
                    b.Property<int>("EndPointID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastChange")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("EndPointID");

                    b.ToTable("EndPoints");
                });

            modelBuilder.Entity("DatabaseHandler.Data.Models.Database.LookupTables.CompanyEndPoint", b =>
                {
                    b.Property<int>("CompanyID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EndPointID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastChange")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.HasKey("CompanyID", "EndPointID");

                    b.ToTable("CompanyEndPoints");
                });

            modelBuilder.Entity("DatabaseHandler.Data.Models.Database.LookupTables.CompanyRoles", b =>
                {
                    b.Property<int>("CompanyID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RoleID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastChange")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.HasKey("CompanyID", "RoleID");

                    b.ToTable("CompanyRoles");
                });

            modelBuilder.Entity("DatabaseHandler.Data.Models.Database.LookupTables.CompanyUser", b =>
                {
                    b.Property<int>("CompanyID")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UserID")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastChange")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.HasKey("CompanyID", "UserID");

                    b.ToTable("CompanyUsers");
                });

            modelBuilder.Entity("DatabaseHandler.Data.Models.Database.Module", b =>
                {
                    b.Property<int>("ModuleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ModuleID");

                    b.ToTable("Modules");
                });

            modelBuilder.Entity("DatabaseHandler.Data.Models.Database.Role", b =>
                {
                    b.Property<int>("RoleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastChange")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("RoleID");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("DatabaseHandler.Data.Models.Database.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastChange")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("UserID");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}
