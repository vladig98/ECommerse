namespace ECommerce.Catalog.Data.Configuration;

public class ProductVariantConfiguration : BaseModelConfiguration<ProductVariant>
{
    public override void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductVariants");

        builder.Property(pv => pv.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(pv => pv.Sku)
            .IsUnique();

        builder.Property(pv => pv.BasePrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(pv => pv.Gtin)
            .HasMaxLength(14);

        builder.HasIndex(pv => pv.Gtin)
            .IsUnique()
            .HasFilter("\"Gtin\" IS NOT NULL");
    }
}
