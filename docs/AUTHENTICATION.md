# 🔐 Authentication Guide

Complete authentication system documentation for the Star Wars API.

## Overview

The Star Wars API uses a **dynamic, user-specific API key authentication system** with **automatic cookie management** for seamless browser integration.

### 🌟 Key Features

- **🔑 Dynamic API Keys**: Each user gets a unique API key generated upon registration/login
- **🍪 Automatic Cookie Management**: API keys are automatically saved and used via HTTP cookies
- **🔄 Dual Authentication Methods**: Support for both header-based (API clients) and cookie-based (browsers) authentication
- **⏰ Expiration Management**: API keys expire after 30 minutes and can be regenerated
- **🛡️ Secure Lifecycle**: Complete key management with regeneration and revocation capabilities

### 🚀 How It Works

1. **Register or Login** → API key automatically generated and saved as cookie
2. **Access Any Endpoint** → Cookie automatically sent, no manual headers needed
3. **Complete Transparency** → Works seamlessly in browsers and API clients

## Authentication Methods

### 1. 🍪 Cookie Authentication (Automatic)
- Set automatically on login/registration
- Cookie name: `StarWarsApiKey`
- Works seamlessly in browsers
- 30-minute expiration (renewable)

### 2. 🔑 Header Authentication (Manual)
- Header: `X-API-Key: your-api-key-here`
- Required for API clients
- Same 30-minute expiration
- Can be obtained from login response

## Quick Start Examples

### Browser Usage (Automatic)
```javascript
// 1. Login once
await fetch('/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email: 'user@example.com', password: 'password' })
});

// 2. Use any endpoint automatically
const starships = await fetch('/api/starships');
const data = await starships.json();
```

### API Client Usage (Manual Headers)
```bash
# Get API key from login
TOKEN=$(curl -s -X POST "https://localhost:7108/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}' | jq -r '.token')

# Use with headers
curl -X GET "https://localhost:7108/api/starships" \
  -H "X-API-Key: $TOKEN"
```

## Authentication Endpoints

### Register New User
```http
POST /api/auth/register
Content-Type: application/json

{
  "firstName": "Luke",
  "lastName": "Skywalker", 
  "email": "luke@jedi.com",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "token": "abc123...",
  "email": "luke@jedi.com",
  "firstName": "Luke",
  "lastName": "Skywalker",
  "expiresAt": "2025-07-29T01:37:48.911Z"
}
```
✅ **API key automatically saved as `StarWarsApiKey` cookie**

### User Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "luke@jedi.com",
  "password": "Password123!"
}
```

**Response:**
```json
{
  "token": "xyz789...",
  "email": "luke@jedi.com", 
  "firstName": "Luke",
  "lastName": "Skywalker",
  "expiresAt": "2025-07-29T01:38:27.915Z"
}
```
✅ **API key automatically saved as `StarWarsApiKey` cookie**

### Password Recovery

#### Request Password Reset
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "luke@jedi.com"
}
```

**Response:**
```json
{
  "message": "If an account with that email exists, a password reset link has been sent",
  "resetTokenSent": true,
  "expiresInMinutes": 15
}
```

#### Reset Password
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "luke@jedi.com",
  "resetToken": "secure-reset-token-from-email",
  "newPassword": "NewSecurePassword123!",
  "confirmPassword": "NewSecurePassword123!"
}
```

## API Key Management

### Get API Key Info
```http
GET /api/apikey/info
```
*Uses automatic cookie authentication*

### Regenerate API Key
```http
POST /api/apikey/regenerate
```
*Uses automatic cookie authentication*

**Response:**
```json
{
  "message": "API key regenerated successfully",
  "note": "New API key has been automatically saved as a cookie for browser access"
}
```

### Revoke API Key
```http
POST /api/apikey/revoke
```
*Uses automatic cookie authentication*

## Architecture Overview

### Hybrid Authentication System

The Star Wars API implements a **hybrid authentication architecture** that seamlessly combines **ASP.NET Core Identity** with **custom API key middleware** to provide enterprise-grade security with developer-friendly usability.

#### Core Components

1. **🛡️ ASP.NET Core Identity Framework**
   - User registration and account management
   - Secure password hashing with PBKDF2
   - Email validation and account confirmation
   - Password reset and recovery mechanisms
   - Role-based authorization support (ready for expansion)

2. **🔑 Custom API Key System**
   - Dynamic per-user API key generation
   - Cryptographically secure key creation
   - Automatic expiration and renewal
   - Dual delivery methods (header + cookie)

3. **🍪 Intelligent Cookie Management**
   - Automatic browser authentication
   - Secure cookie configuration (HttpOnly, Secure, SameSite)
   - Synchronized expiration with API keys
   - GDPR-compliant essential cookies

4. **🔄 Middleware Pipeline Integration**
   - Seamless integration with ASP.NET Core pipeline
   - Custom authentication schemes
   - Request context enrichment
   - Claims-based authorization

### Request Processing Flow

```
HTTP Request
    ↓
