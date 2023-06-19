# 🐱 CatsShelter Microservice 🐱

CatsShelter is a .NET 7 microservice that showcases the use of Test-Driven Development (TDD), Domain-Driven Design (DDD), gRPC, MongoDB, Docker, Error Handling, Dependency Injection, and more. The project is structured using a slice-by-feature approach.

## 📚 Project Overview

The CatsShelter microservice provides the following gRPC endpoints:

1. **GetAvailableCats**: Returns a list of all cats available for adoption.
2. **RequestAdoption**: Allows a user to request the adoption of a cat. Once a cat is requested for adoption, it is no longer available for others to adopt.
3. **CancelAdoption**: Allows a user to cancel a previous adoption request. The cat will then be available for adoption again.

## 🧪 TDD Approach

The Test-Driven Development (TDD) approach followed in this project is as follows:

1. **Domain Model Tests**: Start with the innermost layer of the application, which is the domain layer. In this case, it's the `Cat` class .
2. **Domain Model Implementation**: Implement the `Cat` class.
3. **CatsDatabaseContext Tests**: Write tests for the `CatsDatabaseContext` class. These tests ensure the correct interaction between the application and the database.
4. **CatsDatabaseContext Implementation**: Implement the `CatsDatabaseContext` class. This class interacts with the MongoDB database.
5. **Repository Tests**: Write tests for the `ICatRepository` interface. These tests mock the database context and check if the correct calls are being made on it.
6. **Repository Implementation**: Implement the `ICatRepository` interface.
7. **Service Tests**: Write tests for the `CatsAdoptionService`. These tests mock the `ICatRepository` and check if the correct methods are being called on it.
8. **Service Implementation**: Implement the `CatsAdoptionService`. This layer uses the repository to perform operations and enforce any business rules.
9. **gRPC Service Tests**: Write tests for the `CatsAdoptionGrpcService`. These tests mock the `CatsShelterService` and check if the correct methods are being called on it.
10. **gRPC Service Implementation**: Implement the `CatsAdoptionGrpcService` class. This layer uses the `CatsShelterService` to handle incoming gRPC requests and return the appropriate responses.

## 🏗️ Project Structure

The project follows a Domain-Driven Design (DDD) approach and is structured using the "slice by feature" approach. In this project, there is only one feature: Adoption. Each feature is organized into its own directory, which contains all the necessary components for that feature. This structure makes it easy to understand and manage the code related to each feature.

Here is the structure of the Adoption feature:

```
CatShelter.Service
└── Features
    └── Adoption
        ├── Domain
        │   ├── Entities
        │   │   └── Cat.cs
        │   └── Exceptions
        │       └── CatUnavailableException.cs
        ├── Infrastructure
        │   ├── Repositories
        │   │   ├── ICatsRepository.cs
        │   │   ├── CatsRepository.cs
        │   │   └── Exceptions
        │   │       ├── CatNotFoundException.cs
        │   │       └── CatUpdateException.cs
        ├── Proto
        │   └── cats-shelter-service.proto
        └── Services
            ├── CatAdoptionRequest.cs
            ├── CatAdoptionResponse.cs
            ├── CatsAdoptionGrpcService.cs
            ├── CatsAdoptionService.cs
            ├── FailCatAdoptionResponse.cs
            ├── ICatsAdoptionService.cs
            ├── SuccessCancelCatAdoptionResponse.cs
            ├── SuccessRequestCatAdoptionResponse.cs
            └── Mapping
                ├── CatProfile.cs
                ├── CatRequestProfile.cs
                └── CatResponseProfile.cs
```

