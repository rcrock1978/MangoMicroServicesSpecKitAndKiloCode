# Use the official .NET 9.0 runtime image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

# Use the official .NET 9.0 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["Mango.Services.OrderAPI.csproj", "."]
RUN dotnet restore "Mango.Services.OrderAPI.csproj"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR "/src/."
RUN dotnet build "Mango.Services.OrderAPI.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Mango.Services.OrderAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the base image for the final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "Mango.Services.OrderAPI.dll"]