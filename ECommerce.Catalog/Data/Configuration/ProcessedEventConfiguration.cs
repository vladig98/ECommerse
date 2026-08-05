namespace ECommerce.Catalog.Data.Configuration;

internal class ProcessedEventConfiguration : BaseModelConfiguration<ProcessedEvent>
{
    public override void Configure(EntityTypeBuilder<ProcessedEvent> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProcessedEvents");
    }
}