1. ASP.NET Core Authentication Middleware
   - Processes standard authentication schemes
   - Handles cookie authentication if present
    ↓
2. Custom API Key Middleware (ApiKeyMiddleware.cs)
   - Extracts API key from X-API-Key header
   - Falls back to StarWarsApiKey cookie
   - Validates key format and database lookup
    ↓
3. User Context Establishment
   - Create ClaimsPrincipal with user information
   - Set HttpContext.User for authorization
   - Add custom claims for audit trail
    ↓
4. Authorization Middleware
   - Evaluate authorization policies
   - Check resource-specific permissions
   - Apply role-based access control
    ↓
5. Controller Action Execution
   - Access user context via HttpContext.User
   - Perform authorized operations
   - Return response with security headers
```

## Security Features

### Password Security
- **Minimum Length**: 8 characters
- **Character Types**: Must include uppercase, lowercase, digits, and special characters
- **PBKDF2 Hashing**: Secure password storage with 10,000 iterations
- **Account Lockout**: 5 failed attempts = 15-minute lockout

### API Key Security
- **Cryptographic Generation**: 32-byte secure random values
- **Short Lifespan**: 30-minute expiration reduces exposure
- **Secure Storage**: Indexed database lookups with consistent timing
- **Audit Logging**: Comprehensive security event tracking

### Cookie Security
- **HttpOnly**: Prevents XSS via JavaScript access
- **Secure**: HTTPS-only transmission in production
- **SameSite=Strict**: Prevents CSRF attacks
- **Path Scoped**: Cookie only sent to API paths

### Protection Against Attacks
- **Timing Attacks**: Constant-time comparisons and indexed queries
- **Brute Force**: Account lockout and rate limiting
- **CSRF**: SameSite cookies and custom headers
- **XSS**: HttpOnly cookies and secure headers

## Configuration

### Development Settings
```json
{
  "Authentication": {
    "RequireHttps": false,
    "CookieSecure": false,
    "AllowInsecurePasswords": true,
    "RequireEmailConfirmation": false,
    "LockoutEnabled": false
  }
}
```

### Production Settings
```json
{
  "Authentication": {
    "RequireHttps": true,
    "CookieSecure": true,
    "AllowInsecurePasswords": false,
    "RequireEmailConfirmation": true,
    "LockoutEnabled": true,
    "ApiKeyExpirationMinutes": 30,
    "MaxFailedAccessAttempts": 3,
    "LockoutTimeMinutes": 60
  }
}
```

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| **API Key Expired** | Use `/api/apikey/regenerate` or login again |
| **Cookie Authentication** | Clear browser cookies and re-login |
| **Invalid Credentials** | Check email/password, account may be locked |
| **CORS Issues** | Ensure credentials: 'include' in fetch requests |

### Diagnostic Commands
```bash
# Test API authentication
curl -X POST "https://localhost:7108/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Password123!"}'

# Test API key usage
curl -X GET "https://localhost:7108/api/starships" \
  -H "X-API-Key: [your-api-key-here]"
```

## Testing Authentication

### Unit Tests
```csharp
[Fact]
public async Task ApiKeyMiddleware_ValidKey_AuthenticatesUser()
{
    // Arrange
    var validApiKey = "SW-validkeyexample-2024";
    var context = CreateTestContext();
    context.Request.Headers["X-API-Key"] = validApiKey;
    
    // Act
    await _middleware.InvokeAsync(context);
    
    // Assert
    Assert.NotNull(context.User);
    Assert.True(context.User.Identity.IsAuthenticated);
}
```

### Integration Tests
```csharp
[Fact]
public async Task GetStarships_WithValidApiKey_ReturnsData()
{
    // Arrange
    var client = _factory.CreateClient();
    var apiKey = await RegisterUserAndGetApiKey();
    client.DefaultRequestHeaders.Add("X-API-Key", apiKey);
    
    // Act
    var response = await client.GetAsync("/api/starships");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

---

**[← Back to Main README](../README.md)**
