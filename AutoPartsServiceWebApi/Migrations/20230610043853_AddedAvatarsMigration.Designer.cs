﻿// <auto-generated />
using System;
using AutoPartsServiceWebApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace AutoPartsServiceWebApi.Migrations
{
    [DbContext(typeof(AutoDbContext))]
    [Migration("20230610043853_AddedAvatarsMigration")]
    partial class AddedAvatarsMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Address", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<string>("City")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Region")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Street")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Car", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("BodyNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Color")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Make")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Model")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StateNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserCommonId")
                        .HasColumnType("int");

                    b.Property<string>("VinNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("UserCommonId");

                    b.ToTable("Cars");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Device", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("DeviceId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("UserBusinessId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<int?>("UserCommonId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserBusinessId");

                    b.HasIndex("UserCommonId");

                    b.ToTable("Devices");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.DocumentUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CertificateNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("DocumentNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DocumentType")
                        .HasColumnType("int");

                    b.Property<string>("StateNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UinAccruals")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("UserCommonId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserCommonId");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.LoginSms", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<bool>("NewUser")
                        .HasColumnType("bit");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SmsCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("LoginSmses");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("AcceptedByUserId")
                        .HasColumnType("int");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<DateTime>("CreationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Header")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("UserCommonId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserCommonId")
                        .IsUnique();

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Review", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.Property<int>("UserBusinessId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserBusinessId");

                    b.ToTable("Reviews");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Service", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("ServiceType")
                        .HasColumnType("int");

                    b.Property<int>("UserBusinessId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserBusinessId");

                    b.ToTable("Services");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Sms", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("UserBusinessId")
                        .HasColumnType("int");

                    b.Property<int>("UserCommonId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UserBusinessId");

                    b.HasIndex("UserCommonId");

                    b.ToTable("Smses");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.UserBusiness", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Phone")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Rating")
                        .HasColumnType("int");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("UserBusinesses");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.UserCommon", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.ToTable("UserCommons");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Address", b =>
                {
                    b.HasOne("AutoPartsServiceWebApi.Models.UserCommon", null)
                        .WithOne("Address")
                        .HasForeignKey("AutoPartsServiceWebApi.Models.Address", "Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Car", b =>
                {
                    b.HasOne("AutoPartsServiceWebApi.Models.UserCommon", "UserCommon")
                        .WithMany("Cars")
                        .HasForeignKey("UserCommonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserCommon");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Device", b =>
                {
                    b.HasOne("AutoPartsServiceWebApi.Models.UserBusiness", "UserBusiness")
                        .WithMany("Devices")
                        .HasForeignKey("UserBusinessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AutoPartsServiceWebApi.Models.UserCommon", "UserCommon")
                        .WithMany("Devices")
                        .HasForeignKey("UserCommonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserBusiness");

                    b.Navigation("UserCommon");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.DocumentUser", b =>
                {
                    b.HasOne("AutoPartsServiceWebApi.Models.UserCommon", "UserCommon")
                        .WithMany("Documents")
                        .HasForeignKey("UserCommonId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("UserCommon");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Request", b =>
                {
                    b.HasOne("AutoPartsServiceWebApi.Models.UserCommon", "UserCommon")
                        .WithOne("Request")
                        .HasForeignKey("AutoPartsServiceWebApi.Models.Request", "UserCommonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserCommon");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Review", b =>
                {
                    b.HasOne("AutoPartsServiceWebApi.Models.UserBusiness", "UserBusiness")
                        .WithMany("Reviews")
                        .HasForeignKey("UserBusinessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserBusiness");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Service", b =>
                {
                    b.HasOne("AutoPartsServiceWebApi.Models.UserBusiness", "UserBusiness")
                        .WithMany("Services")
                        .HasForeignKey("UserBusinessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserBusiness");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.Sms", b =>
                {
                    b.HasOne("AutoPartsServiceWebApi.Models.UserBusiness", "UserBusiness")
                        .WithMany("SmsList")
                        .HasForeignKey("UserBusinessId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("AutoPartsServiceWebApi.Models.UserCommon", "UserCommon")
                        .WithMany("SmsList")
                        .HasForeignKey("UserCommonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UserBusiness");

                    b.Navigation("UserCommon");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.UserBusiness", b =>
                {
                    b.Navigation("Devices");

                    b.Navigation("Reviews");

                    b.Navigation("Services");

                    b.Navigation("SmsList");
                });

            modelBuilder.Entity("AutoPartsServiceWebApi.Models.UserCommon", b =>
                {
                    b.Navigation("Address")
                        .IsRequired();

                    b.Navigation("Cars");

                    b.Navigation("Devices");

                    b.Navigation("Documents");

                    b.Navigation("Request")
                        .IsRequired();

                    b.Navigation("SmsList");
                });
#pragma warning restore 612, 618
        }
    }
}
