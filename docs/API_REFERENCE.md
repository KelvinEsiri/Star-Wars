# 📚 API Reference Guide

Complete endpoint documentation for the Star Wars API.

## Overview

The Star Wars API offers **18 endpoints** across **5 main controllers** with comprehensive CRUD operations, authentication, and administrative features.

## Endpoint Summary

| Controller | Endpoints | Authentication | Purpose |
|------------|-----------|---------------|---------|
| **🔐 Auth** | 4 endpoints | None required | User registration, login, password management |
| **🔑 ApiKey** | 3 endpoints | Required | API key lifecycle management |
| **🚀 Starships** | 8 endpoints | Required | Full CRUD operations for starships |
| **🌱 Seed** | 1 endpoint | Required | Database seeding from SWAPI |
| **👑 Admin** | 2 endpoints | Required + Admin Key | Administrative operations |

## Authentication Methods

All protected endpoints support dual authentication:

- **🍪 Cookie Authentication** - Automatic for browsers (set on login/register)
- **🔑 Header Authentication** - Manual for API clients (`X-API-Key` header)

## Response Patterns

### Success Responses
- `200 OK` - Successful data retrieval
- `201 Created` - Resource created successfully  
- `204 No Content` - Successful deletion/update

### Error Responses
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing/invalid authentication
- `403 Forbidden` - Insufficient permissions (admin endpoints)
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

### Standard Error Format
```json
{
  "error": "Error description",
  "details": "Additional context",
  "timestamp": "2025-07-29T01:30:00Z"
}
```

---

## 🔐 AuthController Endpoints

These endpoints handle user registration, login, and password management. **No authentication required**.

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

**Response (201 Created):**
```json
{
  "token": "SW-dGhpc2lzYWV4YW1wbGVhcGlrZXlmb3J0ZXN0aW5n-2024",
  "email": "luke@jedi.com",
  "firstName": "Luke",
  "lastName": "Skywalker",
  "expiresAt": "2025-07-29T01:37:48.911Z"
}
```

**Features:**
- ✅ Automatic API key generation
- ✅ Sets `StarWarsApiKey` cookie for immediate API access
- ✅ Password complexity validation
- ✅ Duplicate email prevention

### User Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "luke@jedi.com",
  "password": "Password123!"
}
```

**Response (200 OK):**
```json
{
  "token": "SW-bmV3YXBpa2V5Zm9ydGVzdGluZ3B1cnBvc2Vz-2024",
  "email": "luke@jedi.com",
  "firstName": "Luke",
  "lastName": "Skywalker",
  "expiresAt": "2025-07-29T01:38:27.915Z"
}
```

**Features:**
- ✅ Automatic cookie management
- ✅ Account lockout protection (5 failed attempts)
- ✅ Security audit logging

### Request Password Reset
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "luke@jedi.com"
}
```

**Response (200 OK):**
```json
{
  "message": "If an account with that email exists, a password reset link has been sent",
  "resetTokenSent": true,
  "expiresInMinutes": 15
}
```

**Security Features:**
- ✅ Always returns success message (prevents email enumeration)
- ✅ Secure 15-minute expiration tokens
- ✅ Rate limiting per email address

### Reset Password
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

**Response (200 OK):**
```json
{
  "message": "Password has been successfully reset",
  "loginRequired": true,
  "note": "Please login with your new password to generate a new API key"
}
```

**Security Features:**
- ✅ Single-use tokens with 15-minute expiration
- ✅ All existing API keys invalidated on password reset
- ✅ Password complexity enforcement

---

## 🔑 ApiKeyController Endpoints

These endpoints manage API key lifecycle. **Authentication required** via cookie or X-API-Key header.

### Get API Key Information
```http
GET /api/apikey/info
```

**Response (200 OK):**
```json
{
  "message": "API key is active and valid",
  "keyStatus": "Active",
  "expiresAt": "2025-07-29T02:05:15.123Z",
  "instructions": "Use your API key in the X-API-Key header for all protected endpoints"
}
```

### Regenerate API Key
```http
POST /api/apikey/regenerate
```

**Response (200 OK):**
```json
{
  "message": "API key regenerated successfully",
  "newApiKey": "SW-bmV3c2VjdXJla2V5Z2VuZXJhdGVkZm9ydXNlcg-2024",
  "expiresAt": "2025-07-29T02:15:33.789Z",
  "warning": "Your previous API key has been revoked. Update your applications with the new key.",
  "note": "New API key has been automatically saved as a cookie for browser access"
}
```

**Features:**
- ✅ Old API key immediately invalidated
- ✅ New cookie automatically set
- ✅ Security audit logging

### Revoke API Key
```http
POST /api/apikey/revoke
```

**Response (200 OK):**
```json
{
  "message": "API key has been successfully revoked",
  "note": "You will need to login again to generate a new API key"
}
```

**Features:**
- ✅ Cookie automatically cleared
- ✅ API key permanently disabled
- ✅ Requires new login for API access

---

## 🚀 StarshipsController Endpoints

Complete CRUD operations for starship management. **Authentication required** - works seamlessly with cookie authentication.

