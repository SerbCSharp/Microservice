using Catalog.API.EventBus;
using Catalog.API.EventBus.RabbitMQ;
using Catalog.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<CatalogContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("CatalogDB")));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IEventBus, RabbitMQBus>();
var rabbitMqConfiguration = builder.Configuration.GetSection(RabbitMqConfiguration.Section);
builder.Services.Configure<RabbitMqConfiguration>(rabbitMqConfiguration);
builder.Services.AddHostedService<ConsumerHostedService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthorization();
app.MapControllers();
app.Run();
