namespace ECommerce.Catalog.Data.Configuration;

public class ProductVariantAttributeConfiguration : IEntityTypeConfiguration<ProductVariantAttributeModel>
{
    // Mapping table
    public void Configure(EntityTypeBuilder<ProductVariantAttributeModel> builder)
    {
        builder.ToTable("ProductVariantAttributes");

        builder.HasKey(pva => new { pva.VariantId, pva.AttributeId });

        builder.HasOne(pva => pva.Variant)
            .WithMany(v => v.VariantAttributes)
            .HasForeignKey(pva => pva.VariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pva => pva.Attribute)
            .WithMany(a => a.ProductVariants)
            .HasForeignKey(pva => pva.AttributeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