1. **Domain Layer**: This layer contains the core business logic and entities of the feature. For example, the `Cat` class represents a cat that is available for adoption. It has methods to request and cancel adoptions, which change the `IsAvailable` property of the cat.

   ```csharp
   public class Cat
   {
       public string Id { get; private set; }
       public string Name { get; private set; }
       public bool IsAvailable { get; private set; }

       public Cat(string id, string name)
       {
           Id = id;
           Name = name;
           IsAvailable = true;
       }

       public void RequestAdoption()
       {
           if (!IsAvailable)
               throw new CatUnavailableException();

           IsAvailable = false;
       }

       public void CancelAdoption() => IsAvailable = true;
   }
   ```

2. **Infrastructure Layer**: This layer provides concrete implementations of the interfaces defined in the domain layer. It interacts with the MongoDB database and handles operations such as finding a cat by its ID and updating a cat's status.

   ```csharp
   public class CatsDatabaseContext : ICatsDatabaseContext
   {
       private readonly IMongoCollection<Cat> _cats;

       public CatsDatabaseContext(
           IMongoClient client,
           string databaseName,
           string collectionName
	   )
       {
           var database = client.GetDatabase(databaseName);
           _cats = database.GetCollection<Cat>(collectionName);
       }

       // Other methods...
   }
   ```

3. **Repositories**: Repositories are used to encapsulate the logic required to access data sources. They centralize common data access functionality, providing better maintainability and decoupling the infrastructure or technology used to access databases from the domain model layer.

   ```csharp
   public class CatsRepository : ICatsRepository
   {
       private readonly ICatsDatabaseContext _context;

       public CatsRepository(ICatsDatabaseContext context)
       {
           _context = context;
       }

       // Other methods...
   }
   ```

4. **Services**: The service layer in this architecture is responsible for executing business logic and interacting with the data repository. It is composed of services that encapsulate the business rules and operations of the application. Let's break down the two services to better understand the role of the service layer.

   a) **CatsAdoptionGrpcService**: This class is a gRPC service that handles incoming gRPC calls related to cat adoption. It uses the `ICatsAdoptionService` to perform operations related to cat adoption. It also uses an `IMapper` to convert between the protocol buffer message types and the domain types used in the service layer.

   ```csharp
   public class CatsAdoptionGrpcService : CatsShelterService.CatsShelterServiceBase
   {
       private readonly ICatsAdoptionService _catsAdoptionService;
       private readonly IMapper _mapper;

       public CatsAdoptionGrpcService(
           ICatsAdoptionService catsAdoptionService,
           IMapper mapper
       )
       {
           _catsAdoptionService = catsAdoptionService;
           _mapper = mapper;
       }

       public override async Task<AdoptionResponse> RequestAdoption(CatRequest request, ServerCallContext context)
       {
           var catRequestAdoptionRequest = _mapper.Map<CatAdoptionRequest>(request);

           var catRequestAdoptionResponse = await _catsAdoptionService.RequestAdoptionAsync(catRequestAdoptionRequest, context.CancellationToken);

           return _mapper.Map<AdoptionResponse>(catRequestAdoptionResponse);
       }

       // Other methods...
   }
   ```

      - `RequestAdoption` method: This method handles requests to adopt a cat. It maps the incoming `CatRequest` to a `CatAdoptionRequest`, then calls the `RequestAdoptionAsync` method on the `ICatsAdoptionService`. The response from the service is then mapped to an `AdoptionResponse` and returned.

      - `GetAvailableCats` method: This method retrieves the list of available cats for adoption. It calls the `GetAvailableCatsAsync` method on the `ICatsAdoptionService` and maps the returned list of cats to a `Cats` message type.

     - `CancelAdoption` method: This method handles requests to cancel a cat adoption. It maps the incoming `CatRequest` to a `CatAdoptionRequest`, then calls the `CancelAdoptionAsync` method on the `ICatsAdoptionService`. The response from the service is then mapped to an `AdoptionResponse` and returned.

   b) **CatsAdoptionService**: This class is a service that encapsulates the business logic related to cat adoption. It uses the `ICatsRepository` to interact with the data layer.

   ```csharp
   public class CatsAdoptionService : ICatsAdoptionService
   {
       private readonly ICatsRepository _catsRepository;

       public CatsAdoptionService(
           ICatsRepository catsRepository
       )
       {
           _catsRepository = catsRepository;
       }

       public async Task<CatAdoptionResponse> RequestAdoptionAsync(CatAdoptionRequest request, CancellationToken cancellationToken)
       {
           try
           {
               var cat = await _catsRepository.GetCatByIdAsync(request.CatId, cancellationToken);

               cat.RequestAdoption();

               await _catsRepository.UpdateCatAsync(cat, cancellationToken);
           }
           catch (Exception exception)
           {
               return new FailCatAdoptionResponse(exception);
           }

           return new SuccessRequestCatAdoptionResponse();
       }

       // Other methods...
   }
   ```

      - `GetAvailableCatsAsync` method: This method retrieves a list of cats that are available for adoption from the repository.

      - `RequestAdoptionAsync` method: This method handles requests to adopt a cat. It retrieves the cat from the repository, calls the `RequestAdoption` method on the cat (which is a domain operation), and then updates the cat in the repository. If any exception occurs during this process, it returns a `FailCatAdoptionResponse`; otherwise, it returns a `SuccessRequestCatAdoptionResponse`.

      - `CancelAdoptionAsync` method: This method handles requests to cancel a cat adoption. It retrieves the cat from the repository, calls the `CancelAdoption` method on the cat (which is a domain operation), and then updates the cat in the repository. If any exception occurs during this process, it returns a `FailCatAdoptionResponse`; otherwise, it returns a `SuccessCancelCatAdoptionResponse`.

