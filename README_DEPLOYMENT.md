# PlanMorph - Complete Deployment Solution

> A comprehensive Docker deployment solution for the PlanMorph architectural design marketplace platform.

## 🚀 Quick Start

### Prerequisites
- Docker 20.10+
- Docker Compose 2.0+
- Git

### 5-Minute Setup

```bash
# 1. Clone repository
git clone https://github.com/your-org/planmorph.git
cd planmorph

# 2. Setup environment
cp .env.example .env
# Edit .env with your values
nano .env

# 3. Start all services
chmod +x deploy.sh
./deploy.sh up

# 4. Access applications
# Frontend: http://localhost:3000
# API: http://localhost:7038
# Admin: http://localhost:8080
```

## 📋 Project Structure

```
planmorph/
├── 🐳 docker-compose.yml          # Main Docker Compose configuration
├── 📄 .env.example                # Environment variables template
├── 🔧 deploy.sh                   # Linux/macOS deployment script
├── 🔧 deploy.ps1                  # Windows PowerShell script
├── 📖 DEPLOYMENT_GUIDE.md         # Comprehensive deployment guide
│
├── 🌐 Frontend
│   ├── planmorph-client/          # Next.js 14 React application
│   ├── planmorph-client/Dockerfile
│   └── planmorph-client/.dockerignore
│
├── ⚙️ Backend
│   ├── PlanMorph.Api/              # .NET 8 REST API
│   ├── PlanMorph.Api/Dockerfile
│   ├── PlanMorph.Admin/            # Blazor Server Admin Panel
│   ├── PlanMorph.Admin/Dockerfile
│   ├── PlanMorph.Application/      # Business logic layer
│   ├── PlanMorph.Core/             # Domain entities
│   └── PlanMorph.Infrastructure/   # Data access layer
│
├── 🗄️ Database
│   └── database/init.sql           # PostgreSQL initialization
│
├── 🔀 Nginx
│   ├── nginx/nginx.conf            # Reverse proxy configuration
│   └── nginx/ssl/                  # SSL certificates (production)
│
└── 🚀 CI/CD
    └── .github/workflows/
        ├── build.yml               # GitHub Actions build workflow
        └── deploy.yml              # GitHub Actions deployment workflow
```

## 🐳 Docker Services

| Service | Port | Technology | Purpose |
|---------|------|-----------|---------|
| **Client** | 3000 | Next.js 14 + React | Web frontend |
| **API** | 7038 | .NET 8 + ASP.NET Core | REST API backend |
| **Admin** | 8080 | .NET 8 + Blazor | Admin dashboard |
| **PostgreSQL** | 5432 | PostgreSQL 16 | Database |
| **Nginx** | 80/443 | Nginx Alpine | Reverse proxy |

## 📖 Documentation

### Deployment Guides

- **[Complete Deployment Guide](./DEPLOYMENT_GUIDE.md)** - Comprehensive guide covering:
  - Development setup
  - Staging environment
  - Production deployment
  - Monitoring & maintenance
  - Troubleshooting
  - Scaling strategies

### Legal & Compliance

- **[Privacy Policy](./PRIVACY_POLICY.md)** - GDPR, CCPA, Kenya DPA compliant
- **[Terms of Service](./TERMS_OF_SERVICE.md)** - Comprehensive legal terms
- **[Email Integration Guide](./LEGAL_EMAIL_INTEGRATION_GUIDE.md)** - Email setup guide

### Architecture Documentation

- **API Documentation** - Swagger/OpenAPI at `http://localhost:7038/swagger`
- **Architecture Overview** - See Docker Compose structure above

## 🚀 Deployment Commands

### Development

```bash
# Start all services
./deploy.sh up

# View logs
./deploy.sh logs

# View specific service logs
./deploy.sh logs api
./deploy.sh logs client

# Stop services
./deploy.sh down

# Restart services
./deploy.sh restart
```

### Production

