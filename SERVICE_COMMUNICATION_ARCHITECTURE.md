# PlanMorph - Service Communication Architecture

**Date:** February 12, 2026
**Status:** Architecture Documentation

---

## 🏗️ System Architecture Overview

Your PlanMorph application has **3 independent services + 1 shared database**:

```
┌─────────────────────────────────────────────────────────────────┐
│                      EXTERNAL USERS                              │
│  (Browser, Mobile App, API Clients)                              │
└──────────────────────────────┬──────────────────────────────────┘
                               │ HTTPS Requests
                               ▼
        ┌──────────────────────────────────────────────┐
        │   Nginx Reverse Proxy (App Platform/Docker)  │
        │   ✅ SSL/TLS Termination                      │
        │   ✅ Request Routing                          │
        │   ✅ Load Balancing                           │
        └──────────┬────────────────┬──────────────────┘
                   │                │
        ┌──────────┴─┐         ┌────┴──────────┐
        │ Route: /   │         │ Route: /api   │
        │ Route: /   │         │ Route: /admin │
        └──────┬─────┘         └────┬──────────┘
               │                    │
        ┌──────▼────────┐  ┌────────▼────────┐
        │  Client       │  │  API + Admin    │
        │  Next.js      │  │  .NET 8 REST    │
        │  Port: 3000   │  │  Port: 80       │
        │               │  │                 │
        │ -HTML/CSS     │  │ -Controllers    │
        │ -JavaScript   │  │ -Business Logic │
        │ -React Comp   │  │ -DB Services    │
        └───────┬───────┘  └────────┬────────┘
                │   HTTP/REST       │
                │   API Calls       │
                └────────┬──────────┘
                         │
                         │ Database Connections
                         ▼
                ┌─────────────────────┐
                │  PostgreSQL 16      │
                │  Shared Database    │
                │                     │
                │ ✅ All services     │
                │    read/write here  │
                └─────────────────────┘
```

---

## 📡 Communication Patterns

### 1. **Client ↔ API Communication**

**Frontend (Next.js) → API (.NET)**

The Next.js client makes HTTP REST API calls to the backend:

```typescript
// planmorph-client/lib/api.ts
const BASE_URL = process.env.NEXT_PUBLIC_API_URL;

export async function getDesigns() {
  const response = await fetch(`${BASE_URL}/api/designs`, {
    method: 'GET',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  return response.json();
}

export async function createOrder(orderData) {
  const response = await fetch(`${BASE_URL}/api/orders`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify(orderData)
  });
  return response.json();
}
```

**Configuration (from app.yaml):**
```yaml
envs:
- key: NEXT_PUBLIC_API_URL
  scope: RUN_TIME
  value: ${NEXT_PUBLIC_API_URL}  # Set to: https://yourdomain.com or https://api.yourdomain.com
```

**What happens:**
1. Browser loads Next.js app from `https://yourdomain.com`
2. App reads `NEXT_PUBLIC_API_URL` environment variable
3. App makes fetch/axios calls to `${NEXT_PUBLIC_API_URL}/api/*`
4. Nginx reverse proxy routes `/api/*` to the API service
5. API processes request and returns JSON response
6. Client renders the response

---

### 2. **Admin ↔ API Communication**

**Admin Panel (Blazor) → API (.NET)**

The Blazor admin server also calls the same API:

```csharp
// PlanMorph.Admin/Services/ApiService.cs
public class ApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;

    public ApiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        var apiBaseUrl = config["ApiSettings:BaseUrl"];
        _httpClient.BaseAddress = new Uri(apiBaseUrl);
    }

    public async Task<List<OrderDto>> GetOrdersAsync()
    {
        var response = await _httpClient.GetAsync("/api/orders");
        response.EnsureSuccessStatusCode();
        var jsonContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<OrderDto>>(jsonContent);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(int orderId, string status)
    {
        var response = await _httpClient.PutAsync($"/api/orders/{orderId}/status",
            new StringContent(JsonSerializer.Serialize(new { status })));
        response.EnsureSuccessStatusCode();
        var jsonContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<OrderDto>(jsonContent);
    }
}
```

