namespace ECommerce.Catalog.Data.Configuration;

public class EventMessageConfiguration : BaseModelConfiguration<EventMessage>
{
    public override void Configure(EntityTypeBuilder<EventMessage> builder)
    {
        base.Configure(builder);

        builder.ToTable("EventMessages");

        builder.Property(e => e.Key)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(e => e.Value)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(e => e.Key);
        builder.HasIndex(e => e.EventType);
    }
}
