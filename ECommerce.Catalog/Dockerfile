# STAGE 1: Build & Generate
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /src

# Install the EF Core tool globally in the build SDK container
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

# Copy the tool manifest and restore the Avro generator
COPY ECommerce.Catalog/dotnet-tools.json ./ECommerce.Catalog/
RUN dotnet tool restore --tool-manifest ECommerce.Catalog/dotnet-tools.json

# Copy project files and restore NuGet dependencies cleanly inside Linux
COPY ECommerce.slnx ./
COPY ECommerce.Catalog/ECommerce.Catalog.csproj ./ECommerce.Catalog/
RUN dotnet restore ECommerce.Catalog/ECommerce.Catalog.csproj

# Copy the rest of the source code
COPY . .

# PIPELINE AUTO-FIX: Fully qualify the Migration base class to resolve the Kafka namespace collision
RUN sed -i 's/: Migration/: Microsoft.EntityFrameworkCore.Migrations.Migration/g' ECommerce.Catalog/Migrations/*.cs

# FIX: Let the bundle command compile its own context natively without conflicting flags
RUN dotnet ef migrations bundle --project ECommerce.Catalog/ECommerce.Catalog.csproj --self-contained -r linux-x64 -o /app/out/migrate

# Publish the web app to the same out directory
RUN dotnet publish ECommerce.Catalog/ECommerce.Catalog.csproj -c Release -o /app/out --no-restore

# STAGE 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copies the published app AND the compiled 'migrate' bundle together
COPY --from=build-env /app/out .

ENTRYPOINT ["dotnet", "ECommerce.Catalog.dll"]