### Get All Starships (Paginated)
```http
GET /api/starships?page=1&pageSize=10&search=falcon&sortBy=name&sortDescending=false
```

**Query Parameters:**
- `page` (optional): Page number (default: 1)
- `pageSize` (optional): Items per page (default: 10, max: 100)
- `search` (optional): Search term for name, model, or manufacturer
- `sortBy` (optional): Sort field (name, model, manufacturer, length)
- `sortDescending` (optional): Sort order (default: false)

**Response (200 OK):**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Millennium Falcon",
      "model": "YT-1300 light freighter",
      "manufacturer": "Corellian Engineering Corporation",
      "costInCredits": "100000",
      "length": "34.37",
      "maxAtmospheringSpeed": "1050",
      "crew": "4",
      "passengers": "6",
      "cargoCapacity": "100000",
      "consumables": "2 months",
      "hyperdriveRating": "0.5",
      "mglt": "75",
      "starshipClass": "Light freighter"
    }
  ],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalPages": 5,
    "totalItems": 47,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### Get Specific Starship
```http
GET /api/starships/1
```

**Response (200 OK):**
```json
{
  "id": 1,
  "name": "Millennium Falcon",
  "model": "YT-1300 light freighter",
  "manufacturer": "Corellian Engineering Corporation",
  "costInCredits": "100000",
  "length": "34.37",
  "maxAtmospheringSpeed": "1050",
  "crew": "4",
  "passengers": "6",
  "cargoCapacity": "100000",
  "consumables": "2 months",
  "hyperdriveRating": "0.5",
  "mglt": "75",
  "starshipClass": "Light freighter",
  "createdAt": "2025-07-29T01:15:30.456Z",
  "updatedAt": "2025-07-29T01:15:30.456Z"
}
```

**Response (404 Not Found):**
```json
{
  "error": "Starship not found",
  "details": "No starship found with ID: 999",
  "timestamp": "2025-07-29T01:30:00Z"
}
```

### Create New Starship
```http
POST /api/starships
Content-Type: application/json

{
  "name": "X-wing Starfighter",
  "model": "T-65 X-wing",
  "manufacturer": "Incom Corporation",
  "costInCredits": "149999",
  "length": "12.5",
  "maxAtmospheringSpeed": "1050",
  "crew": "1",
  "passengers": "0",
  "cargoCapacity": "110",
  "consumables": "1 week",
  "hyperdriveRating": "1.0",
  "mglt": "100",
  "starshipClass": "Starfighter"
}
```

**Response (201 Created):**
```json
{
  "id": 48,
  "name": "X-wing Starfighter",
  "model": "T-65 X-wing",
  "manufacturer": "Incom Corporation",
  "costInCredits": "149999",
  "length": "12.5",
  "maxAtmospheringSpeed": "1050",
  "crew": "1",
  "passengers": "0",
  "cargoCapacity": "110",
  "consumables": "1 week",
  "hyperdriveRating": "1.0",
  "mglt": "100",
  "starshipClass": "Starfighter",
  "createdAt": "2025-07-29T01:45:15.789Z",
  "updatedAt": "2025-07-29T01:45:15.789Z"
}
```

**Validation Rules:**
- `name`: Required, max 100 characters
- `model`: Required, max 100 characters
- `manufacturer`: Required, max 100 characters
- Numeric fields: Must be valid numbers or "unknown"

### Update Starship
```http
PUT /api/starships/48
Content-Type: application/json

{
  "name": "X-wing Starfighter (Updated)",
  "model": "T-65B X-wing",
  "manufacturer": "Incom Corporation",
  "costInCredits": "159999",
  "length": "12.5",
  "maxAtmospheringSpeed": "1050",
  "crew": "1",
  "passengers": "0",
  "cargoCapacity": "110",
  "consumables": "1 week",
  "hyperdriveRating": "1.0",
  "mglt": "100",
  "starshipClass": "Starfighter"
}
```

**Response (200 OK):**
```json
{
  "id": 48,
  "name": "X-wing Starfighter (Updated)",
  "model": "T-65B X-wing",
  "manufacturer": "Incom Corporation",
  "costInCredits": "159999",
  "length": "12.5",
  "maxAtmospheringSpeed": "1050",
  "crew": "1",
  "passengers": "0",
  "cargoCapacity": "110",
  "consumables": "1 week",
  "hyperdriveRating": "1.0",
  "mglt": "100",
  "starshipClass": "Starfighter",
  "createdAt": "2025-07-29T01:45:15.789Z",
  "updatedAt": "2025-07-29T01:50:22.123Z"
}
```

### Delete Starship
```http
DELETE /api/starships/48
```

**Response (204 No Content)**

### Get All Manufacturers
```http
GET /api/starships/manufacturers
```

**Response (200 OK):**
```json
[
  "Corellian Engineering Corporation",
  "Incom Corporation",
  "Sienar Fleet Systems",
  "Kuat Drive Yards",
  "Mon Calamari shipyards"
]
```

### Get All Starship Classes
```http
GET /api/starships/classes
```