5. **Proto**: This directory contains the Protobuf file that defines the gRPC service and the messages it uses.
The gRPC service for the cat shelter application is defined using Protocol Buffers (protobuf), a language-neutral, platform-neutral, extensible mechanism for serializing structured data. The protobuf file defines the structure of the data and the service interface for the gRPC service.

   Here's a brief explanation of the protobuf file:

   ```protobuf
   syntax = "proto3";

   option csharp_namespace = "CatsShelter.Service.Features.Adoption.Proto";

   service CatsShelterService {
     rpc GetAvailableCats (Empty) returns (Cats) {}

     rpc RequestAdoption (CatRequest) returns (AdoptionResponse) {}

     rpc CancelAdoption (CatRequest) returns (AdoptionResponse) {}
   }

   message CatRequest {
     string id = 1;
   }

   message Cats {
     repeated Cat cats = 1;
   }

   message AdoptionResponse {
     bool success = 1;
     string message = 2;
   }

   message Cat {
     string id = 1;
     string name = 2;
     bool isAvailable = 3;
   }

   message Empty {}
   ```

   - `syntax = "proto3";` - Specifies that we're using version 3 of the protobuf language.

   - `option csharp_namespace = "CatsShelter.Service.Features.Adoption.Proto";` - Specifies the namespace for the generated C# classes.

   - `service CatsShelterService {...}` - Defines a gRPC service with three methods: `GetAvailableCats`, `RequestAdoption`, and `CancelAdoption`. Each method has specified request and response types.

   - `message CatRequest {...}`, `message Cats {...}`, `message AdoptionResponse {...}`, `message Cat {...}`, and `message Empty {}` - Define the structure of the data that will be sent and received by the service. For example, the `CatRequest` message consists of a single string field `id`.

6. **Mapping**: This directory contains AutoMapper profiles, which are used to map between the domain entities and the Protobuf messages.

## Error Handling

The project uses custom exceptions to handle errors. For instance, if a user tries to adopt a cat that is not available, a `CatUnavailableException` is thrown. This exception is defined in the domain layer and is used to enforce the business rule that only available cats can be adopted.

```csharp
public class CatUnavailableException : Exception
{
    public CatUnavailableException()
        : base("Cat is not available for adoption.")
    {
    }
}
```

