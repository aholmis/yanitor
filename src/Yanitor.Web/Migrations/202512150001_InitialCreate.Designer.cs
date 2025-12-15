using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Yanitor.Web.Migrations
{
    [DbContext(typeof(Yanitor.Web.Data.YanitorDbContext))]
    [Migration("202512150001_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("Yanitor.Web.Data.ActiveTaskRow", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<int>("IntervalDays")
                    .HasColumnType("INTEGER");

                b.Property<string>("ItemName")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.Property<DateTime?>("LastCompletedAt")
                    .HasColumnType("TEXT");

                b.Property<DateTime>("NextDueDate")
                    .HasColumnType("TEXT");

                b.Property<int>("RoomType")
                    .HasColumnType("INTEGER");

                b.Property<string>("TaskName")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.Property<string>("TaskType")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("NextDueDate");

                b.ToTable("ActiveTasks");
            });

            modelBuilder.Entity("Yanitor.Web.Data.House", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("TEXT");

                b.Property<Guid>("OwnerId")
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("OwnerId")
                    .IsUnique();

                b.ToTable("Houses");
            });

            modelBuilder.Entity("Yanitor.Web.Data.SelectedItemType", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<Guid>("HouseId")
                    .HasColumnType("TEXT");

                b.Property<string>("Type")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("HouseId", "Type")
                    .IsUnique();

                b.ToTable("SelectedItemTypes");
            });

            modelBuilder.Entity("Yanitor.Web.Data.User", b =>
            {
                b.Property<Guid>("Id")
                    .HasColumnType("TEXT");

                b.Property<Guid?>("HouseId")
                    .HasColumnType("TEXT");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnType("TEXT");

                b.HasKey("Id");

                b.HasIndex("Name")
                    .IsUnique();

                b.ToTable("Users");
            });

            modelBuilder.Entity("Yanitor.Web.Data.SelectedItemType", b =>
            {
                b.HasOne("Yanitor.Web.Data.House", null)
                    .WithMany("SelectedItemTypes")
                    .HasForeignKey("HouseId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();
            });

            modelBuilder.Entity("Yanitor.Web.Data.House", b =>
            {
                b.HasOne("Yanitor.Web.Data.User", "Owner")
                    .WithOne("House")
                    .HasForeignKey("Yanitor.Web.Data.House", "OwnerId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Owner");
            });

            modelBuilder.Entity("Yanitor.Web.Data.House", b =>
            {
                b.Navigation("SelectedItemTypes");
            });

            modelBuilder.Entity("Yanitor.Web.Data.User", b =>
            {
                b.Navigation("House");
            });
#pragma warning restore 612, 618
        }
    }
}
