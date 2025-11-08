using FreelancerAssignment.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FreelancerAssignment.Presistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.ProductCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Image)
            .IsRequired()
            .HasMaxLength(500);


        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.MinimumQuantity)
            .IsRequired();

        builder.Property(p => p.Discount)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.HasIndex(p => p.ProductCode)
            .IsUnique();


        builder.HasOne(e => e.CreatedUser)
            .WithMany(e => e.CreatedProducts)
            .HasForeignKey(e => e.CreatedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Users)
            .WithMany(e => e.Products);
    }
}