Similarly, if a user tries to update a cat that does not exist in the database, a `CatNotFoundException` is thrown. This exception is defined in the repository layer and is used to enforce the rule that only existing cats can be updated.

```csharp
public class CatNotFoundException : Exception
{
    public CatNotFoundException(string id) : base($"No cat with id {id} was found.")
    {
    }
}
```

## AutoMapper Usage

AutoMapper is used to map between the domain entities and the Protobuf messages. This is done using AutoMapper profiles. For instance, the `CatProfile` maps between the `Cat` domain entity and the `Cat` Protobuf message.

```csharp
public class CatProfile : Profile
{
    public CatProfile()
    {
        CreateMap<Domain.Entities.Cat, Proto.Cat>();
        CreateMap<Proto.Cat, Domain.Entities.Cat>();
    }
}
```

Similarly, the `CatRequestProfile` maps between the `CatAdoptionRequest` domain entity and the `CatRequest` Protobuf message.

```csharp
public class CatRequestProfile : Profile
{
    public CatRequestProfile()
    {
        CreateMap<CatAdoptionRequest, Proto.CatRequest>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.CatId));

        CreateMap<Proto.CatRequest, CatAdoptionRequest>()
            .ForMember(dest => dest.CatId, opt => opt.MapFrom(src => src.Id));
    }
}
```

These profiles are registered in the `Startup` class, and the `IMapper` interface is used to perform the actual mapping in the services and controllers.

## 🐳 Dockerization

The project includes a `Dockerfile` and a `docker-compose.yml` file. To build and run the Docker image, use the following commands:

```bash
docker-compose build
docker-compose up
```

### Dockerfile

The Dockerfile is a script that contains instructions on how to build a Docker image for the project. Here's a breakdown of the Dockerfile:

```Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["CatsShelter.Service/CatsShelter.Service.csproj", "CatsShelter.Service/"]
RUN dotnet restore "CatsShelter.Service/CatsShelter.Service.csproj"
COPY . .
WORKDIR "/src/CatsShelter.Service"
RUN dotnet build "CatsShelter.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatsShelter.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatsShelter.Service.dll"]
```

The Dockerfile starts by defining the base image and setting up the working directory and exposed ports. It then copies the project files into the Docker container, restores any NuGet packages, and builds the application. After that, it publishes the application and sets up the final image, which includes copying the published files and setting the entry point for the Docker container.

### Docker Compose

Docker Compose is used to define and run multi-container Docker applications. Here's a breakdown of the `docker-compose.yml` file:

```yaml
version: '3.4'

services:
  catsshelter.service:
    image: ${DOCKER_REGISTRY-}catsshelterservice
    build:
      context: ./src
      dockerfile: src/CatsShelter.Service/Dockerfile
    env_file:
      - .env
    ports:
      - "51217:80"
      - "44349:443"
    volumes:
      - ${DOCKER_HOME}/.aspnet/https:/https/
      - ${DOCKER_HOME}/.microsoft/usersecrets/:/root/.microsoft/usersecrets      
    depends_on:
      - mongo
  mongo:
    image: mongo
    env_file:
      - .env
    ports:
      - "27017:27017"
```

The `catsshelter.service` service is built from the Dockerfile in the `src/CatsShelter.Service` directory. It uses the environment variables defined in the `.env` file and depends on the `mongo` service, which means that the `mongo` service will be started before the `catsshelter.service` service.

The `mongo` service uses the `mongo` image, which is the official MongoDB Docker image. It also uses the environment variables defined in the `.env` file. The MongoDB service is exposed on port 27017.

The `catsshelter.service` service also mounts two volumes: one for the HTTPS certificates and another for the user secrets. The `${DOCKER_HOME}/.aspnet/https:/https/` volume is used to store the HTTPS development certificate, and the `${DOCKER_HOME}/.microsoft/usersecrets/:/root/.microsoft/usersecrets` volume is used to store the user secrets.