**Configuration (from app.yaml):**
```yaml
envs:
- key: ApiSettings__BaseUrl
  scope: RUN_TIME
  value: ${API_BASE_URL}  # Set to: https://yourdomain.com or http://api (internal)
```

**What happens:**
1. Admin panel requests order data at `${API_BASE_URL}/api/orders`
2. In development: `http://api:80` (Docker hostname)
3. In production: `https://yourdomain.com/api` (through Nginx)
4. API service processes request
5. Blazor renders data in admin dashboard

---

### 3. **Both Services Reading/Writing to Database**

**API & Admin ↔ PostgreSQL Database**

Both services connect to the same PostgreSQL database:

```csharp
// Connection string from app.yaml
// Format: ${db.username}:${db.password}@${db.host}:${db.port}/${db.name}

// Example values in Docker:
// postgres://planmorph:password@postgres:5432/planmorph

// Example values in App Platform:
// postgres://planmorph:password@db-1234.ondigitalocean.com:25060/planmorph
```

**In app.yaml for API:**
```yaml
- key: ConnectionStrings__DefaultConnection
  scope: RUN_TIME
  value: ${db.username}:${db.password}@${db.host}:${db.port}/${db.name}
```

**In app.yaml for Admin:**
```yaml
- key: ConnectionStrings__DefaultConnection
  scope: RUN_TIME
  value: ${db.username}:${db.password}@${db.host}:${db.port}/${db.name}
```

**What happens:**
1. Both services inject `DbContext` with connection string
2. API reads/writes Orders, Designs, Users data
3. Admin reads/writes same database
4. Changes in API appear immediately in Admin
5. PostgreSQL handles concurrent connections

---

## 🌐 Environment-Specific Communication

### Development (Docker Compose)

```
Client (localhost:3000)
    ↓ http://localhost:7038/api
API (localhost:7038)
    ↓ postgres://postgres:password@postgres:5432/planmorph
Database (localhost container)

Admin (localhost:8080)
    ↓ http://api:80 (Docker DNS resolution)
API (port 80 in container)
```

**Docker Compose networking:**
- Services connect by container name (hostname)
- `api` service is accessible as `http://api:80` from other containers
- All containers on same Docker network (`planmorph_network`)

---

### Production - App Platform

```
Browser → https://yourdomain.com
            ↓ (Routing at Nginx)
            ├→ / → Client (Next.js)
            ├→ /api → API (port 80)
            └→ /admin → Admin (port 80)

Client JavaScript:
    → fetch(`${NEXT_PUBLIC_API_URL}/api/orders`)
    → NEXT_PUBLIC_API_URL = `https://yourdomain.com` or `https://api.yourdomain.com`
    → Nginx routes to API service

Admin Blazor:
    → _httpClient.GetAsync("/api/orders")
    → ApiSettings__BaseUrl = `https://yourdomain.com` or `http://api` (internal)
    → If internal: Docker DNS or App Platform internal routing
    → If external: Nginx routes to API service

Both → Database at:
    → postgres://user:pass@db-xxx.ondigitalocean.com:25060/planmorph
```

---

## 🔌 Required Configuration For Communication

### Configuration Variables Needed

**For Client to Reach API:**
```env
NEXT_PUBLIC_API_URL=https://yourdomain.com
# OR
NEXT_PUBLIC_API_URL=https://api.yourdomain.com
```

**For Admin to Reach API:**
```env
API_BASE_URL=https://yourdomain.com
# OR (internal Docker/App Platform)
API_BASE_URL=http://api:80
```

**For Both to Reach Database:**
```env
# Automatically provided by App Platform
# Manual in Docker Compose:
ConnectionStrings__DefaultConnection=postgres://planmorph:password@postgres:5432/planmorph
```

---

## 🔐 Authentication & Security Between Services

### Token-Based Communication

All API calls use JWT Bearer tokens:

```typescript
// Client makes authenticated request
const token = localStorage.getItem('authToken');
const response = await fetch(`${NEXT_PUBLIC_API_URL}/api/orders`, {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});
```

```csharp
// Admin makes authenticated request
var response = await _httpClient.GetAsync("/api/orders");
// HttpClient automatically includes cookie/token from previous login
```

**Shared Security:**
- Same JWT secret used by API: `JWT_SECRET_KEY`
- Client and Admin both get tokens from API's `/login` endpoint
- Admin can make requests to any API endpoint with its auth token
- Client can make requests to any API endpoint with its auth token

---

## 📊 Data Flow Examples

### Example 1: User Creates an Order

```
1. User clicks "Order Design" in Client (Next.js)
   └─ Client sends: POST /api/orders { designId, quantity, ... }

