using AutoMapper;
using CatsShelter.Service.Features.Adoption.Infrastructure.Repositories;
using CatsShelter.Service.Features.Adoption.Infrastructure;
using CatsShelter.Service.Features.Adoption.Services;
using MongoDB.Driver;

namespace CatsShelter.Service;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddGrpc();
        services.AddAutoMapper(typeof(Program));

        var mongoClient = new MongoClient(Configuration["MongoDbConnection"]);
        services.AddSingleton<IMongoClient>(mongoClient);

        services.AddScoped<IMapper, Mapper>();
        services.AddScoped<ICatsRepository, CatsRepository>();
        services.AddScoped<ICatsAdoptionService, CatsAdoptionService>();

        services.AddScoped<ICatsDatabaseContext>(sp =>
            new CatsDatabaseContext(mongoClient, Configuration["DatabaseName"]!, Configuration["CollectionName"]!));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
            endpoints.MapGrpcService<CatsAdoptionGrpcService>();
        });
    }
}