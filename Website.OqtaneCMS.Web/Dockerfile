# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files and restore dependencies
COPY ["Website.OqtaneCMS.Web/Website.OqtaneCMS.Web.csproj", "Website.OqtaneCMS.Web/"]
COPY ["Website.OqtaneCMS.ServiceDefaults/Website.OqtaneCMS.ServiceDefaults.csproj", "Website.OqtaneCMS.ServiceDefaults/"]
RUN dotnet restore "Website.OqtaneCMS.Web/Website.OqtaneCMS.Web.csproj"

# Copy remaining source code
COPY . .

# Build the application
WORKDIR "/src/Website.OqtaneCMS.Web"
RUN dotnet build "Website.OqtaneCMS.Web.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Website.OqtaneCMS.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Install required runtime dependencies for Oqtane
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libc6-dev \
    && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Create directories for Oqtane runtime
RUN mkdir -p /app/Content/Tenants/Default \
    && mkdir -p /app/wwwroot/Modules \
    && mkdir -p /app/wwwroot/Themes

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "Website.OqtaneCMS.Web.dll"]