```bash
# Start with Nginx proxy
./deploy.sh up production

# Check status
./deploy.sh status

# Rebuild images
./deploy.sh rebuild

# Backup database
./deploy.sh backup-db

# Restore database
./deploy.sh restore-db backup_20260211_020000.sql
```

### Database

```bash
# Run migrations
./deploy.sh migrate

# Backup database
./deploy.sh backup-db

# Restore database
./deploy.sh restore-db <backup-file>
```

## ⚙️ Configuration

### Environment Variables

Copy `.env.example` to `.env` and configure:

```env
# Database
POSTGRES_DB=planmorph
POSTGRES_USER=planmorph
POSTGRES_PASSWORD=your_secure_password

# JWT
JWT_SECRET_KEY=your_jwt_secret_key_minimum_32_chars

# Email (SMTP)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password

# Payment (Paystack)
PAYSTACK_SECRET_KEY=sk_test_xxxxx
PAYSTACK_PUBLIC_KEY=pk_test_xxxxx
PAYSTACK_WEBHOOK_SECRET=xxxxx

# File Storage (DigitalOcean Spaces)
DO_SPACES_ACCESS_KEY=xxxxx
DO_SPACES_SECRET_KEY=xxxxx
DO_SPACES_BUCKET_NAME=planmorph-designs
DO_SPACES_REGION=nyc3

# Frontend
NEXT_PUBLIC_API_URL=http://localhost:7038
```

See `.env.example` for all available options.

## 🏥 Health Checks

### API Health Endpoints

```bash
# Basic health check
curl http://localhost:7038/health

# Detailed health check
curl http://localhost:7038/health/detailed

# Database health
curl http://localhost:7038/health/database

# Readiness check (for Kubernetes)
curl http://localhost:7038/health/ready

# Liveness check (for Kubernetes)
curl http://localhost:7038/health/live
```

## 📊 Monitoring

### View Service Status

```bash
./deploy.sh status
```

### Monitor Resource Usage

```bash
# Real-time stats
docker stats

# Check specific container
docker compose ps api
docker compose ps postgres
```

### View Logs

```bash
# All services
docker compose logs -f

# Specific service
docker compose logs -f api
docker compose logs -f postgres
docker compose logs -f client

# Last N lines
docker compose logs --tail=100 api
```

## 🔒 Security Features

