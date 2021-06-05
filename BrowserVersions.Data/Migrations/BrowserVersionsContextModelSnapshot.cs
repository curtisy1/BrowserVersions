﻿// <auto-generated />
using System;
using BrowserVersions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BrowserVersions.Data.Migrations
{
    [DbContext(typeof(BrowserVersionsContext))]
    partial class BrowserVersionsContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.0-preview.4.21253.1");

            modelBuilder.Entity("BrowserVersion", b =>
                {
                    b.Property<int>("BrowsersBrowserId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("VersionsVersionId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BrowsersBrowserId", "VersionsVersionId");

                    b.HasIndex("VersionsVersionId");

                    b.ToTable("BrowserVersion");
                });

            modelBuilder.Entity("BrowserVersions.Data.Entities.Browser", b =>
                {
                    b.Property<int>("BrowserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Platform")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("BrowserId");

                    b.ToTable("Browsers");
                });

            modelBuilder.Entity("BrowserVersions.Data.Entities.Version", b =>
                {
                    b.Property<int>("VersionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime?>("EndOfSupportDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ReleaseDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("VersionCode")
                        .HasColumnType("TEXT");

                    b.HasKey("VersionId");

                    b.ToTable("Versions");
                });

            modelBuilder.Entity("BrowserVersion", b =>
                {
                    b.HasOne("BrowserVersions.Data.Entities.Browser", null)
                        .WithMany()
                        .HasForeignKey("BrowsersBrowserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BrowserVersions.Data.Entities.Version", null)
                        .WithMany()
                        .HasForeignKey("VersionsVersionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
