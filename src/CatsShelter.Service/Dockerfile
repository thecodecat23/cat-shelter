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