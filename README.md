# Coinbase WebSocket Client

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Overview

The Coinbase WebSocket Client is a high-performance, real-time C# application designed for robust interaction with the Coinbase WebSocket API. It provides market data streaming capabilities for cryptocurrency markets on the Coinbase platform, with a focus on reliability, maintainability, and extensibility.

## Key Features

- Real-time WebSocket connection with automatic reconnection and error handling
- Secure authentication using API keys and JWT tokens
- Market data streaming for multiple channels:
  - Candles
  - Status updates
  - Market trades
  - Heartbeats
  - Level 2 order book data
- Support for multiple product IDs
- Modular architecture with dependency injection for easy testing and maintenance
- Comprehensive logging for debugging and monitoring
- Rate limiting to comply with Coinbase API restrictions

## Technology Stack

- **Framework**: .NET 8.0
- **Language**: C# 12
- **Dependencies**:
  - Microsoft.Extensions.Configuration.Json (v8.0.0) for configuration management
  - Microsoft.Extensions.DependencyInjection (v8.0.0) for dependency injection
  - Microsoft.Extensions.Logging (v8.0.0) for structured logging
  - System.Text.Json for JSON parsing
- **Testing**: xUnit, Moq (planned)

## Project Structure
```
CoinbaseWebSocketClient/
├── src/
│   └── CoinbaseWebSocketClient/
│       ├── Configuration/
│       │   ├── Config.cs
│       │   └── Constants.cs
│       ├── Services/
│       │   ├── MessageProcessor.cs
│       │   ├── WebSocketClient.cs
│       │   ├── WebSocketHandler.cs
│       │   └── WebSocketManager.cs
│       ├── Models/
│       │   ├── CandlesMessage.cs
│       │   ├── StatusMessage.cs
│       │   ├── UserMessage.cs
│       │   ├── HeartbeatMessage.cs
│       │   ├── MarketTradesMessage.cs
│       │   └── Level2Message.cs
│       ├── Interfaces/
│       └── Program.cs
├── tests/
│   └── CoinbaseWebSocketClient.Tests/
└── README.md
```

## Services

### WebSocketManager

Manages multiple WebSocket connections:

- Initializes and maintains connections for each product ID
- Handles reconnection logic
- Distributes received messages to the appropriate handlers

### WebSocketHandler

Handles individual WebSocket connections:

- Manages the connection lifecycle
- Sends subscription messages
- Receives and forwards messages to the MessageProcessor

### MessageProcessor

Processes received WebSocket messages:

- Deserializes messages into appropriate model classes
- Handles different message types (candles, status, market trades, etc.)
- Implements error handling and logging

Key methods:
- `ProcessReceivedMessage`: Main entry point for message processing
- `ProcessCandlesMessage`: Handles candle-specific messages
- `ProcessHeartbeat`: Extracts and logs heartbeat information
- `ProcessStatusMessage`: Processes status updates for products
- `ProcessMarketTradesMessage`: Handles market trade information

```typescript:CoinbaseWebSocketClient/Services/MessageProcessor.cs
startLine: 1
endLine: 199
```

## Models

The application uses the following key models:

### CandlesMessage

Represents the structure of candle data received from the Coinbase WebSocket API.

```typescript:CoinbaseWebSocketClient/Models/CandlesMessage.cs
startLine: 1
endLine: 31
```

### StatusMessage

Represents the structure of status messages received from the Coinbase WebSocket API.

```typescript:CoinbaseWebSocketClient/Models/StatusMessage.cs
startLine: 1
endLine: 33
```

### MarketTradesMessage

Represents the structure of market trade data received from the Coinbase WebSocket API.

```typescript:CoinbaseWebSocketClient/Models/MarketTradesMessage.cs
startLine: 1
endLine: 29
```

### Level2Message

Represents the structure of Level 2 order book data received from the Coinbase WebSocket API.

```typescript:CoinbaseWebSocketClient/Models/Level2Message.cs
startLine: 1
endLine: 28
```

## Configuration

The application uses environment variables for configuration. Key configuration options include:

- API credentials (API Key and Private Key)
- WebSocket URL
- Product IDs to subscribe to
- Channels to subscribe to
- WebSocket buffer size

Configuration is managed through the `Config` class:

```typescript:CoinbaseWebSocketClient/Configuration/Config.cs
startLine: 1
endLine: 117
```

## Usage

To use the Coinbase WebSocket Client:

1. Set the required environment variables (API Key, Private Key, etc.)
2. Run the application using `dotnet run`

The application will automatically connect to the Coinbase WebSocket API and start receiving data for the configured products and channels.

## Contributing

We welcome contributions! Please follow these steps:

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

Please ensure your code adheres to the existing style and includes appropriate tests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support and Contact

For bug reports and feature requests, please open an issue on GitHub. For security-related issues, please contact the maintainers directly.

---

Developed and maintained by [Your Name/Company]