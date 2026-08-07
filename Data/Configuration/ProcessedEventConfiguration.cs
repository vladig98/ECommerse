namespace ECommerce.Catalog.Data.Configuration;

public class ProcessedEventConfiguration : BaseModelConfiguration<ProcessedEvent>
{
    public override void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProcessedEvents");
    }
}
