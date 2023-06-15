using AutoMapper;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;
using CatsShelter.Service.Features.Adoption.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddAutoMapper(typeof(Program));  // Register AutoMapper


builder.Services.AddScoped<IMapper, Mapper>();
builder.Services.AddScoped<ICatsRepository, CatsRepository>();
builder.Services.AddScoped<ICatsAdoptionService, CatsAdoptionService>();

var app = builder.Build();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGrpcService<CatsAdoptionGrpcService>();

app.Run();