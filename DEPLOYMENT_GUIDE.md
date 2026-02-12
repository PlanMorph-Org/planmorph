# PlanMorph Complete Deployment Guide

**Version:** 1.0
**Last Updated:** February 11, 2026
**Status:** Production Ready

---

## Table of Contents

1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Quick Start Guide](#quick-start-guide)
4. [Detailed Deployment](#detailed-deployment)
5. [Production Deployment](#production-deployment)
6. [Monitoring & Maintenance](#monitoring--maintenance)
7. [Troubleshooting](#troubleshooting)
8. [Scaling](#scaling)

---

## Overview

### Architecture

PlanMorph consists of **4 main services** deployed as Docker containers:

```
┌─────────────────────────────────────────────────────────────┐
│                   Nginx Reverse Proxy                       │
│                  (Production Only - Port 80/443)            │
└──────────────┬──────────────┬──────────────┬────────────────┘
               │              │              │
        ┌──────▼────────┐ ┌──▼─────────┐ ┌──▼────────────┐
        │  Next.js      │ │  .NET API  │ │  Blazor Admin │
        │  Client       │ │  (Port 80) │ │  (Port 80)    │
        │ (Port 3000)   │ └──┬─────────┘ └───────────────┘
        └────────────────┘   │
                             │
                        ┌────▼─────────┐
                        │  PostgreSQL  │
                        │  Database    │
                        │ (Port 5432)  │
                        └──────────────┘
```

### Services

| Service | Port | Technology | Purpose |
|---------|------|-----------|---------|
| **Client** | 3000 | Next.js 14 + React | Frontend web application |
| **API** | 7038 | .NET 8 + ASP.NET Core | REST API backend |
| **Admin** | 8080 | .NET 8 + Blazor | Admin panel |
| **PostgreSQL** | 5432 | PostgreSQL 16 | Database |
| **Nginx** | 80/443 | Nginx Alpine | Reverse proxy (production) |

---

## Prerequisites

### System Requirements

**Minimum:**
- CPU: 2 cores
- RAM: 4GB
- Storage: 20GB
- OS: Linux, macOS, or Windows (with Docker Desktop)

**Recommended (Production):**
- CPU: 4+ cores
- RAM: 8GB+
- Storage: 50GB+
- OS: Linux (Ubuntu 20.04+, CentOS 8+, Debian 11+)

### Software Requirements

**Required:**
- Docker 20.10+ ([Install Docker](https://docs.docker.com/get-docker/))
- Docker Compose 2.0+ ([Install Compose](https://docs.docker.com/compose/install/))

**Optional:**
- Git (for cloning the repository)
- curl or Postman (for API testing)

### Verify Installation

```bash
# Check Docker version
docker --version
# Expected: Docker version 20.10.0 or higher

# Check Docker Compose version
docker compose version
# Expected: Docker Compose version 2.0.0 or higher

# Test Docker installation
docker run hello-world
```

---

## Quick Start Guide

### Step 1: Clone Repository

```bash
git clone https://github.com/your-org/planmorph.git
cd planmorph
```

### Step 2: Setup Environment

```bash
# Copy environment template
cp .env.example .env

# Edit .env with your configuration
# On Linux/macOS:
nano .env

# On Windows PowerShell:
notepad .env
```

**Minimum required environment variables:**
```env
POSTGRES_PASSWORD=your_secure_password
JWT_SECRET_KEY=your_jwt_secret_key_minimum_32_chars
SMTP_HOST=smtp.gmail.com
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
PAYSTACK_SECRET_KEY=your_paystack_secret_key
PAYSTACK_PUBLIC_KEY=your_paystack_public_key
DO_SPACES_ACCESS_KEY=your_digitalocean_spaces_key
DO_SPACES_SECRET_KEY=your_digitalocean_spaces_secret
```

### Step 3: Start Services

**Linux/macOS:**
```bash
chmod +x deploy.sh
./deploy.sh up
```

**Windows PowerShell:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
.\deploy.ps1 up
```

### Step 4: Verify Services

```bash
# Check if all services are healthy
docker compose ps

# View logs
docker compose logs -f
```

### Step 5: Access Applications

- **Frontend:** http://localhost:3000
- **API:** http://localhost:7038
- **Admin Panel:** http://localhost:8080
- **Database:** localhost:5432

---

## Detailed Deployment

### Development Environment

**Use Case:** Local development with hot reload and debugging

```bash
# Start services
./deploy.sh up

# View logs (follow mode)
./deploy.sh logs

# View logs for specific service
./deploy.sh logs api
./deploy.sh logs client
./deploy.sh logs admin
./deploy.sh logs postgres

# Restart a specific service
docker compose restart api

# Stop all services
./deploy.sh down
```

### Staging Environment

**Use Case:** Pre-production testing with production-like configuration

**1. Create Staging .env file:**

```bash
cp .env.example .env.staging
# Edit .env.staging with staging-specific values
```

**2. Create `docker-compose.staging.yml`:**

```bash
cp docker-compose.yml docker-compose.staging.yml
```

**3. Update staging compose file to use staging .env:**

```yaml
# At the top of docker-compose.staging.yml
env_file:
  - .env.staging
```

**4. Deploy staging environment:**

```bash
docker compose -f docker-compose.staging.yml up -d
```

**5. Verify staging deployment:**

```bash
docker compose -f docker-compose.staging.yml ps
docker compose -f docker-compose.staging.yml logs -f
```

### Production Environment

**Use Case:** Public-facing application with SSL, security hardening, and monitoring

#### Production Deployment Steps:

**1. Prepare Production Server**

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install Docker and Docker Compose
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add user to docker group (optional, for sudo-less commands)
sudo usermod -aG docker $USER
```

**2. Create Production Directory**

```bash
# Create directory structure
mkdir -p /opt/planmorph
cd /opt/planmorph

# Clone or copy repository
git clone https://github.com/your-org/planmorph.git .

# Or if uploading files
scp -r /local/planmorph/* user@server:/opt/planmorph/
```

**3. Setup Production Environment**

```bash
# Copy and edit production .env
cp .env.example .env
nano .env

# Set production values:
# - Strong database password
# - Real JWT secret key
# - Production email credentials (SendGrid/AWS SES recommended)
# - Real Paystack production keys
# - Real DigitalOcean credentials
# - NEXT_PUBLIC_API_URL=https://api.yourdomain.com
```

**4. Setup SSL Certificates**

```bash
# Create SSL directory
mkdir -p nginx/ssl

# Option A: Using Let's Encrypt (Recommended)
sudo certbot certonly --standalone -d yourdomain.com -d www.yourdomain.com

# Copy certificates to nginx directory
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem nginx/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem nginx/ssl/
sudo chown -R 1000:1000 nginx/ssl/

# Option B: Using self-signed certificate (Development only)
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
  -keyout nginx/ssl/privkey.pem \
  -out nginx/ssl/fullchain.pem
```

**5. Configure Nginx for Production**

Update `nginx/nginx.conf`:
- Change `server_name _;` to `server_name yourdomain.com www.yourdomain.com;`
- Uncomment HTTPS server block
- Update certificate paths if needed

**6. Start with Production Profile**

```bash
# Make deployment script executable
chmod +x deploy.sh

# Start with nginx proxy
./deploy.sh up production

# Verify services
./deploy.sh status
```

**7. Setup Auto-Restart on Reboot**

```bash
# Create systemd service file
sudo nano /etc/systemd/system/planmorph.service
```

Add the following:

```ini
[Unit]
Description=PlanMorph Docker Services
After=docker.service
Requires=docker.service

[Service]
Type=oneshot
WorkingDirectory=/opt/planmorph
ExecStart=/bin/bash -c 'docker compose up -d'
ExecStop=/bin/bash -c 'docker compose down'
RemainAfterExit=yes

[Install]
WantedBy=multi-user.target
```

Enable and start:

```bash
sudo systemctl daemon-reload
sudo systemctl enable planmorph.service
sudo systemctl start planmorph.service
```

**8. Setup Firewall Rules**

```bash
# For Ubuntu/Debian
sudo ufw allow 22/tcp   # SSH
sudo ufw allow 80/tcp   # HTTP
sudo ufw allow 443/tcp  # HTTPS
sudo ufw enable
```

---

## Production Deployment

### Production Checklist

**Pre-Deployment:**
- [ ] All environment variables configured with production values
- [ ] SSL certificates obtained and configured
- [ ] Database backups configured
- [ ] Email service tested
- [ ] Paystack payment gateway tested
- [ ] DigitalOcean Spaces configured
- [ ] Domain DNS records pointing to server
- [ ] Firewall rules configured

**Deployment:**
- [ ] Run pre-deployment tests
- [ ] Build and test Docker images
- [ ] Deploy with production profile
- [ ] Verify all services are healthy
- [ ] Run database migrations
- [ ] Test all critical workflows
- [ ] Monitor logs for errors

**Post-Deployment:**
- [ ] Setup monitoring and alerts
- [ ] Configure automated backups
- [ ] Document deployment details
- [ ] Setup SSL certificate renewal
- [ ] Configure log aggregation

### Database Backup Strategy

**Daily Backups:**

```bash
# Manual backup
./deploy.sh backup-db

# Automated daily backup (cron)
0 2 * * * cd /opt/planmorph && ./deploy.sh backup-db >> /var/log/planmorph-backup.log 2>&1
```

**Keep Backups:**

```bash
# Keep daily backups for 7 days
# Keep weekly backups for 4 weeks
# Keep monthly backups for 1 year

# Setup backup retention script
mkdir -p backups
```

**Restore from Backup:**

```bash
# List available backups
ls -la backups/

# Restore specific backup
./deploy.sh restore-db backups/backup_20260211_020000.sql
```

---

## Monitoring & Maintenance

### Service Health Checks

```bash
# Check health status
./deploy.sh status

# Check Docker stats (CPU, memory, etc.)
docker stats

# Check specific service health
docker compose ps api
docker compose ps postgres
```

### View Logs

```bash
# All logs
docker compose logs

# Follow logs in real-time
docker compose logs -f

# Last 100 lines
docker compose logs --tail=100

# Logs for specific service
docker compose logs -f api
docker compose logs -f postgres
docker compose logs -f client
docker compose logs -f admin

# Export logs to file
docker compose logs > logs_backup.txt
```

### Performance Monitoring

```bash
# Monitor resource usage
docker stats --no-stream

# Check database size
docker compose exec postgres du -sh /var/lib/postgresql/data

# Check disk space
df -h
```

### Updates & Maintenance

**Update Docker Images:**

```bash
# Build new images with latest code
./deploy.sh rebuild

# Or manually
docker compose build --no-cache
docker compose up -d
```

**Database Maintenance:**

```bash
# Connect to database
docker compose exec postgres psql -U planmorph planmorph

# Inside postgres:
-- Check database size
SELECT pg_size_pretty(pg_database_size('planmorph'));

-- Vacuum database (maintenance)
VACUUM ANALYZE;

-- List tables
\dt

-- Exit
\q
```

**Certificate Renewal (Let's Encrypt):**

```bash
# Automatic renewal with certbot timer (should be automatic)
sudo certbot renew --dry-run

# Manual renewal
sudo certbot renew

# Copy renewed certificates
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem /opt/planmorph/nginx/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem /opt/planmorph/nginx/ssl/
sudo chown -R 1000:1000 /opt/planmorph/nginx/ssl/

# Restart nginx
docker compose restart nginx
```

---

## Troubleshooting

### Common Issues

**1. Services won't start**

```bash
# Check logs
docker compose logs

# Verify .env file exists and is valid
cat .env

# Ensure all required environment variables are set
grep -E "(POSTGRES_PASSWORD|JWT_SECRET|PAYSTACK)" .env

# Rebuild containers
./deploy.sh rebuild
```

**2. Database connection errors**

```bash
# Check if postgres is running
docker compose ps postgres

# Check postgres logs
docker compose logs postgres

# Verify connection string in .env
grep "POSTGRES" .env

# Restart postgres
docker compose restart postgres
```

**3. API won't connect to database**

```bash
# Wait for postgres to be ready
docker compose exec postgres pg_isready -U planmorph

# Run migrations
./deploy.sh migrate

# Restart API
docker compose restart api
```

**4. Next.js client not loading**

```bash
# Check next.config.ts has output: 'standalone'
cat planmorph-client/next.config.ts

# Check NEXT_PUBLIC_API_URL is set correctly
grep "NEXT_PUBLIC_API_URL" .env

# Rebuild client
docker compose build --no-cache client
docker compose restart client
```

**5. Upload files not working (DigitalOcean Spaces)**

```bash
# Verify credentials
grep "DO_SPACES" .env

# Test S3 connection
docker compose exec api bash
# Run test inside container...

# Check bucket exists and is accessible
# Use AWS CLI or DigitalOcean console
```

**6. Email not sending**

```bash
# Check SMTP configuration
grep "SMTP" .env

# View email logs
docker compose logs api | grep -i email

# Test SMTP connection
# Replace with your values:
telnet smtp.gmail.com 587
```

**7. High memory usage**

```bash
# Check what's using memory
docker stats

# Limit container memory in docker-compose.yml
# Add to service:
# mem_limit: 512m
# memswap_limit: 1g

# Restart with memory limits
./deploy.sh rebuild
```

**8. Port already in use**

```bash
# Check what's using the port
lsof -i :3000  # Check port 3000
lsof -i :7038  # Check port 7038
lsof -i :5432  # Check port 5432

# Or on Windows:
netstat -ano | findstr :3000

# Kill process or change port in docker-compose.yml
```

### Debug Mode

```bash
# Enable verbose logging
export DEBUG=*
docker compose up

# Build images with logs
docker compose build --progress=plain --no-cache

# Inspect running container
docker exec -it planmorph-api bash
docker exec -it planmorph-postgres psql -U planmorph planmorph
```

---

## Scaling

### Horizontal Scaling

For high traffic, scale to multiple instances:

```bash
# Scale API service to 3 instances
docker compose up -d --scale api=3

# Scale Client service
docker compose up -d --scale client=2
```

### Load Balancing

Update `nginx/nginx.conf` to load balance:

```nginx
upstream api_backend {
    server api:80;
    server api_2:80;
    server api_3:80;
}
```

### Performance Optimization

**1. Enable Caching**

```nginx
# Add to nginx.conf
proxy_cache_path /var/cache/nginx levels=1:2 keys_zone=api_cache:10m;

location /api/ {
    proxy_cache api_cache;
    proxy_cache_valid 200 10m;
}
```

**2. Enable Compression**

Already enabled in `nginx/nginx.conf` with gzip

**3. Database Connection Pooling**

Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Max Pool Size=20;..."
  }
}
```

**4. API Rate Limiting**

Already configured in `nginx/nginx.conf`:
```nginx
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=100r/m;
```

---

## Deployment Summary

### Deployment Commands Quick Reference

```bash
# Development
./deploy.sh up

# Staging
docker compose -f docker-compose.staging.yml up -d

# Production
./deploy.sh up production

# Stop services
./deploy.sh down

# View status
./deploy.sh status

# View logs
./deploy.sh logs [service]

# Backup database
./deploy.sh backup-db

# Restore database
./deploy.sh restore-db backup.sql

# Rebuild
./deploy.sh rebuild

# Clean everything
./deploy.sh clean
```

### File Structure

```
planmorph/
├── docker-compose.yml           # Main compose file
├── .env.example                 # Environment template
├── .env                         # Environment variables (not in git)
├── deploy.sh                    # Linux/macOS deployment script
├── deploy.ps1                   # Windows deployment script
├── .dockerignore                # Files to exclude from Docker builds
├── nginx/
│   └── nginx.conf              # Nginx reverse proxy config
├── PlanMorph.Api/
│   └── Dockerfile              # API Dockerfile
├── PlanMorph.Admin/
│   └── Dockerfile              # Admin Dockerfile
└── planmorph-client/
    ├── Dockerfile              # Client Dockerfile
    ├── next.config.ts          # Next.js config (updated)
    └── .dockerignore           # Client-specific ignore
```

---

## Support & Documentation

**For Issues:**
- Check logs: `./deploy.sh logs`
- Review troubleshooting section above
- Check Docker Compose documentation: https://docs.docker.com/compose/

**External Resources:**
- Docker Docs: https://docs.docker.com/
- Docker Compose Docs: https://docs.docker.com/compose/
- .NET 8 Docs: https://learn.microsoft.com/en-us/dotnet/
- Next.js Docs: https://nextjs.org/docs
- Nginx Docs: https://nginx.org/en/docs/
- PostgreSQL Docs: https://www.postgresql.org/docs/

---

**End of Deployment Guide**

**Version:** 1.0
**Last Updated:** February 11, 2026
**Next Review:** August 11, 2026
