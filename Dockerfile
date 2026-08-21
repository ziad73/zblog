# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /src
ENV ASPNETCORE_URLS=http://+:5112
EXPOSE 5112
# Copy project file and restore dependencies ONCE
COPY zblog.csproj .
RUN dotnet restore

# Target 1: Development (Hot Reload with `dotnet watch`)
FROM base AS development
COPY . .
CMD ["dotnet", "watch", "run", "--no-launch-profile"]


# Stage 2: Build & Publish for Production
FROM base AS build
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Target 2: Production (Slim Runtime ~200MB)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS production
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5112
EXPOSE 5112

COPY --from=build /app/publish .

# Non-root execution (ASP.NET 10 image provides $APP_UID / 'app' user)
# environment variable set via ENV APP_UID=1654
USER $APP_UID

ENTRYPOINT ["dotnet", "zblog.dll"]
