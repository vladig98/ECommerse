namespace ECommerce.Catalog.Data.Configuration;

public class DeadLetterMessageConfiguration : BaseModelConfiguration<DeadLetterMessage>
{
    public override void Configure(EntityTypeBuilder<DeadLetterMessage> builder)
    {
        base.Configure(builder);

        builder.Property(e => e.Source)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(255);

        builder.Property(e => e.ErrorReason)
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnType("jsonb")
            .IsRequired();
    }
}
