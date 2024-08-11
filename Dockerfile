# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["CoinbaseWebSocketClient/CoinbaseWebSocketClient.csproj", "./"]
RUN dotnet restore "CoinbaseWebSocketClient.csproj"

# Copy the rest of the source code
COPY CoinbaseWebSocketClient .

# Build and publish the application
RUN dotnet publish "CoinbaseWebSocketClient.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "CoinbaseWebSocketClient.dll"]
ENV VERSION=2.0.0
