# WebEngage Private SSP

A comprehensive Server-Side Platform (SSP) implementation for WebEngage that enables sending SMS campaigns to encrypted phone numbers while maintaining user privacy.

## Features

- **PII Encryption/Decryption**: Secure handling of hashed phone numbers and emails
- **Priority Queue System**: Multi-level priority queuing for SMS messages
- **SMS Provider Integration**: Flexible integration with various SMS service providers
- **Webhook Notifications**: Real-time delivery status updates back to WebEngage
- **Retry Mechanism**: Automatic retry logic for failed SMS deliveries
- **Comprehensive Logging**: Detailed logging for monitoring and debugging
- **Health Monitoring**: Built-in health checks and status endpoints

## Architecture

The solution follows a clean architecture pattern with the following layers:

- **Core**: Domain models, interfaces, and business logic
- **Services**: Implementation of core business services
- **API**: Web API endpoints for WebEngage integration
- **Infrastructure**: Data access and external service integrations

## Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or JetBrains Rider
- Access to an SMS service provider (Twilio, AWS SNS, etc.)

## Setup Instructions

### 1. Clone and Build

```bash
git clone <repository-url>
cd PrivateSSP
dotnet restore
dotnet build
```

### 2. Configuration

Update the `appsettings.json` file with your configuration:

```json
{
  "Encryption": {
    "Key": "your-32-byte-encryption-key-here-base64-encoded",
    "IV": "your-16-byte-iv-here-base64-encoded"
  },
  "SmsProvider": {
    "Name": "your-provider-name",
    "ApiKey": "your-sms-provider-api-key",
    "ApiUrl": "https://your-sms-provider.com/api/send",
    "StatusUrl": "https://your-sms-provider.com/api/status",
    "BalanceUrl": "https://your-sms-provider.com/api/balance"
  },
  "WebEngage": {
    "WebhookUrl": "https://your-webengage-instance.com/webhook/sms-status"
  }
}
```

### 3. Generate Encryption Keys

Generate a 32-byte key and 16-byte IV for AES encryption:

```csharp
using System.Security.Cryptography;

var key = new byte[32];
var iv = new byte[16];

using (var rng = RandomNumberGenerator.Create())
{
    rng.GetBytes(key);
    rng.GetBytes(iv);
}

var keyBase64 = Convert.ToBase64String(key);
var ivBase64 = Convert.ToBase64String(iv);

Console.WriteLine($"Key: {keyBase64}");
Console.WriteLine($"IV: {ivBase64}");
```

### 4. Run the Application

```bash
cd src/PrivateSSP.API
dotnet run
```

The API will be available at `https://localhost:5001` (or the configured port).

## API Endpoints

### SMS Endpoint

**POST** `/api/sms/send`

Receives SMS requests from WebEngage and processes them through the queue.

**Request Body:**
```json
{
  "uuid": "unique-message-id",
  "hashed_phone": "encrypted-phone-number",
  "message": "SMS message content",
  "sender_id": "SENDER",
  "campaign_id": "campaign-123",
  "user_id": "user-456",
  "custom_data": {
    "priority": 1
  }
}
```

**Response:**
```json
{
  "status": "sms_accepted",
  "statusCode": 1000,
  "message": "SMS accepted and queued for delivery",
  "uuid": "unique-message-id",
  "timestamp": "2024-01-01T00:00:00Z"
}
```

### Health Check

**GET** `/health`

Returns the health status of the service.

### Queue Status

**GET** `/api/sms/queue/status`

Returns the current status of the SMS queue.

## WebEngage Integration

### 1. Configure Private SSP in WebEngage

1. Go to **Data Platform > Integrations > SMS Setup**
2. Select **Private SSP** from the available SSPs
3. Configure with your API endpoint: `https://your-domain.com/api/sms/send`
4. Add any custom headers if needed for authentication

### 2. PII Hashing

Before sending data to WebEngage, hash your PII data:

```javascript
// JavaScript example
webengage.user.setAttribute({
  'we_hashed_phone': 'encrypted-phone-number',
  'we_hashed_email': 'encrypted-email'
});
```

### 3. Webhook Configuration

The Private SSP will automatically send delivery status notifications to the configured WebEngage webhook URL.

## SMS Provider Integration

The system is designed to work with any SMS provider that offers a REST API. Update the configuration to point to your preferred provider.

### Supported Providers

- Twilio
- AWS SNS
- MessageBird
- Plivo
- Custom providers with REST APIs

## Monitoring and Logging

### Log Levels

- **Information**: Normal operation events
- **Warning**: Non-critical issues
- **Error**: Critical errors and failures
- **Debug**: Detailed debugging information (development only)

### Health Monitoring

Monitor the `/health` endpoint to ensure the service is running correctly.

## Security Considerations

1. **Encryption Keys**: Store encryption keys securely and rotate them regularly
2. **API Authentication**: Implement proper authentication for your API endpoints
3. **HTTPS**: Always use HTTPS in production
4. **Rate Limiting**: Implement rate limiting to prevent abuse
5. **Input Validation**: All inputs are validated before processing

## Performance Tuning

### Queue Configuration

Adjust queue settings in `appsettings.json`:

```json
{
  "Queue": {
    "MaxConcurrentMessages": 10,
    "RetryDelaySeconds": 30,
    "MaxRetries": 3
  }
}
```

### SMS Provider Rate Limits

Configure your SMS provider's rate limits to match your expected message volume.

## Troubleshooting

### Common Issues

1. **Encryption Errors**: Verify encryption key and IV configuration
2. **SMS Provider Errors**: Check API credentials and endpoint URLs
3. **Webhook Failures**: Verify WebEngage webhook URL configuration
4. **Queue Issues**: Monitor queue size and processing logs

### Debug Mode

Enable debug logging in development:

```json
{
  "Logging": {
    "LogLevel": {
      "PrivateSSP": "Debug"
    }
  }
}
```

## Development

### Project Structure

```
PrivateSSP/
├── src/
│   ├── PrivateSSP.API/          # Web API endpoints
│   ├── PrivateSSP.Core/         # Domain models and interfaces
│   ├── PrivateSSP.Services/     # Business logic implementation
│   └── PrivateSSP.Infrastructure/ # Data access and external services
├── tests/
│   └── PrivateSSP.Tests/        # Unit and integration tests
└── PrivateSSP.sln               # Solution file
```

### Adding New Features

1. Define interfaces in the Core project
2. Implement services in the Services project
3. Add API endpoints in the API project
4. Write tests in the Tests project

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and questions:

1. Check the troubleshooting section
2. Review the logs for error details
3. Create an issue in the repository
4. Contact the development team

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## Version History

- **v1.0.0**: Initial release with core SMS functionality
- Future versions will include additional features and improvements
