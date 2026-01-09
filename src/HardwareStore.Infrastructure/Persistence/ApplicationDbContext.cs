using Microsoft.EntityFrameworkCore;
using HardwareStore.Domain.Entities;
using HardwareStore.Domain.Enums;

namespace HardwareStore.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // CONFIGURACIÓN DE USER
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20).HasDefaultValue("Customer");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.IsGoogleUser).HasDefaultValue(false);
                entity.Property(e => e.GoogleId).HasMaxLength(100);
                entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            });

            // CONFIGURACIÓN DE CATEGORY
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // CONFIGURACIÓN DE PRODUCT
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Stock).HasDefaultValue(0);
                entity.Property(e => e.Brand).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.Platform).HasMaxLength(50);
                entity.Property(e => e.MainImageUrl).HasMaxLength(500);
                entity.Property(e => e.Status)
                    .HasConversion<int>()
                    .HasDefaultValue(ProductStatus.Active);

                entity.Property(e => e.DeletedAt).IsRequired(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CategoryId);

                // Relación con Category
                entity.HasOne(e => e.Category)
                      .WithMany()
                      .HasForeignKey(e => e.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CONFIGURACIÓN DE PRODUCTIMAGE
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.DisplayOrder).HasDefaultValue(0);
                entity.Property(e => e.IsMain).HasDefaultValue(false);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

                // Relación con Product
                entity.HasOne(e => e.Product)
                      .WithMany(p => p.Images)
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // CONFIGURACIÓN DE SHIPPINGADDRESS 
            modelBuilder.Entity<ShippingAddress>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Información de domicilio
                entity.Property(e => e.AddressType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Street).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Number).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Floor).HasMaxLength(10);
                entity.Property(e => e.Apartment).HasMaxLength(10);
                entity.Property(e => e.Tower).HasMaxLength(50);
                entity.Property(e => e.BetweenStreets).HasMaxLength(200);
                entity.Property(e => e.PostalCode).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Province).IsRequired().HasMaxLength(100);
                entity.Property(e => e.City).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Observations).HasMaxLength(500);

                // Configuración
                entity.Property(e => e.IsActive).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                // Relación con User
                entity.HasOne(e => e.User)
                      .WithMany(u => u.ShippingAddresses)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índice para optimizar consultas
                entity.HasIndex(e => e.UserId);
            });

            // CONFIGURACIÓN DE ORDER
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);

                // Información del cliente
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(100);

                // Dirección de envío (copiada para historial)
                entity.Property(e => e.ShippingAddressType).HasMaxLength(50);
                entity.Property(e => e.ShippingStreet).HasMaxLength(200);
                entity.Property(e => e.ShippingNumber).HasMaxLength(10);
                entity.Property(e => e.ShippingFloor).HasMaxLength(10);
                entity.Property(e => e.ShippingApartment).HasMaxLength(10);
                entity.Property(e => e.ShippingTower).HasMaxLength(50);
                entity.Property(e => e.ShippingBetweenStreets).HasMaxLength(200);
                entity.Property(e => e.ShippingPostalCode).HasMaxLength(20);
                entity.Property(e => e.ShippingProvince).HasMaxLength(100);
                entity.Property(e => e.ShippingCity).HasMaxLength(100);
                entity.Property(e => e.ShippingObservations).HasMaxLength(500);

                // Persona autorizada (copiada para historial)
                entity.Property(e => e.AuthorizedPersonFirstName).HasMaxLength(100);
                entity.Property(e => e.AuthorizedPersonLastName).HasMaxLength(100);
                entity.Property(e => e.AuthorizedPersonPhone).HasMaxLength(20);
                entity.Property(e => e.AuthorizedPersonDni).HasMaxLength(20);

                // Información de la orden
                entity.Property(e => e.Total).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Status).IsRequired().HasMaxLength(30).HasDefaultValue("pending_payment");
                entity.Property(e => e.PaymentMethod).HasMaxLength(50).HasDefaultValue("bank_transfer");
                entity.Property(e => e.PaymentReceiptUrl).HasMaxLength(500);
                entity.Property(e => e.AdminNotes).HasMaxLength(1000);
                entity.Property(e => e.TrackingNumber).HasMaxLength(100);
                entity.Property(e => e.ShippingProvider).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

                // Relación opcional con User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Relación opcional con ShippingAddress
                entity.HasOne(e => e.ShippingAddress)
                      .WithMany()
                      .HasForeignKey(e => e.ShippingAddressId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Índices para optimizar consultas
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // CONFIGURACIÓN DE ORDERITEM
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.UnitPrice).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.Subtotal).IsRequired().HasColumnType("decimal(10,2)");
                entity.Property(e => e.ProductName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ProductImageUrl).HasMaxLength(500);
                entity.Property(e => e.ProductBrand).HasMaxLength(100); 
                entity.Property(e => e.ProductModel).HasMaxLength(100); 

                // Relación con Order
                entity.HasOne(e => e.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(e => e.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relación con Product
                entity.HasOne(e => e.Product)
                      .WithMany()
                      .HasForeignKey(e => e.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // CONFIGURACIÓN DE PASSWORDRESETTOKEN
            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
                entity.Property(e => e.ExpiresAt).IsRequired();
                entity.Property(e => e.IsUsed).HasDefaultValue(false);

                // Relación con User
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Índice para optimizar búsquedas por token
                entity.HasIndex(e => e.Token);

                // Índice para limpiar tokens expirados
                entity.HasIndex(e => e.ExpiresAt);
            });
        }
    }
}