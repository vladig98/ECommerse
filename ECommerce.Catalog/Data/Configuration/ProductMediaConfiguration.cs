namespace ECommerce.Catalog.Data.Configuration;

public class ProductMediaConfiguration : BaseModelConfiguration<ProductMedia>
{
    public override void Configure(EntityTypeBuilder<ProductMedia> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductMedia");

        builder.Property(pm => pm.Url)
            .HasMaxLength(2048)
            .IsRequired();

        builder.Property(pm => pm.AltText)
            .HasMaxLength(255);

        builder.Property(pm => pm.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(pm => pm.DisplayOrder)
            .HasDefaultValue(0);

        builder.Property(pm => pm.IsPrimary)
            .HasDefaultValue(false);

        // Optional Variant association (for variant-specific photos)
        builder.HasOne(pm => pm.ProductVariant)
            .WithMany()
            .HasForeignKey(pm => pm.ProductVariantId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
