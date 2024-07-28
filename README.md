# Coinbase WebSocket Client

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Overview

The Coinbase WebSocket Client is a high-performance, real-time C# application designed for robust interaction with the Coinbase WebSocket API. It provides market data streaming capabilities for cryptocurrency markets on the Coinbase platform, with a focus on reliability and maintainability.

## Key Features

- Real-time WebSocket connection with automatic reconnection and error handling
- Secure authentication using API keys
- Market data streaming (e.g., candles) for specified product IDs
- Kafka integration for scalable data persistence and processing
- Containerized deployment support

## Technology Stack

- **Framework**: .NET 8.0
- **Language**: C# 12
- **Dependencies**:
  - Confluent.Kafka (v2.3.0) for Kafka integration
  - Microsoft.Extensions.Configuration.Json (v8.0.0) for configuration management
  - Microsoft.Extensions.DependencyInjection (v8.0.0) for dependency injection
  - Microsoft.Extensions.Logging (v8.0.0) for structured logging
  - System.IdentityModel.Tokens.Jwt (v7.2.0) for JWT handling
  - jose-jwt (v4.1.0) for JWT creation and validation
- **Testing**: xUnit, Moq
- **Containerization**: Docker

## Project Structure

```
CoinbaseWebSocketClient/
├── src/
│   └── CoinbaseWebSocketClient/
│       ├── Configuration/
│       ├── Services/
│       │   ├── WebSocketClient.cs
│       │   ├── MessageProcessor.cs
│       │   ├── KafkaProducer.cs
│       │   ├── WebSocketHandler.cs
│       │   └── WebSocketManager.cs
│       ├── Utilities/
│       ├── Interfaces/
│       └── Program.cs
├── tests/
│   └── CoinbaseWebSocketClient.Tests/
├── Dockerfile
├── CoinbaseWebSocketClient.sln
└── README.md
```

## Setup and Configuration

### Prerequisites

- .NET 8.0 SDK
- Docker (for containerized deployment)

### Local Development Setup

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/CoinbaseWebSocketClient.git
   cd CoinbaseWebSocketClient
   ```

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Configure the application:
   ```bash
   cp src/CoinbaseWebSocketClient/appsettings.json src/CoinbaseWebSocketClient/appsettings.Development.json
   # Edit appsettings.Development.json with your Coinbase API credentials and Kafka configuration
   ```

4. Run the application:
   ```bash
   dotnet run --project src/CoinbaseWebSocketClient
   ```

## Testing

Run the unit tests:

```bash
dotnet test
```

## Deployment

### Docker Deployment

1. Build the Docker image:
   ```bash
   docker build -t coinbase-websocket-client .
   ```

2. Run the container:
   ```bash
   docker run -d --name coinbase-client coinbase-websocket-client
   ```

## Configuration

The application uses `appsettings.json` for configuration. Key configuration options include:

- `apiKey`: Your Coinbase API key
- `privateKey`: Your Coinbase API private key
- `webSocketUrl`: Coinbase WebSocket endpoint
- `productIds`: Array of product IDs to subscribe to
- `channel`: Channel to subscribe to (e.g., "candles")
- `webSocketBufferSize`: Buffer size for WebSocket messages
- `kafkaBootstrapServers`: Kafka broker addresses
- `kafkaTopic`: Kafka topic for market data
- `kafkaUsername`: Kafka username
- `kafkaPassword`: Kafka password

## Architecture

The application follows a modular, dependency-injected architecture:

- `WebSocketManager`: Handles WebSocket connections and message receiving
- `MessageProcessor`: Processes incoming WebSocket messages
- `KafkaProducer`: Sends processed data to Kafka
- `JwtGenerator`: Generates JWT tokens for authentication

## Services

The application's core functionality is implemented in the Services folder:

### WebSocketClient

A lightweight wrapper around the `ClientWebSocket` class, implementing the `IWebSocketClient` interface. It provides a clean abstraction for WebSocket operations:

- `ConnectAsync`: Establishes a connection to the WebSocket server
- `SendAsync`: Sends data over the WebSocket connection
- `ReceiveAsync`: Receives data from the WebSocket connection
- `CloseAsync`: Closes the WebSocket connection

This class simplifies WebSocket interactions and makes it easier to mock for testing purposes.

```typescript:CoinbaseWebSocketClient/Services/WebSocketClient.cs
startLine: 1
endLine: 27
```

### MessageProcessor

Handles the processing of received WebSocket messages:

- Logs received messages for debugging purposes
- Sends messages to Kafka using the `KafkaProducer`
- Includes specialized methods for processing heartbeats and ticker messages
- Implements error handling to ensure continuous operation even if Kafka production fails

Key methods:
- `ProcessReceivedMessage`: Main entry point for message processing
- `ProcessHeartbeat`: Extracts and logs heartbeat information
- `ProcessTicker`: Handles ticker-specific messages and sends them to Kafka

```typescript:CoinbaseWebSocketClient/Services/MessageProcessor.cs
startLine: 1
endLine: 59
```

### KafkaProducer

Manages the production of messages to Kafka:

- Initializes a Kafka producer with configured settings
- Provides a thread-safe method to produce messages to a specified topic
- Implements `IDisposable` for proper resource management
- Includes logging for successful message delivery and errors

Key features:
- Lazy initialization of the Kafka producer
- Error handling for production failures
- Logging of delivery results and errors

```typescript:CoinbaseWebSocketClient/Services/KafkaProducer.cs
startLine: 1
endLine: 61
```

### WebSocketHandler

Manages individual WebSocket connections for specific product IDs:

- Handles connection establishment and subscription to Coinbase channels
- Implements rate limiting for message sending to comply with API restrictions
- Manages message receiving and processing
- Utilizes a semaphore to limit concurrent connections

Key methods:
- `ConnectAndSubscribe`: Establishes connection and subscribes to specified channels
- `ReceiveMessages`: Continuously receives and processes messages from the WebSocket

```typescript:CoinbaseWebSocketClient/Services/WebSocketHandler.cs
startLine: 1
endLine: 97
```

### WebSocketManager

Orchestrates multiple WebSocket connections for different product IDs:

- Initializes and manages multiple `WebSocketHandler` instances
- Coordinates the connection and message receiving processes across all handlers
- Utilizes dependency injection for flexible configuration and easier testing

Key methods:
- `InitializeConnections`: Sets up WebSocket connections for all configured product IDs
- `StartReceiving`: Begins the message receiving process for all active connections

```typescript:CoinbaseWebSocketClient/Services/WebsocketManager.cs
startLine: 1
endLine: 57
```

## Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support and Contact

For bug reports and feature requests, please open an issue on GitHub.

---

Developed and maintained by [Your Name/Company]