2. Nginx routes /api → API service
   └─ API receives request

3. API creates Order record in PostgreSQL
   └─ Writes to orders table

4. API sends email via SMTP
   └─ Uses SMTP credentials from environment

5. API returns { orderId, status, ... } to Client
   └─ Client shows success message

6. Admin checks dashboard
   └─ Admin makes: GET /api/orders
   └─ API queries same PostgreSQL database
   └─ Admin sees the new order immediately

7. Admin updates order status
   └─ Admin sends: PUT /api/orders/{id}/status
   └─ API updates database
   └─ Client polls or receives WebSocket update
   └─ Both show new status
```

### Example 2: Admin Assigns Contractor

```
1. Admin clicks "Assign Contractor" in Blazor Admin
   └─ Admin sends: PUT /api/orders/{id}/contractor { contractorId }

2. Nginx routes /admin → Admin service
   └─ Admin Blazor component makes internal call
   └─ Admin HttpClient calls API via ApiSettings__BaseUrl

3. API receives request, updates database
   └─ Updates construction_contracts table
   └─ Triggers email notification

4. API sends email to Contractor
   └─ Uses email service with SMTP config

5. Email contains link like: yourdomain.com/project/123
   └─ Opens Client app in browser

6. Contractor logs in to Client
   └─ Client authenticates via API
   └─ Client shows assigned project

7. Real-time sync (optional - can add WebSockets)
   └─ Client WebSocket connects to API
   └─ Admin updates trigger notifications
   └─ Client receives updates in real-time
```

---

## 🔄 Service Discovery

### In Docker Compose

Services discover each other by **container names**:

```yaml
services:
  api:
    container_name: planmorph-api
    ports:
      - "7038:80"

  client:
    container_name: planmorph-client
    environment:
      NEXT_PUBLIC_API_URL: http://api:80

  admin:
    container_name: planmorph-admin
    environment:
      API_BASE_URL: http://api:80

  postgres:
    container_name: planmorph-postgres
```

**How it works:**
- Docker's internal DNS resolves `api` → IP of `planmorph-api` container
- `postgres` hostname → IP of `planmorph-postgres` container
- All containers on `planmorph_default` network

---

### In App Platform

Services discover each other via **internal networking**:

```yaml
routes:
  - path: /api
    component:
      name: api

  - path: /
    component:
      name: client

  - path: /admin
    component:
      name: admin
```

**How it works:**
1. **Internal (service-to-service):**
   - Admin → API: `http://api:80` (internal routing)
   - Both → Database: Direct PostgreSQL connection

2. **External (browser-to-service):**
   - Client: `https://yourdomain.com → /`
   - API: `https://yourdomain.com/api → /api`
   - Admin: `https://yourdomain.com/admin → /admin`

---

## ⚙️ Configuration Best Practices

### For Development

```env
# .env.development
NEXT_PUBLIC_API_URL=http://localhost:7038
API_BASE_URL=http://localhost:7038
ConnectionStrings__DefaultConnection=postgres://postgres:password@localhost:5432/planmorph
```

### For Staging

```env
# Set in App Platform console
NEXT_PUBLIC_API_URL=https://staging.yourdomain.com
API_BASE_URL=https://staging.yourdomain.com
ConnectionStrings__DefaultConnection=postgres://user:pass@staging-db.ondigitalocean.com:25060/planmorph
```

