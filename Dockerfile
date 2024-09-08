# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the CoinbaseWebSocketClient project file
COPY ["CoinbaseWebSocketClient/CoinbaseWebSocketClient.csproj", "CoinbaseWebSocketClient/"]

# Restore dependencies
RUN dotnet restore "CoinbaseWebSocketClient/CoinbaseWebSocketClient.csproj"

# Copy only the CoinbaseWebSocketClient directory
COPY CoinbaseWebSocketClient/ CoinbaseWebSocketClient/

# Build and publish the application
RUN dotnet publish "CoinbaseWebSocketClient/CoinbaseWebSocketClient.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "CoinbaseWebSocketClient.dll"]
ENV VERSION=3.0.0
