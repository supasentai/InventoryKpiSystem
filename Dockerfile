FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY InventoryKpiSystem.sln ./
COPY src/Inventory.Domain/Inventory.Domain.csproj src/Inventory.Domain/
COPY src/Inventory.Application/Inventory.Application.csproj src/Inventory.Application/
COPY src/Inventory.Infrastructure/Inventory.Infrastructure.csproj src/Inventory.Infrastructure/
COPY src/Inventory.ConsoleApp/Inventory.ConsoleApp.csproj src/Inventory.ConsoleApp/
COPY src/Inventory.Api/Inventory.Api.csproj src/Inventory.Api/
COPY tests/Inventory.Application.Tests/Inventory.Application.Tests.csproj tests/Inventory.Application.Tests/

RUN dotnet restore InventoryKpiSystem.sln

COPY . .
RUN dotnet publish src/Inventory.Api/Inventory.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
