using Autofac;
using Autofac.Extensions.DependencyInjection;
using Bazaar.Api;
using Extensions.Http.Mvc;

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

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.AddCommandQueryInternal();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bazaar API v1"));

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
