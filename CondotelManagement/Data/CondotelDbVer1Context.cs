using System;
using System.Collections.Generic;
using CondotelManagement.Models;
using Microsoft.EntityFrameworkCore;
using HostModel = CondotelManagement.Models.Host;

namespace CondotelManagement.Data;

public partial class CondotelDbVer1Context : DbContext
{
    public CondotelDbVer1Context()
    {
    }

    public CondotelDbVer1Context(DbContextOptions<CondotelDbVer1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<AdminReport> AdminReports { get; set; }

    public virtual DbSet<Amenity> Amenities { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<BookingDetail> BookingDetails { get; set; }

    public virtual DbSet<Condotel> Condotels { get; set; }

    public virtual DbSet<CondotelAmenity> CondotelAmenities { get; set; }

    public virtual DbSet<CondotelDetail> CondotelDetails { get; set; }

    public virtual DbSet<CondotelImage> CondotelImages { get; set; }

    public virtual DbSet<CondotelPrice> CondotelPrices { get; set; }

    public virtual DbSet<CondotelUtility> CondotelUtilities { get; set; }

    public virtual DbSet<HostModel> Hosts { get; set; }

    public virtual DbSet<HostPackage> HostPackages { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }



    public virtual DbSet<Resort> Resorts { get; set; }

    public virtual DbSet<ResortUtility> ResortUtilities { get; set; }

    public virtual DbSet<Review> Reviews { get; set; }

    public virtual DbSet<RewardPoint> RewardPoints { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<ServicePackage> ServicePackages { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Utility> Utilities { get; set; }

    public virtual DbSet<Wallet> Wallets { get; set; }

    private string GetConnectionString()
    {
        IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true).Build();
        return configuration["ConnectionStrings:MyCnn"];
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(GetConnectionString());
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminReport>(entity =>
        {
            entity.HasKey(e => e.ReportId).HasName("PK__AdminRep__D5BD48E5DD896BC2");

            entity.ToTable("AdminReport");

            entity.Property(e => e.ReportId).HasColumnName("ReportID");
            entity.Property(e => e.AdminId).HasColumnName("AdminID");
            entity.Property(e => e.FileUrl)
                .HasMaxLength(255)
                .HasColumnName("FileURL");
            entity.Property(e => e.GeneratedDate)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ReportType).HasMaxLength(100);

            entity.HasOne(d => d.Admin).WithMany(p => p.AdminReports)
                .HasForeignKey(d => d.AdminId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AdminReport_User");
        });

        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.HasKey(e => e.AmenityId).HasName("PK__Amenitie__842AF52B7667116F");

            entity.Property(e => e.AmenityId).HasColumnName("AmenityID");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.BookingId).HasName("PK__Booking__73951ACDB87A75DE");

            entity.ToTable("Booking");

            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.Condotel).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CondotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_Condotel");

            entity.HasOne(d => d.Customer).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Booking_User");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PromotionId)
                .HasConstraintName("FK_Booking_Promotion");
        });