**Response (200 OK):**
```json
[
  "Light freighter",
  "Starfighter",
  "Star Destroyer",
  "Assault starfighter",
  "Star Cruiser"
]
```

### Advanced Search
```http
GET /api/starships/search?manufacturer=Corellian&class=freighter&minLength=30&maxLength=50
```

**Query Parameters:**
- `manufacturer`: Filter by manufacturer
- `class`: Filter by starship class
- `minLength`: Minimum length filter
- `maxLength`: Maximum length filter
- `minCrew`: Minimum crew size
- `maxCrew`: Maximum crew size

---

## 🌱 SeedController Endpoints

Database seeding operations to populate your database with external data. **Authentication required**.

### Seed Starships from SWAPI
```http
POST /api/seed/starships
```

**Response (200 OK):**
```json
{
  "message": "Starship seeding completed successfully",
  "totalFetched": 36,
  "newStarships": 15,
  "duplicatesSkipped": 21,
  "processingTime": "2.5 seconds",
  "source": "https://swapi.info/api/starships/"
}
```

**Features:**
- ✅ Fetches all starships from Star Wars API (swapi.info)
- ✅ Automatically prevents duplicates
- ✅ Bulk database operations for performance
- ✅ Comprehensive error handling
- ✅ Progress feedback and statistics

**Error Response (503 Service Unavailable):**
```json
{
  "error": "External API unavailable",
  "details": "Unable to connect to SWAPI. Please try again later.",
  "timestamp": "2025-07-29T01:30:00Z"
}
```

---

## 👑 AdminController Endpoints

Administrative operations with enhanced security. **Authentication + Admin Key required**.

### Get Order 66 Information
```http
GET /api/admin/order-66/info
```

**Response (200 OK):**
```json
{
  "operation": "Order 66 - Database Purge",
  "description": "Permanently delete all user data from the database",
  "requirements": [
    "Valid authentication (cookie or X-API-Key header)",
    "Admin key: FOR-PADME-FOR-LOVE",
    "Exact confirmation phrase: 'Execute Order 66'"
  ],
  "warning": "⚠️ IRREVERSIBLE OPERATION - Permanently deletes all user data",
  "affectedTables": [
    "AspNetUsers",
    "AspNetRoles", 
    "AspNetUserRoles",
    "Starships (user-created only)"
  ],
  "auditTrail": "All operations are logged with user details and timestamps"
}
```

### Execute Order 66 (Database Purge)
```http
DELETE /api/admin/order-66
Content-Type: application/json
X-Admin-Key: FOR-PADME-FOR-LOVE

{
  "confirmation": "Execute Order 66",
  "reason": "Database cleanup for testing environment"
}
```

**Security Requirements:**
- ✅ Valid authentication (cookie or X-API-Key header)
- ✅ Admin key in `X-Admin-Key` header: `FOR-PADME-FOR-LOVE`
- ✅ Exact confirmation phrase: `"Execute Order 66"`
- ✅ Optional reason field for audit trail

**Response (200 OK):**
```json
{
  "message": "Order 66 executed successfully",
  "deletedUsers": 15,
  "deletedRoles": 2,
  "deletedStarships": 8,
  "executedBy": "admin@starwars.com",
  "executedAt": "2025-07-29T01:55:33.456Z",
  "reason": "Database cleanup for testing environment",
  "warning": "All user data has been permanently deleted"
}
```

**Error Response (403 Forbidden):**
```json
{
  "error": "Insufficient permissions",
  "details": "Admin key required for this operation",
  "requiredHeader": "X-Admin-Key: FOR-PADME-FOR-LOVE",
  "timestamp": "2025-07-29T01:30:00Z"
}
```

---

## Usage Examples

### JavaScript/Browser Integration
```javascript
// Automatic cookie authentication after login
const response = await fetch('/api/starships', {
    method: 'GET',
    credentials: 'include'  // Include cookies
});

const starships = await response.json();
console.log(starships);
```

### cURL Examples
```bash
# Login and get API key
curl -X POST "https://localhost:7108/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"luke@jedi.com","password":"Password123!"}' \
  -c cookies.txt

# Use cookie authentication
curl -X GET "https://localhost:7108/api/starships" \
  -b cookies.txt

# Use header authentication
curl -X GET "https://localhost:7108/api/starships" \
  -H "X-API-Key: SW-your-api-key-here"
```

### C# Client Example
```csharp
// Using HttpClient with API key
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-API-Key", "SW-your-api-key-here");

var response = await client.GetAsync("https://localhost:7108/api/starships");
var starships = await response.Content.ReadAsStringAsync();
```

### Python Example
```python
import requests

# Login and get API key
login_response = requests.post('https://localhost:7108/api/auth/login', 
    json={'email': 'luke@jedi.com', 'password': 'Password123!'})

api_key = login_response.json()['token']

# Use API key for requests
headers = {'X-API-Key': api_key}
starships = requests.get('https://localhost:7108/api/starships', headers=headers)
print(starships.json())
```

---

**[← Back to Main README](../README.md)**
