# Use the official .NET 9.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy solution file and project files
COPY GeminiWarehouse.sln ./
COPY src/GeminiWarehouse.Api/GeminiWarehouse.Api.csproj ./src/GeminiWarehouse.Api/
COPY src/GeminiWarehouse.Application/GeminiWarehouse.Application.csproj ./src/GeminiWarehouse.Application/
COPY src/GeminiWarehouse.Contracts/GeminiWarehouse.Contracts.csproj ./src/GeminiWarehouse.Contracts/
COPY src/GeminiWarehouse.Domain/GeminiWarehouse.Domain.csproj ./src/GeminiWarehouse.Domain/
COPY src/GeminiWarehouse.Infrastructure/GeminiWarehouse.Infrastructure.csproj ./src/GeminiWarehouse.Infrastructure/

# Restore NuGet packages
RUN dotnet restore GeminiWarehouse.sln

# Copy the entire source code
COPY . ./

# Build and publish the API project
RUN dotnet publish src/GeminiWarehouse.Api/GeminiWarehouse.Api.csproj -c Release -o out

# Use the official .NET 9.0 ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build-env /app/out .

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser


# Set the entry point for the container
ENTRYPOINT ["dotnet", "GeminiWarehouse.Api.dll"]