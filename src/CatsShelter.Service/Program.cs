using AutoMapper;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;
using CatsShelter.Service.Features.Adoption.Services;
using MongoDB.Driver;
using CatsShelter.Service.Features.Adoption.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddAutoMapper(typeof(Program));

// Adding MongoDB client
var mongoClient = new MongoClient(builder.Configuration["MongoDbConnection"]);
builder.Services.AddSingleton<IMongoClient>(mongoClient);

builder.Services.AddScoped<IMapper, Mapper>();
builder.Services.AddScoped<ICatsRepository, CatsRepository>();
builder.Services.AddScoped<ICatsAdoptionService, CatsAdoptionService>();

// Adding CatsDatabaseContext
builder.Services.AddScoped<ICatsDatabaseContext>(sp => 
    new CatsDatabaseContext(mongoClient, builder.Configuration["DatabaseName"]!, builder.Configuration["CollectionName"]!));

var app = builder.Build();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGrpcService<CatsAdoptionGrpcService>();

// Database seeding
var context = app.Services.GetService<ICatsDatabaseContext>();
var cancellationTokenSource = new CancellationTokenSource(builder.Configuration.GetValue<int>("MaxTimeoutDbSeedMilliseconds"));
await context!.SeedDatabase(builder.Configuration.GetValue<int>("CatsToSeed"), cancellationTokenSource.Token);

app.Run();