namespace ECommerce.Catalog.Extensions;

public static class WebBuilderExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddDatabase()
        {
            string connectionString = builder.Configuration.GetConnectionString("Main")
                ?? throw new InvalidOperationException("Missing connection string.");

            builder.Services.AddSingleton<AuditInterceptor>();
            builder.Services.AddSingleton<OutboxInterceptor>();

            builder.Services.AddDbContext<MainDbContext>((serviceProvider, options) =>
            {
                AuditInterceptor auditInterceptor = serviceProvider.GetRequiredService<AuditInterceptor>();
                OutboxInterceptor outboxInterceptor = serviceProvider.GetRequiredService<OutboxInterceptor>();

                options.UseNpgsql(connectionString)
                   .AddInterceptors(auditInterceptor, outboxInterceptor);
            });

            return builder;
        }

        public WebApplicationBuilder AddOpenTelemetry()
        {
            builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("CatalogService"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddNpgsql()
                .AddSource("ECommerce.Catalog.Kafka")
                .AddConsoleExporter()
                .AddOtlpExporter())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddConsoleExporter()
                .AddOtlpExporter());

            return builder;
        }

        public WebApplicationBuilder AddKafkaAvroSchema()
        {
            KafkaSettings kafkaSettings = new();
            builder.Configuration.GetSection("KafkaSettings").Bind(kafkaSettings);

            string bootstrapServers = string.IsNullOrWhiteSpace(kafkaSettings.Server) ? "localhost:9092" : kafkaSettings.Server;
            string schemaRegistryUrl = string.IsNullOrWhiteSpace(kafkaSettings.SchemaRegistryUrl) ? "http://localhost:8081" : kafkaSettings.SchemaRegistryUrl;

            builder.Services.AddSingleton(sp =>
            {
                ISchemaRegistryClient schemaRegistry = sp.GetRequiredService<ISchemaRegistryClient>();
                ProducerConfig config = new() { BootstrapServers = bootstrapServers };

                if (!string.IsNullOrWhiteSpace(kafkaSettings.Username))
                {
                    config.SecurityProtocol = SecurityProtocol.SaslSsl;
                    config.SaslMechanism = SaslMechanism.Plain;
                    config.SaslUsername = kafkaSettings.Username;
                    config.SaslPassword = kafkaSettings.Password;
                }

                AvroSerializerConfig avroSerializerConfig = new()
                {
                    SubjectNameStrategy = SubjectNameStrategy.Record
                };

                return new ProducerBuilder<string, ISpecificRecord>(config)
                    .SetValueSerializer(new AvroSerializer<ISpecificRecord>(schemaRegistry, avroSerializerConfig))
                    .Build();
            });

            builder.Services.AddSingleton(sp =>
            {
                ISchemaRegistryClient schemaRegistry = sp.GetRequiredService<ISchemaRegistryClient>();
                ConsumerConfig config = new()
                {
                    BootstrapServers = bootstrapServers,
                    GroupId = "catalog-service-group",
                    AutoOffsetReset = AutoOffsetReset.Earliest,
                    EnableAutoCommit = false
                };

                if (!string.IsNullOrWhiteSpace(kafkaSettings.Username))
                {
                    config.SecurityProtocol = SecurityProtocol.SaslSsl;
                    config.SaslMechanism = SaslMechanism.Plain;
                    config.SaslUsername = kafkaSettings.Username;
                    config.SaslPassword = kafkaSettings.Password;
                }

                return new ConsumerBuilder<Ignore, ISpecificRecord>(config)
                    .SetValueDeserializer(new AvroDeserializer<ISpecificRecord>(schemaRegistry).AsSyncOverAsync())
                    .Build();
            });

            builder.Services.AddSingleton<ISchemaRegistryClient>(sp =>
            {
                SchemaRegistryConfig config = new() { Url = schemaRegistryUrl };

                if (!string.IsNullOrWhiteSpace(kafkaSettings.Username))
                {
                    config.BasicAuthUserInfo = $"{kafkaSettings.Username}:{kafkaSettings.Password}";
                }

                return new CachedSchemaRegistryClient(config);
            });

            return builder;
        }

        public WebApplicationBuilder AddCache()
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("Redis");
            });

            builder.Services.AddHybridCache();

            return builder;
        }

        public WebApplicationBuilder MapSettings()
        {
            builder.Services.Configure<KafkaSettings>(builder.Configuration.GetSection("KafkaSettings"));
            return builder;
        }

        public WebApplicationBuilder ConfigureRouting()
        {
            builder.Services.AddOpenApi();

            builder.Services.Configure<RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });

            return builder;
        }

        public WebApplicationBuilder ConfigureSerilog()
        {
            builder.Services.AddSerilog();
            builder.Services.AddSingleton(Log.Logger);

            return builder;
        }

        public WebApplicationBuilder ConfigureAuth()
        {
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();

            return builder;
        }

        public WebApplicationBuilder RegisterApplicationServices()
        {
            // Messaging Abstractions
            builder.Services.AddSingleton<IMessageProducer, KafkaMessageProducer>();
            builder.Services.AddSingleton<IMessageConsumer, KafkaMessageConsumer>();

            // Repositories
            builder.Services.AddScoped<IProductsRepository, ProductsRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IVariantAttributeRepository, VariantAttributeRepository>();

            // Services
            builder.Services.AddKeyedScoped<IProductService, ProductService>(KeyedServices.ProductService);
            builder.Services.AddKeyedScoped<IProductService, CachedProductService>(KeyedServices.CachedProductService);

            builder.Services.AddKeyedScoped<ICategoryService, CategoryService>(KeyedServices.CategoryService);
            builder.Services.AddKeyedScoped<ICategoryService, CachedCategoryService>(KeyedServices.CachedCategoryService);

            builder.Services.AddKeyedScoped<IVariantAttributeService, VariantAttributeService>(KeyedServices.AttributeService);
            builder.Services.AddKeyedScoped<IVariantAttributeService, CachedVariantAttributeService>(KeyedServices.CachedAttributeService);

            // Background Services
            builder.Services.AddHostedService<OutboxPublisherService>();
            builder.Services.AddHostedService<InventoryConsumerService>();

            return builder;
        }
    }
}