        modelBuilder.Entity<BookingDetail>(entity =>
        {
            entity.HasKey(e => e.BookingDetailId).HasName("PK__BookingD__8136D47A7F505BA4");

            entity.ToTable("BookingDetail");

            entity.Property(e => e.BookingDetailId).HasColumnName("BookingDetailID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Quantity).HasDefaultValue(1);
            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDetail_Booking");

            entity.HasOne(d => d.Service).WithMany(p => p.BookingDetails)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookingDetail_Service");
        });

        modelBuilder.Entity<Condotel>(entity =>
        {
            entity.HasKey(e => e.CondotelId).HasName("PK__Condotel__5137994D066CC8AE");

            entity.ToTable("Condotel");

            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.Bathrooms).HasDefaultValue(1);
            entity.Property(e => e.Beds).HasDefaultValue(1);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.HostId).HasColumnName("HostID");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.PricePerNight).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.ResortId).HasColumnName("ResortID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Host).WithMany(p => p.Condotels)
                .HasForeignKey(d => d.HostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Condotel_Host");

            entity.HasOne(d => d.Resort).WithMany(p => p.Condotels)
                .HasForeignKey(d => d.ResortId)
                .HasConstraintName("FK_Condotel_Resort");
        });

        modelBuilder.Entity<CondotelAmenity>(entity =>
        {
            entity.HasKey(e => new { e.CondotelId, e.AmenityId });

            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.AmenityId).HasColumnName("AmenityID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Amenity).WithMany(p => p.CondotelAmenities)
                .HasForeignKey(d => d.AmenityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondotelAmenities_Amenity");

            entity.HasOne(d => d.Condotel).WithMany(p => p.CondotelAmenities)
                .HasForeignKey(d => d.CondotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondotelAmenities_Condotel");
        });

        modelBuilder.Entity<CondotelDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__Condotel__135C314DC0149A17");

            entity.Property(e => e.DetailId).HasColumnName("DetailID");
            entity.Property(e => e.Bathrooms).HasDefaultValue((byte)1);
            entity.Property(e => e.Beds).HasDefaultValue((byte)1);
            entity.Property(e => e.BuildingName).HasMaxLength(150);
            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.HygieneStandards).HasMaxLength(500);
            entity.Property(e => e.RoomNumber).HasMaxLength(50);
            entity.Property(e => e.SafetyFeatures).HasMaxLength(500);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Condotel).WithMany(p => p.CondotelDetails)
                .HasForeignKey(d => d.CondotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondotelDetails_Condotel");
        });

        modelBuilder.Entity<CondotelImage>(entity =>
        {
            entity.HasKey(e => e.ImageId).HasName("PK__Condotel__7516F4EC2F1E6A9D");

            entity.ToTable("CondotelImage");

            entity.Property(e => e.ImageId).HasColumnName("ImageID");
            entity.Property(e => e.Caption).HasMaxLength(255);
            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(255)
                .HasColumnName("ImageURL");

            entity.HasOne(d => d.Condotel).WithMany(p => p.CondotelImages)
                .HasForeignKey(d => d.CondotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondotelImage_Condotel");
        });

        modelBuilder.Entity<CondotelPrice>(entity =>
        {
            entity.HasKey(e => e.PriceId).HasName("PK__Condotel__4957584FD528DBBF");

            entity.ToTable("CondotelPrice");

            entity.Property(e => e.PriceId).HasColumnName("PriceID");
            entity.Property(e => e.BasePrice).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PriceType)
                .HasMaxLength(50)
                .HasDefaultValue("Normal");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Condotel).WithMany(p => p.CondotelPrices)
                .HasForeignKey(d => d.CondotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondotelPrice_Condotel");
        });

        modelBuilder.Entity<CondotelUtility>(entity =>
        {
            entity.HasKey(e => new { e.CondotelId, e.UtilityId });

            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.UtilityId).HasColumnName("UtilityID");
            entity.Property(e => e.DateAdded).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Condotel).WithMany(p => p.CondotelUtilities)
                .HasForeignKey(d => d.CondotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondotelUtilities_Condotel");

            entity.HasOne(d => d.Utility).WithMany(p => p.CondotelUtilities)
                .HasForeignKey(d => d.UtilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CondotelUtilities_Utilities");
        });

        modelBuilder.Entity<HostModel>(entity =>
        {
            entity.HasKey(e => e.HostId).HasName("PK__Host__08D4870CF71CA63F");

            entity.ToTable("Host");

            entity.Property(e => e.HostId).HasColumnName("HostID");
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.PhoneContact).HasMaxLength(20);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.Hosts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Host_User");
        });

        modelBuilder.Entity<HostPackage>(entity =>
        {
            entity.HasKey(e => new { e.HostId, e.PackageId }).HasName("PK__HostPack__DBF68452C59B64C5");

            entity.ToTable("HostPackage");

            entity.Property(e => e.HostId).HasColumnName("HostID");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Host).WithMany(p => p.HostPackages)
                .HasForeignKey(d => d.HostId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HostPackage_Host");

            entity.HasOne(d => d.Package).WithMany(p => p.HostPackages)
                .HasForeignKey(d => d.PackageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_HostPackage_Package");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.LocationId).HasName("PK__Location__E7FEA477D86590DA");

            entity.ToTable("Location");

            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__Package__322035EC4A75639D");

            entity.ToTable("Package");

            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Duration).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.PromotionId).HasName("PK__Promotio__52C42F2FAD255FDB");

            entity.ToTable("Promotion");

            entity.Property(e => e.PromotionId).HasColumnName("PromotionID");
            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.DiscountPercentage).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.TargetAudience).HasMaxLength(100);

            entity.HasOne(d => d.Condotel).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.CondotelId)
                .HasConstraintName("FK_Promotion_Condotel");
        });

        modelBuilder.Entity<Resort>(entity =>
        {
            entity.HasKey(e => e.ResortId).HasName("PK__Resort__7D2D742E06FB88E2");

            entity.ToTable("Resort");

            entity.Property(e => e.ResortId).HasColumnName("ResortID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.LocationId).HasColumnName("LocationID");
            entity.Property(e => e.Name).HasMaxLength(150);

            entity.HasOne(d => d.Location).WithMany(p => p.Resorts)
                .HasForeignKey(d => d.LocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Resort_Location");
        });

        modelBuilder.Entity<ResortUtility>(entity =>
        {
            entity.HasKey(e => new { e.ResortId, e.UtilityId });

            entity.Property(e => e.ResortId).HasColumnName("ResortID");
            entity.Property(e => e.UtilityId).HasColumnName("UtilityID");
            entity.Property(e => e.Cost).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.DescriptionDetail).HasMaxLength(255);
            entity.Property(e => e.OperatingHours).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Resort).WithMany(p => p.ResortUtilities)
                .HasForeignKey(d => d.ResortId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResortUtilities_Resort");

            entity.HasOne(d => d.Utility).WithMany(p => p.ResortUtilities)
                .HasForeignKey(d => d.UtilityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ResortUtilities_Utility");
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__Review__74BC79AE7C3C93E9");

            entity.ToTable("Review");

            entity.Property(e => e.ReviewId).HasColumnName("ReviewID");
            entity.Property(e => e.Comment).HasMaxLength(500);
            entity.Property(e => e.CondotelId).HasColumnName("CondotelID");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Condotel).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.CondotelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_Condotel");

            entity.HasOne(d => d.User).WithMany(p => p.Reviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Review_User");
        });

        modelBuilder.Entity<RewardPoint>(entity =>
        {
            entity.HasKey(e => e.PointId).HasName("PK__RewardPo__40A97781A5ECB5B9");

            entity.Property(e => e.PointId).HasColumnName("PointID");
            entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
            entity.Property(e => e.LastUpdated)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.Customer).WithMany(p => p.RewardPoints)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RewardPoints_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3AA814A924");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B6160A4980567").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<ServicePackage>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("PK__ServiceP__C51BB0EAEC41FF93");

            entity.ToTable("ServicePackage");

            entity.Property(e => e.ServiceId).HasColumnName("ServiceID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC33739598");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D1053476481B2D").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FullName).HasMaxLength(150);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.PasswordHash).HasMaxLength(100);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        modelBuilder.Entity<Utility>(entity =>
        {
            entity.HasKey(e => e.UtilityId).HasName("PK__Utilitie__8B7E2E3F99BB3C25");

            entity.Property(e => e.UtilityId).HasColumnName("UtilityID");
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Wallet>(entity =>
        {
            entity.HasKey(e => e.WalletId).HasName("PK__Wallet__84D4F92E7D1E0C09");

            entity.ToTable("Wallet");

            entity.Property(e => e.WalletId).HasColumnName("WalletID");
            entity.Property(e => e.AccountHolderName).HasMaxLength(150);
            entity.Property(e => e.AccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.HostId).HasColumnName("HostID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Host).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.HostId)
                .HasConstraintName("FK_Wallet_Host");

            entity.HasOne(d => d.User).WithMany(p => p.Wallets)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Wallet_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
