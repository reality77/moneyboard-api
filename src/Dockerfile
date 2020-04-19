FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY . ./
WORKDIR /app/api

RUN dotnet restore

# Copy everything else and build
RUN dotnet publish -c Release -o out

CMD rm -f /app/api/out/appsettings.json && rm -f /app/api/out/appsettings.Development.json    

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-env /app/api/out .
ENTRYPOINT ["dotnet", "api.dll"]
