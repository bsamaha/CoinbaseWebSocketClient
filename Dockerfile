FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CoinbaseWebSocketClient.csproj", "./"]
RUN dotnet restore "CoinbaseWebSocketClient.csproj"
COPY . .
RUN dotnet publish "CoinbaseWebSocketClient.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
CMD ["dotnet", "CoinbaseWebSocketClient.dll"]