### For Production

```env
# Set in App Platform console
NEXT_PUBLIC_API_URL=https://yourdomain.com
# OR for separate subdomain
NEXT_PUBLIC_API_URL=https://api.yourdomain.com

API_BASE_URL=https://yourdomain.com
# OR for internal communication
API_BASE_URL=http://api  # App Platform internal

ConnectionStrings__DefaultConnection=postgres://user:pass@prod-db.ondigitalocean.com:25060/planmorph
```

---

## 🚀 Deployment Communication Summary

| Component | Reaches | Via | Protocol | Config |
|-----------|---------|-----|----------|--------|
| **Client (Browser)** | API | HTTPS | REST + JWT | `NEXT_PUBLIC_API_URL` |
| **Client (Browser)** | Admin | HTTPS | HTML/Blazor | Direct URL |
| **Admin Panel** | API | HTTP/HTTPS | REST + JWT | `API_BASE_URL` |
| **API** | Database | Internal | PostgreSQL | `ConnectionStrings__DefaultConnection` |
| **Admin** | Database | Internal | PostgreSQL | `ConnectionStrings__DefaultConnection` |
| **API/Admin** | SMTP | HTTPS | SMTP | `SMTP_HOST`, `SMTP_PORT`, etc. |
| **API/Admin** | DO Spaces | HTTPS | S3 API | `DO_SPACES_*` |
| **API** | Paystack | HTTPS | REST API | `PAYSTACK_SECRET_KEY` |

---

## 🔧 Troubleshooting Communication Issues

### "Client can't connect to API"

**Check:**
1. Verify `NEXT_PUBLIC_API_URL` is set correctly
2. Verify CORS is enabled on API (Nginx config)
3. Check browser console for network errors
4. Verify API is responding: `curl https://yourdomain.com/api/health`

**Solution:**
- Rebuild and redeploy client
- Check Nginx configuration for CORS headers
- Verify API container is running

---

### "Admin can't reach API"

**Check:**
1. Verify `API_BASE_URL` environment variable
2. For Docker: check container network (should be on same network)
3. For App Platform: verify internal routing
4. Check Admin logs: `docker-compose logs admin`

**Solution:**
```bash
# In Docker, test connectivity
docker-compose exec admin curl http://api:80/health

# Should return: {"status":"healthy"}
```

---

### "Database connection failed"

**Check:**
1. Verify `ConnectionStrings__DefaultConnection`
2. Check database credentials are correct
3. Verify host/port is accessible

**For App Platform:**
```yaml
ConnectionStrings__DefaultConnection: ${db.username}:${db.password}@${db.host}:${db.port}/${db.name}

# Should resolve to something like:
# postgres://user:pass@db-123.ondigitalocean.com:25060/planmorph
```

**For Docker:**
```bash
# Test connection
docker-compose exec api psql -h postgres -U planmorph -d planmorph -c "SELECT 1"
```

---

## Summary

**Communication Flow:**

```
┌─ Client (Next.js)
│   └─ HTTP REST calls
│      └─ ${NEXT_PUBLIC_API_URL}/api/*
│
├─ Admin (Blazor)
│   └─ HTTP REST calls
│      └─ ${API_BASE_URL}/api/*
│
├─ All Services
│   └─ Database connections
│      └─ PostgreSQL 16
│
└─ Both Connect To
    └─ Shared Database
        └─ Orders, Designs, Users, etc.
```

**All communication is:**
- ✅ HTTP/HTTPS REST APIs
- ✅ JWT Token-based authentication
- ✅ Environment variable configuration
- ✅ Via shared PostgreSQL database
- ✅ Through Nginx reverse proxy (production)

This architecture ensures:
- **Separation of Concerns**: Each service has a specific role
- **Scalability**: Services can be scaled independently
- **Flexibility**: Can add/remove/update services
- **Security**: Isolated containers, JWT auth, encrypted connections