### Docker Home Variable

The `DOCKER_HOME` variable is used in the `docker-compose.yml` file to specify the path to the home directory on the host machine. This variable is typically set to the same value as the `HOME` environment variable. The `DOCKER_HOME` variable is used to specify the source directories for Docker volumes. In this case, it is used to specify the source directories for the volumes that share the HTTPS certificate and the user secrets with the Docker container.

### HTTPS Development Certificate

The application uses an HTTPS development certificate for secure communication. This certificate is generated using the `dotnet dev-certs` tool, which is a part of the .NET Core SDK. The certificate is then shared with the Docker container through a Docker volume.

To generate the HTTPS development certificate, use the following command:

```bash
dotnet dev-certs https -ep ${DOCKER_HOME}/.aspnet/https/aspnetapp.pfx -p <password>
```

Replace `<password>` with a password of your choice. This command generates a certificate and saves it in the `${DOCKER_HOME}/.aspnet/https` directory with the name `aspnetapp.pfx`.

The password for the certificate is stored in the user secrets, which are then accessed by the application at runtime. To set the user secrets, use the following commands:

```bash
dotnet user-secrets set "Kestrel:Certificates:Default:Password" "<password>"
dotnet user-secrets set "Kestrel:Certificates:Default:Path" "/https/aspnetapp.pfx"
```

Replace `<password>` with the same password you used when generating the certificate. These commands store the password and the path to the certificate in the user secrets.

The user secrets and the HTTPS certificate are then shared with the Docker container through Docker volumes. The `${DOCKER_HOME}/.aspnet/https:/https/` volume is used to share the HTTPS certificate, and the `${DOCKER_HOME}/.microsoft/usersecrets/:/root/.microsoft/usersecrets` volume is used to share the user secrets.

Please note that this development certificate should not be used in a production environment. For production, you should use a certificate from a trusted certificate authority.

### Environment Variables

The `.env` file is used to define environment variables that are used by the Docker containers. Here's a breakdown of the `.env` file:

```env
MONGO_INITDB_ROOT_USERNAME=myUser
MONGO_INITDB_ROOT_PASSWORD=myPassword
MongoDbConnection=mongodb://myUser:myPassword@mongo:27017
DatabaseName=cats-db
CollectionName=cats
ASPNETCORE_URLS=https://+;http://+
ASPNETCORE_HTTPS_PORT=44349
```

The `MONGO_INITDB_ROOT_USERNAME` and `MONGO_INITDB_ROOT_PASSWORD` variables are used to set the username and password for the MongoDB database. The `MongoDbConnection` variable is used to define the connection string for the MongoDB database. The `DatabaseName` and `CollectionName` variables are used to specify the name of the database and the collection that the application will use.

The `ASPNETCORE_URLS` variable is used to configure the URLs the application will listen on. The `ASPNETCORE_HTTPS_PORT` variable is used to set the HTTPS port the application will listen on.

In order to correctly run the Docker environment, this file must be added in the same path as the solution and compiled with the fitting values. For example, you should replace `myUser` and `myPassword` with the actual username and password that you want to use for the MongoDB database. Similarly, you should replace `cats-db` and `cats` with the actual name of the database and the collection that you want to use.

By using Docker and Docker Compose, the application and its dependencies can be run with a single command (`docker-compose up`), regardless of the host operating system. This simplifies the deployment process and ensures that the application runs in the same environment, regardless of where it is deployed.

## 🏃‍♂️ Running the Project with Docker

To run the project locally using Docker, follow these steps:

1. **Clone the repository**: Use git to clone the project repository to your local machine.

2. **Navigate to the project directory**: Use the command line to navigate into the root directory of the project.

3. **Set the DOCKER_HOME environment variable**: Set the `DOCKER_HOME` environment variable to the path of your home directory. This variable is used to specify the source directories for Docker volumes.

