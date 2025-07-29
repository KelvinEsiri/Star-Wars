# Use the official .NET 8 runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Star Wars.csproj", "."]
RUN dotnet restore "./Star Wars.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "Star Wars.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Star Wars.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

ENTRYPOINT ["dotnet", "Star Wars.dll"]