- ✅ HTTPS/TLS encryption (production)
- ✅ JWT bearer token authentication
- ✅ Role-based access control (RBAC)
- ✅ Firewall rules configuration
- ✅ SSL certificate management (Let's Encrypt)
- ✅ Database password hashing
- ✅ HMAC-SHA512 webhook signature validation
- ✅ Rate limiting and DDoS protection
- ✅ Security headers (X-Frame-Options, HSTS, etc.)

## 🛠️ Troubleshooting

### Services won't start

```bash
# View logs
docker compose logs

# Check environment file
cat .env

# Verify Docker is running
docker ps
```

### Database connection error

```bash
# Check PostgreSQL is running
docker compose ps postgres

# Check database logs
docker compose logs postgres

# Wait for PostgreSQL to be ready
docker compose exec postgres pg_isready -U planmorph
```

### Port already in use

```bash
# Find what's using port
lsof -i :3000

# Change port in docker-compose.yml or use different port
```

### More troubleshooting

See [DEPLOYMENT_GUIDE.md#troubleshooting](./DEPLOYMENT_GUIDE.md#troubleshooting) for detailed troubleshooting steps.

## 📈 Scaling

### Horizontal Scaling

```bash
# Scale API to 3 instances
docker compose up -d --scale api=3

# Scale client to 2 instances
docker compose up -d --scale client=2
```

### Performance Optimization

- Enable Nginx caching
- Configure database connection pooling
- Implement CDN for static assets
- Use read replicas for database

See [DEPLOYMENT_GUIDE.md#scaling](./DEPLOYMENT_GUIDE.md#scaling) for detailed guidance.

## 🔄 CI/CD Pipeline

### GitHub Actions

The project includes automated workflows:

- **Build & Test** (`.github/workflows/build.yml`)
  - Runs on every push and pull request
  - Tests backend (.NET)
  - Tests frontend (Next.js)
  - Builds Docker images

- **Deployment** (`.github/workflows/deploy.yml`)
  - Runs on push to main branch
  - Deploys to production server
  - Health checks
  - Slack notifications

### Setup CI/CD

Add GitHub Secrets:

```
DEPLOY_SSH_KEY          # SSH private key for deployment
DEPLOY_HOST             # Production server IP/domain
DEPLOY_USER             # SSH user on production server
SLACK_WEBHOOK_URL       # Slack webhook for notifications
```

## 🐳 Docker Compose Profiles

### Development (default)

```bash
docker compose up
```

Services: Client, API, Admin, PostgreSQL

### Production

```bash
docker compose --profile production up
```

Adds: Nginx reverse proxy for SSL/TLS

## 📝 Backup & Recovery

### Database Backup

```bash
# Daily backup
./deploy.sh backup-db

# Scheduled backup (cron)
0 2 * * * cd /opt/planmorph && ./deploy.sh backup-db
```

### Database Recovery

```bash
# List backups
ls -la backups/

# Restore
./deploy.sh restore-db backups/backup_20260211_020000.sql
```

## 🔗 Useful Links

- **Docker Docs:** https://docs.docker.com/
- **Docker Compose:** https://docs.docker.com/compose/
- **Next.js Docs:** https://nextjs.org/docs
- **.NET 8 Docs:** https://learn.microsoft.com/dotnet/
- **PostgreSQL Docs:** https://www.postgresql.org/docs/
- **Nginx Docs:** https://nginx.org/en/docs/

## 💬 Support

### Getting Help

1. Check [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) for detailed documentation
2. Review [Troubleshooting](./DEPLOYMENT_GUIDE.md#troubleshooting) section
3. Check Docker Compose logs: `./deploy.sh logs`
4. Open GitHub issue with:
   - Error logs
   - Steps to reproduce
   - Environment details

### Reporting Issues

When reporting issues, include:

```bash
# Docker version
docker --version

# Docker Compose version
docker compose version

# Environment info
cat .env

# Logs
docker compose logs > logs.txt
```

## 📄 License

See LICENSE file in repository root.

## ✨ Features Included

### Backend
- ✅ REST API with Swagger documentation
- ✅ Entity Framework Core with PostgreSQL
- ✅ JWT authentication & authorization
- ✅ CORS support
- ✅ Global exception handling
- ✅ Health check endpoints
- ✅ Database migrations

### Frontend
- ✅ Next.js 14 with React 19
- ✅ Tailwind CSS styling
- ✅ Responsive design
- ✅ API integration with Axios
- ✅ State management (Zustand)
- ✅ Error handling & toast notifications
- ✅ Authentication flows

### Admin Panel
- ✅ Blazor Server application
- ✅ User management dashboard
- ✅ Design approval workflow
- ✅ Order management
- ✅ Professional review interface

### DevOps
- ✅ Docker & Docker Compose
- ✅ Multi-stage builds
- ✅ Health checks
- ✅ Environment-based configuration
- ✅ Nginx reverse proxy with SSL
- ✅ Database backups
- ✅ Deployment scripts

### Documentation
- ✅ Privacy Policy (GDPR/CCPA compliant)
- ✅ Terms of Service
- ✅ Email integration guide
- ✅ Comprehensive deployment guide
- ✅ This README

## 🎯 Next Steps

1. ✅ Copy `.env.example` to `.env`
2. ✅ Update environment variables with your credentials
3. ✅ Run `./deploy.sh up` to start services
4. ✅ Access applications at respective ports
5. ✅ Review [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md) for production setup

---

**Version:** 1.0
**Last Updated:** February 11, 2026
**Status:** Production Ready ✅

For detailed deployment instructions, see [DEPLOYMENT_GUIDE.md](./DEPLOYMENT_GUIDE.md)