4. **Generate HTTPS development certificate**: Use the `dotnet dev-certs` tool to generate an HTTPS development certificate and save it in the `${DOCKER_HOME}/.aspnet/https` directory. Remember to set the password for the certificate in the user secrets.

5. **Build the Docker images**: Run the `docker-compose build` command to build the Docker images for the project. This command reads the `docker-compose.yml` file and builds Docker images for the services defined in it. The build process includes executing the instructions in the Dockerfile, such as copying the project files into the Docker image, restoring the NuGet packages, and compiling the application.

6. **Run the Docker containers**: After the Docker images have been built, you can start the Docker containers by running the `docker-compose up` command. This command starts the Docker containers for the services defined in the `docker-compose.yml` file. The containers are started in the correct order, taking into account the dependencies between them.

At this point, the application should be running inside a Docker container, and you should be able to interact with it as if it were running directly on your local machine. The MongoDB database is also running inside a Docker container and is accessible to the application.

Remember, since no volume is mounted for the MongoDB service in this sample project, the data stored in the MongoDB database will not persist across Docker sessions. This is intentional, as database persistence is not required for this sample project.

### 🧪 Testing the Project

Before you can proceed with testing, you need to ensure that the database is populated with data. The current state of the project does not seed the database by default. Therefore, you need to manually populate the database with entries for cats and their details. 

#### Populating the Database

To populate the database, you can use a MongoDB client such as MongoDB Compass. MongoDB Compass is a GUI for MongoDB that allows you to interact with your data visually. It's available for Windows, Mac, and Linux.

Here are the steps to download, install, and use MongoDB Compass:

