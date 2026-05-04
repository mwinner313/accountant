using Autofac;
using Bazaar.Data;
using Bazaar.Handlers.Commands.Shops.Create;
using Infra;
using Infra.Common.Decorators;
using Infra.EFCore;
using Infra.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

namespace Bazaar.Api;

public static class ServiceExtensions
{
    private static IConfiguration _configuration = default!;

    public static void Init(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public static void AddDbContextInternal(this IServiceCollection services)
    {
        var connectionString = _configuration.GetConnectionString("SqlServer");
        Guard.NotNullOrEmpty(connectionString, nameof(connectionString));

        services.AddDbContextPool<BazaarDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<DbContext, BazaarDbContext>();
    }

    public static void AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Bazaar API", Version = "v1" });
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
            c.CustomSchemaIds(type => type.ToString());
        });
    }

    public static void AddAuthInternal(this IServiceCollection services)
    {
        var identityUrl = _configuration["Identity:Url"];
        Guard.NotNullOrEmpty(identityUrl, "Identity:Url");

        services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.Authority = identityUrl;
                options.RequireHttpsMetadata = false;
                options.Audience = "bazaar_api";
            });

        services.AddAuthorization();
    }

    public static ContainerBuilder AddCommandQueryInternal(this ContainerBuilder builder)
    {
        var scannedAssemblies = new[]
        {
            typeof(CreateShopCommand).Assembly
        };

        builder.Register<IUnitOfWork>(context =>
            {
                var db = context.Resolve<DbContext>();
                var logger = context.Resolve<ILogger<EfUnitOfWork>>();
                var syncEventBus = context.Resolve<SyncEventBus>();
                return new EfUnitOfWork(db, syncEventBus, logger);
            })
            .InstancePerLifetimeScope();

        builder.AddCommandQuery(scannedAssemblies: scannedAssemblies);
        builder.AddSyncEventHandlers(scannedAssemblies);

        return builder;
    }
}
