namespace ECommerce.Catalog.Data.Configuration;

internal class VariantAttributeConfiguration : BaseModelConfiguration<VariantAttributeModel>
{
    public override void Configure(EntityTypeBuilder<VariantAttributeModel> builder)
    {
        base.Configure(builder);

        builder.ToTable("VariantAttributes");

        builder.Property(va => va.Name)
            .HasColumnType("citext")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(va => va.Value)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(va => new { va.Name, va.Value })
            .IsUnique();
    }
}