1. **Download MongoDB Compass**: Visit the [MongoDB Download Center](https://www.mongodb.com/try/download/compass) and download the version of MongoDB Compass suitable for your operating system.

2. **Install MongoDB Compass**: Open the downloaded file and follow the installation instructions. 

3. **Connect to the MongoDB instance**: Open MongoDB Compass. In the connection dialog, enter the connection string for your MongoDB instance running in the Docker container:

    ```
    mongodb://myUser:myPassword@localhost:27017
    ```
   
   Click on "Connect" to establish the connection.

4. **Populate the Database**: Once connected, you can add entries to the `cats` collection in your database. Each entry should represent a cat and should include details like `id`, `name`, and `isAvailable`. To add a new document (entry), navigate to your database and collection, then click on the "INSERT DOCUMENT" button. In the dialog that appears, you can input the details of the cat in JSON format, like so:

    ```json
    {
        "_id": "1",
        "name": "Fluffy",
        "isAvailable": true
    }
    ```

   Click on "INSERT" to add the document to the collection.

**Note**: Replace `localhost` with the appropriate IP address if you're running the Docker container on a different machine.

#### Testing with BloomRPC

Once the database is populated, you can test the application using a gRPC client. [BloomRPC](https://github.com/uw-labs/bloomrpc) is an open-source GUI client for gRPC services that allows you to construct requests, send them to the gRPC server, and inspect the responses.

To test the application using BloomRPC, follow these steps:

1. **Install BloomRPC**: Download BloomRPC from the [official GitHub repository](https://github.com/uw-labs/bloomrpc) and install it on your machine.

2. **Import the Proto file**: Open BloomRPC and import the Proto file from the project. This file, located at `Features/Adoption/Proto/cats-shelter-service.proto`, describes the gRPC service and the message types it uses.

3. **Connect to the gRPC server**: Specify the address of the gRPC server in BloomRPC. If you're running the application locally with Docker, the address will be `localhost` and the port will be `80` (or `443` for HTTPS), unless you've specified a different port in the Docker configuration.

4. **Send requests**: Construct requests using the message types defined in the Proto file, send them to the server, and inspect the responses. For example, you can send a `GetAvailableCats` request to get a list of available cats, or a `RequestAdoption` request to request the adoption of a specific cat.

Remember to replace `localhost` with the appropriate IP address if you're running the Docker container on a different machine.

## 🧪 Tests Explanation

### Unit Tests

Unit tests focus on testing individual units of code in isolation to ensure their correctness and proper behavior. The unit tests in the project follow a "slice by feature" approach, organizing the tests by specific features. The test project structure is as follows:

```
CatShelter.Service.UnitTests
└── Features
    └── Adoption
        ├── Domain
        │   └── Entities
        │       └── CatTests.cs
        ├── Infrastructure
        │   └── Repositories
        │       └── CatsRepositoryTests.cs
        └── Services
            ├── CatsAdoptionGrpcServiceTests.cs
            └── CatsAdoptionServiceTests.cs
```

The unit tests are categorized based on the feature they cover. Here's an overview of the unit test categories:

- **Adoption / Domain / Entities**: Contains unit tests for the `Cat` entity to ensure its behavior and functionality.

- **Adoption / Infrastructure / Repositories**: Includes unit tests for the `CatsRepository` class. These tests ensure the repository functions correctly.

- **Adoption / Services / CatsAdoptionGrpcService**: Contains unit tests for the `CatsAdoptionGrpcService` class, covering various scenarios and edge cases related to the adoption gRPC service.

- **Adoption / Services / CatsAdoptionService**: Contains unit tests for the CatsAdoptionService class. These tests ensure the adoption service functions correctly.

### Integration Tests

Integration tests aim to test the interaction and integration between different components of the application to ensure they work seamlessly together. The integration tests in the project follow the same "slice by feature" approach. Here's the structure of the integration tests:

```
CatShelter.Service.IntegrationTests
└── Features
    └── Adoption
        ├── Infrastructure
        │   └── CatsDatabaseContextTests.cs
        └── Services
            ├── CatsAdoptionGrpcServiceTests.cs
            └── GrpcTestFixture.cs
```

The integration tests are organized based on the feature they cover. Here's an overview of the integration test categories:

- **Adoption / Infrastructure / CatsDatabaseContext**: Contains integration tests for the `CatsDatabaseContext` class. Due to the difficulty of mocking `MongoDB.Driver` classes, only integration tests were feasible for this component. These tests ensure the proper interaction between the application and the database context, covering database operations.

- **Adoption / Services / CatsAdoptionGrpcService**: Contains integration tests for the `CatsAdoptionGrpcService` class. These tests focus on testing the adoption gRPC service in an integrated environment, simulating real-world scenarios. They ensure the service interacts correctly with the database and responds appropriately to various requests.

Note: Currently, there are no integration tests available for the `CatsRepository` and `CatsAdoptionService` classes.

### CatsAdoptionGrpcServiceTests with GrpcTestFixture

The `CatsAdoptionGrpcServiceTests` class demonstrates the usage of the `GrpcTestFixture` as a test fixture for setting up the integration tests. The `GrpcTestFixture` class provides a convenient way to configure the test environment by starting an in-memory MongoDB instance, setting up the necessary configurations, and creating the gRPC client for testing.

Here's an example of the `GrpcTestFixture` class:

```csharp
public class GrpcTestFixture<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    public IHost Host { get; private set; }
    public MongoDbRunner MongoDbRunner { get; private set; }
    public IMongoCollection<Cat>? CatsCollection { get; private set; }
    public IMongoDatabase? MongoDatabase { get; private set; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        MongoDbRunner = MongoDbRunner.Start();
        var mongoDbConnectionString = MongoDbRunner.ConnectionString;
        var databaseName = $"TestDb_{Guid.NewGuid()}";
        var collectionName = $"TestCollection_{Guid.NewGuid()}";

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "MongoDbConnection", mongoDbConnectionString },
                { "DatabaseName", databaseName },
                { "CollectionName", collectionName }
            });
        });

        Host = builder.Build();
        Host.Start();

        var mongoClient = new MongoClient(mongoDbConnectionString);
        MongoDatabase = mongoClient.GetDatabase(databaseName);
        CatsCollection = MongoDatabase.GetCollection<Cat>(collectionName);

        // Add MongoClient to the service collection
        var serviceScopeFactory = Host.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = serviceScopeFactory.CreateScope();
        var services = scope.ServiceProvider;
        var serviceCollection = new ServiceCollection();
        foreach (var descriptor in services.GetRequiredService<IEnumerable<ServiceDescriptor>>())
        {
            serviceCollection.Add(descriptor);
        }
        serviceCollection.AddSingleton<IMongoClient>(mongoClient);
        var newServiceProvider = serviceCollection.BuildServiceProvider();

        return Host;
    }
}
```

The `GrpcTestFixture` class inherits from `WebApplicationFactory<TStartup>` and provides additional functionalities for configuring the test environment. It includes properties such as `Host`, `MongoDbRunner`, `CatsCollection`, and `MongoDatabase` for managing the test environment.

In the `CreateHost` method, the `MongoDbRunner` is started to initiate an in-memory MongoDB instance. The connection string, database name, and collection name are generated dynamically to ensure a unique and isolated test environment.

The `builder.ConfigureAppConfiguration` block is used to configure the application's settings, such as the MongoDB connection, database name, and collection name, by adding them to the in-memory configuration.

Next, the host is built and started. A `MongoClient` is created using the connection string, and the database and collection objects are obtained from the client.

To ensure that the MongoDB client is available within the service collection, it is added as a singleton using the `serviceCollection.AddSingleton<IMongoClient>(mongoClient)` method.

Finally, the new service provider is built, and the `Host` object is returned.

The `CatsAdoptionGrpcServiceTests` class utilizes the `GrpcTestFixture` as a test fixture for setting up the integration tests. Here's an example:

```csharp
public class CatsAdoptionGrpcServiceTests : IClassFixture<GrpcTestFixture<Startup>>
{
    private readonly Fixture _fixture;
    private readonly GrpcTestFixture<Startup> _factory;

    private const string ExpectedAdoptionSuccessMessage = "Adoption successful.";
    private const string ExpectedCancelAdoptionSuccessMessage = "Adoption canceled.";

    public CatsAdoptionGrpcServiceTests(GrpcTestFixture<Startup> factory)
    {
        _factory = factory;
        _fixture = new Fixture();
    }

    // Helper method to create the gRPC client
    private CatsShelterServiceClient CreateClient() => new
        (
            GrpcChannel.ForAddress
            (
                _factory.Server.BaseAddress,
                new GrpcChannelOptions { HttpClient = _factory.CreateClient() }
            )
        );

    // Integration tests go here...
}
```

The `GrpcTestFixture<Startup>` instance is passed to the constructor of the `CatsAdoptionGrpcServiceTests` class, allowing access to the test fixture's properties and functionalities.

In this example, the `_fixture` object is used for generating test data. The `CreateClient` helper method creates a gRPC client using the `GrpcChannel.ForAddress` method, configuring it with the base address of the test server obtained from the `GrpcTestFixture` instance.

The example integration tests in `CatsAdoptionGrpcServiceTests` demonstrate different scenarios and assertions, such as getting available cats, requesting adoption, canceling adoption, and handling edge cases.

The tests also include setup and cleanup steps using the in-memory MongoDB instance. Before each test, some data is inserted into the `CatsCollection` using the `_factory.CatsCollection!.InsertManyAsync` or `_factory.CatsCollection!.InsertOneAsync` method. After each test, the inserted data is deleted using the `_factory.CatsCollection.DeleteManyAsync` method.

### Running the Tests

To run the tests for the project, follow these steps:

1. Open a terminal or command prompt.

2. Navigate to the project directory.

3. Run the following command to execute the tests:

   ```shell
   dotnet test
   ```

   This command will discover and run all the tests within the solution.
