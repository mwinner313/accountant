using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bazaar.Api;
using Bazaar.Identity;
using Bazaar.Identity.Data;
using Bazaar.Identity.Grants;
using Extensions.Http.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

ServiceExtensions.Init(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContextInternal();
builder.Services.AddSwagger();
builder.Services.AddAuthInternal();
builder.Services.AddExecutionContext();

var identityUrl = builder.Configuration["Identity:Url"] ?? "http://localhost:5108";

builder.Services.AddIdentityServer(options =>
    {
        options.IssuerUri = identityUrl;
        options.Events.RaiseErrorEvents = true;
        options.Events.RaiseFailureEvents = true;
    })
    .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
    .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
    .AddInMemoryApiResources(IdentityConfig.ApiResources)
    .AddInMemoryClients(IdentityConfig.Clients)
    .AddExtensionGrantValidator<OtpGrantValidator>()
    .AddDeveloperSigningCredential();

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.AddCommandQueryInternal();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<IdentityDbContext>().Database.EnsureCreated();
    scope.ServiceProvider.GetRequiredService<Bazaar.Data.BazaarDbContext>().Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bazaar API v1"));

app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
