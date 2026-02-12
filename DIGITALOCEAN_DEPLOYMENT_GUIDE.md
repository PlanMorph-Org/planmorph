# PlanMorph - DigitalOcean Deployment Guide

**Date:** February 11, 2026
**Updated:** Step-by-Step DigitalOcean Deployment
**Difficulty:** Moderate (30-45 minutes)

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Step 1: Create DigitalOcean Account](#step-1-create-digitalocean-account)
3. [Step 2: Create a Droplet](#step-2-create-a-droplet)
4. [Step 3: Connect to Your Droplet](#step-3-connect-to-your-droplet)
5. [Step 4: Install Docker & Docker Compose](#step-4-install-docker--docker-compose)
6. [Step 5: Clone Repository](#step-5-clone-repository)
7. [Step 6: Configure Environment](#step-6-configure-environment)
8. [Step 7: Setup DigitalOcean Spaces](#step-7-setup-digitalocean-spaces)
9. [Step 8: Configure Domain & DNS](#step-8-configure-domain--dns)
10. [Step 9: Setup SSL Certificates](#step-9-setup-ssl-certificates)
11. [Step 10: Deploy Application](#step-10-deploy-application)
12. [Step 11: Verify Deployment](#step-11-verify-deployment)
13. [Step 12: Setup Monitoring](#step-12-setup-monitoring)
14. [Troubleshooting](#troubleshooting)

---

## Prerequisites

You will need:
- ✅ DigitalOcean account (sign up at https://www.digitalocean.com)
- ✅ A domain name (can use any registrar)
- ✅ SSH client (built into macOS/Linux, use PuTTY on Windows)
- ✅ Text editor for configuration files
- ✅ Email credentials (Gmail, SendGrid, AWS SES, etc.)
- ✅ Paystack account with API keys
- ✅ DigitalOcean Spaces API keys

---

## Step 1: Create DigitalOcean Account

### 1.1 Sign Up

1. Go to https://www.digitalocean.com
2. Click **Sign Up**
3. Use email or GitHub account
4. Verify email address
5. Enter billing information

### 1.2 Add SSH Key (Recommended)

1. Go to **Settings → Security → SSH Keys**
2. Click **Add SSH Key**
3. On your local machine, generate SSH key:

```bash
# macOS/Linux
ssh-keygen -t rsa -b 4096 -f ~/.ssh/digitalocean_key

# Windows (Git Bash)
ssh-keygen -t rsa -b 4096 -f C:\Users\YourUsername\.ssh\digitalocean_key
```

4. Copy public key content:

```bash
# macOS/Linux
cat ~/.ssh/digitalocean_key.pub

# Windows (Git Bash)
cat C:\Users\YourUsername\.ssh\digitalocean_key.pub
```

5. Paste into DigitalOcean SSH Keys section
6. Name it "PlanMorph Production Key"
7. Click **Add SSH Key**

---

## Step 2: Create a Droplet

### 2.1 Create New Droplet

1. Click **Create → Droplet**
2. Choose configuration:

**Basic Configuration:**

| Setting | Value |
|---------|-------|
| Region | Choose closest to your users |
| OS | Ubuntu 22.04 LTS |
| Droplet Type | Basic (Shared CPU) |
| Size | $12/month (2GB RAM, 2vCPU, 50GB SSD) |

**Recommended Setup:**

```
For production: Choose $24/month (4GB RAM, 2vCPU, 80GB SSD)
For development: Choose $6/month (1GB RAM, 1vCPU, 25GB SSD)
```

3. Click **Create Droplet**
4. Wait for droplet to be created (2-3 minutes)
5. Note the **IP Address** (you'll need this for DNS)

### 2.2 Firewall Setup (Optional but Recommended)

1. Go to **Create → Firewall**
2. Configure inbound rules:

| Rule | Protocol | Port | Source |
|------|----------|------|--------|
| HTTP | TCP | 80 | All IPv4 |
| HTTPS | TCP | 443 | All IPv4 |
| SSH | TCP | 22 | Your IP |

3. Select your droplet
4. Click **Create Firewall**

---

## Step 3: Connect to Your Droplet

### 3.1 Via SSH (Linux/macOS)

```bash
# Connect using SSH key
ssh -i ~/.ssh/digitalocean_key root@YOUR_DROPLET_IP

# If using password auth (not recommended)
ssh root@YOUR_DROPLET_IP
# Enter password sent via email
```

### 3.2 Via SSH (Windows - PuTTY)

1. Download PuTTY: https://www.putty.org/
2. Install PuTTY and PuTTYgen
3. Convert SSH key:
   - Open PuTTYgen
   - Click **Load** → select `digitalocean_key`
   - Click **Save private key** → save as `.ppk`
4. Open PuTTY:
   - Hostname: `YOUR_DROPLET_IP`
   - Port: 22
   - SSH → Auth → Private key file: select `.ppk`
   - Click **Open**

### 3.3 Via DigitalOcean Console (Easiest)

1. Go to your Droplet in DigitalOcean console
2. Click **Console** tab
3. Note: Username is `root` and password was emailed to you

---

## Step 4: Install Docker & Docker Compose

Once connected to your Droplet:

### 4.1 Update System

```bash
# Update package lists
sudo apt update

# Upgrade packages
sudo apt upgrade -y
```

### 4.2 Install Docker

```bash
# Download Docker installer
curl -fsSL https://get.docker.com -o get-docker.sh

# Run installer
sudo sh get-docker.sh

# Add your user to docker group
sudo usermod -aG docker root

# Verify Docker installation
docker --version
docker run hello-world
```

### 4.3 Install Docker Compose

```bash
# Download Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose

# Make it executable
sudo chmod +x /usr/local/bin/docker-compose

# Verify installation
docker compose version
```

### 4.4 Setup Auto-Start (Important!)

```bash
# Enable Docker to start on reboot
sudo systemctl enable docker
sudo systemctl start docker
```

---

## Step 5: Clone Repository

### 5.1 Create Application Directory

```bash
# Create directory
sudo mkdir -p /opt/planmorph
cd /opt/planmorph

# Set permissions
sudo chown -R root:root /opt/planmorph
sudo chmod -R 755 /opt/planmorph
```

### 5.2 Clone Repository

```bash
# Clone PlanMorph repository
git clone https://github.com/your-org/planmorph.git .

# Verify files were cloned
ls -la
```

**Expected files:**
- docker-compose.yml ✓
- deploy.sh ✓
- .env.example ✓
- Dockerfiles ✓
- nginx/ directory ✓

### 5.3 Make Scripts Executable

```bash
chmod +x deploy.sh
chmod +x deploy.ps1
```

---

## Step 6: Configure Environment

### 6.1 Create .env File

```bash
# Copy template
cp .env.example .env

# Edit the file
nano .env
```

### 6.2 Required Configuration Values

Fill in the following in your `.env` file:

```env
# ========================================
# Database Configuration
# ========================================
POSTGRES_DB=planmorph
POSTGRES_USER=planmorph
POSTGRES_PASSWORD=your_very_secure_random_password_here_at_least_32_chars

# ========================================
# JWT Authentication
# ========================================
# Generate with: openssl rand -base64 32
JWT_SECRET_KEY=your_generated_jwt_secret_key_here_minimum_32_chars

# ========================================
# Email Configuration (SMTP)
# ========================================
# Option 1: Gmail (recommended for testing)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-gmail-app-password  # Generate at myaccount.google.com/apppasswords

# Option 2: SendGrid (recommended for production)
# SMTP_HOST=smtp.sendgrid.net
# SMTP_PORT=587
# SMTP_USERNAME=apikey
# SMTP_PASSWORD=SG.your-sendgrid-api-key

FROM_EMAIL=noreply@yourdomain.com
FROM_NAME=PlanMorph

# ========================================
# Paystack Payment Gateway
# ========================================
# Get from: https://dashboard.paystack.com/settings/developer
PAYSTACK_SECRET_KEY=sk_test_your_paystack_secret_key
PAYSTACK_PUBLIC_KEY=pk_test_your_paystack_public_key
PAYSTACK_WEBHOOK_SECRET=your_paystack_webhook_secret

# For production (use live keys):
# PAYSTACK_SECRET_KEY=sk_live_xxxxx
# PAYSTACK_PUBLIC_KEY=pk_live_xxxxx

# ========================================
# DigitalOcean Spaces (File Storage)
# ========================================
# Get from: https://cloud.digitalocean.com/account/api/tokens
DO_SPACES_ACCESS_KEY=your_do_spaces_access_key
DO_SPACES_SECRET_KEY=your_do_spaces_secret_key
DO_SPACES_BUCKET_NAME=planmorph-designs
DO_SPACES_REGION=nyc3
DO_SPACES_ENDPOINT=https://nyc3.digitaloceanspaces.com

# ========================================
# Frontend API URL
# ========================================
# For local: http://localhost:7038
# For production: https://yourdomain.com or https://api.yourdomain.com
NEXT_PUBLIC_API_URL=https://api.yourdomain.com
```

### 6.3 Save Configuration

```bash
# In nano editor: Press Ctrl+X → Y → Enter
# File is saved
```

### 6.4 Verify Configuration

```bash
# Check file was saved
cat .env

# Make sure it's not world-readable (security)
chmod 600 .env

# Verify
ls -la .env
```

---

## Step 7: Setup DigitalOcean Spaces

### 7.1 Create Spaces Bucket

1. Go to DigitalOcean console
2. Click **Create → Spaces**
3. Configure:
   - Name: `planmorph-designs`
   - Region: Same as your droplet (e.g., nyc3)
   - ACL: Restrict File to private by default
   - Click **Create Space**

### 7.2 Generate Access Tokens

1. Go to **Account → API Tokens**
2. Click **Generate New Token**
3. Name: `PlanMorph App`
4. Scopes: **Spaces** (read + write)
5. Click **Generate Token**
6. Copy the credentials provided

### 7.3 Get Credentials

```bash
# Display current .env
cat .env | grep "DO_SPACES"

# Update if needed
nano .env
# Update DO_SPACES_ACCESS_KEY and DO_SPACES_SECRET_KEY
```

---

## Step 8: Configure Domain & DNS

### 8.1 Point Domain to DigitalOcean

If your domain is registered elsewhere:

1. Go to your domain registrar (GoDaddy, Namecheap, etc.)
2. Find DNS settings
3. Update nameservers to DigitalOcean:
   - ns1.digitalocean.com
   - ns2.digitalocean.com
   - ns3.digitalocean.com
4. Wait 24-48 hours for DNS propagation

### 8.2 Add DNS Records in DigitalOcean

1. Go to **Networking → Domains**
2. Click **Add Domain**
3. Enter your domain: `yourdomain.com`
4. Select your droplet
5. Add DNS records:

| Type | Name | Value |
|------|------|-------|
| A | @ | YOUR_DROPLET_IP |
| A | www | YOUR_DROPLET_IP |
| A | api | YOUR_DROPLET_IP |
| A | admin | YOUR_DROPLET_IP |

6. Click **Create Records**

### 8.3 Verify DNS

```bash
# Test from your droplet
nslookup yourdomain.com
nslookup api.yourdomain.com

# Should return your droplet IP
```

---

## Step 9: Setup SSL Certificates

### 9.1 Install Certbot

```bash
# Install certbot
sudo apt install -y certbot python3-certbot-nginx

# Or for generic use
sudo apt install -y certbot
```

### 9.2 Get SSL Certificate

```bash
# Create ssl directory
sudo mkdir -p /opt/planmorph/nginx/ssl

# Request certificate (standalone mode)
sudo certbot certonly --standalone \
  -d yourdomain.com \
  -d www.yourdomain.com \
  -d api.yourdomain.com \
  -d admin.yourdomain.com \
  --email your-email@gmail.com \
  --agree-tos \
  --no-eff-email

# Copy certificates
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem /opt/planmorph/nginx/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem /opt/planmorph/nginx/ssl/

# Set permissions
sudo chown -R root:root /opt/planmorph/nginx/ssl/
sudo chmod -R 600 /opt/planmorph/nginx/ssl/
```

### 9.3 Setup Auto-Renewal

```bash
# Test renewal
sudo certbot renew --dry-run

# Setup automatic renewal
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

# Verify
sudo systemctl status certbot.timer
```

### 9.4 Update Nginx Config

```bash
# Edit nginx configuration
sudo nano /opt/planmorph/nginx/nginx.conf
```

Update the server_name:
```nginx
server_name yourdomain.com www.yourdomain.com api.yourdomain.com admin.yourdomain.com;
```

---

## Step 10: Deploy Application

### 10.1 Pull Latest Code

```bash
cd /opt/planmorph

# Update repository
git pull origin main

# Verify files
ls -la
```

### 10.2 Pre-Deployment Checks

```bash
# Check docker-compose file
cat docker-compose.yml | head -20

# Check environment file
cat .env | grep "POSTGRES\|JWT\|PAYSTACK"

# Test docker
docker --version
docker compose version
```

### 10.3 Deploy with Nginx (Production)

```bash
# Make sure you're in the right directory
cd /opt/planmorph

# Deploy with production profile (includes Nginx)
./deploy.sh up production

# This will:
# 1. Build Docker images
# 2. Start all services
# 3. Start Nginx reverse proxy
# 4. Setup health checks
```

### 10.4 Wait for Services

```bash
# Check status (wait 2-3 minutes)
watch -n 5 './deploy.sh status'

# Press Ctrl+C to exit watch

# View logs
./deploy.sh logs

# Check specific service
./deploy.sh logs api
./deploy.sh logs postgres
./deploy.sh logs client
```

---

## Step 11: Verify Deployment

### 11.1 Check Services Status

```bash
# List all containers
./deploy.sh status

# Should show:
# - planmorph-api (healthy)
# - planmorph-admin (running)
# - planmorph-client (running)
# - planmorph-postgres (healthy)
# - planmorph-nginx (running)
```

### 11.2 Test API Health

```bash
# Test health endpoint
curl https://api.yourdomain.com/health

# Should return:
# {"status":"healthy"}

# Test detailed health
curl https://api.yourdomain.com/health/detailed
```

### 11.3 Test Frontend

```bash
# Visit in browser
https://yourdomain.com

# Should see:
# - PlanMorph homepage
# - Design marketplace
# - No SSL warnings
```

### 11.4 Test Admin Panel

```bash
# Visit in browser
https://admin.yourdomain.com

# Should see:
# - Blazor admin login
# - No SSL warnings
```

### 11.5 Check Database

```bash
# Connect to database
docker compose exec postgres psql -U planmorph planmorph

# Inside postgres:
\dt              # List tables
\q               # Quit

# Check database size
docker compose exec postgres du -sh /var/lib/postgresql/data
```

### 11.6 Test Email Sending

```bash
# Check email logs
docker compose logs api | grep -i email

# Or register a test account and verify email is sent
```

---

## Step 12: Setup Monitoring

### 12.1 Setup Log Rotation

```bash
# Create logrotate config
sudo nano /etc/logrotate.d/planmorph
```

Add:
```
/opt/planmorph/backups/*log {
    daily
    rotate 7
    compress
    delaycompress
    missingok
    notifempty
}
```

### 12.2 Setup Automated Backups

```bash
# Add to crontab
crontab -e

# Add these lines:
# Daily backup at 2 AM
0 2 * * * cd /opt/planmorph && ./deploy.sh backup-db >> /var/log/planmorph-backup.log 2>&1

# Weekly backup cleanup (keep only 4 weekly backups)
0 3 * * 0 find /opt/planmorph/backups -name "backup_*.sql" -mtime +7 -delete

# Save and exit (Ctrl+X in nano editors)
```

### 12.3 Setup Monitoring Script

```bash
# Create monitoring script
sudo nano /usr/local/bin/planmorph-monitor.sh
```

Add:
```bash
#!/bin/bash

# Check if services are running
services_ok=true

if ! curl -f https://api.yourdomain.com/health > /dev/null 2>&1; then
    echo "API health check failed"
    services_ok=false
fi

if ! docker compose ps | grep -q "planmorph-postgres.*healthy"; then
    echo "Database health check failed"
    services_ok=false
fi

if [ "$services_ok" = false ]; then
    # Send alert (email/Slack)
    echo "PlanMorph services are degraded" | mail -s "Alert" admin@yourdomain.com
    # Or restart services
    cd /opt/planmorph && ./deploy.sh restart
fi
```

Make executable:
```bash
sudo chmod +x /usr/local/bin/planmorph-monitor.sh
```

Add to crontab:
```bash
crontab -e

# Add:
*/5 * * * * /usr/local/bin/planmorph-monitor.sh
```

### 12.4 View Real-Time Monitoring

```bash
# Watch container stats
docker stats

# Watch logs
docker compose logs -f

# Watch specific service
docker compose logs -f api
```

---

## Troubleshooting

### Issue: Services won't start

```bash
# Check logs
docker compose logs

# Common issues:
# 1. postgres not healthy
./deploy.sh logs postgres

# 2. API can't connect to database
# Check connection string in .env
grep "POSTGRES" .env

# 3. Port already in use
sudo lsof -i :80
sudo lsof -i :443
sudo lsof -i :3000
sudo lsof -i :7038
```

### Issue: SSL Certificate errors

```bash
# Check certificate
sudo ls -la /opt/planmorph/nginx/ssl/

# Renew certificate
sudo certbot renew --force-renewal

# Restart nginx
docker compose restart nginx
```

### Issue: Database connection errors

```bash
# Make sure postgres is running
docker compose ps postgres

# Check logs
docker compose logs postgres

# Restart postgres
docker compose restart postgres

# Verify connection
docker compose exec postgres psql -U planmorph -d planmorph -c "SELECT 1"
```

### Issue: API not responding

```bash
# Check if API is running
docker compose ps api

# Check API logs
docker compose logs api

# Restart API
docker compose restart api

# Check health endpoint
curl https://yourdomain.com/api/health
```

### Issue: High memory/CPU usage

```bash
# Check resource usage
docker stats

# Limit resources in docker-compose.yml
# Add to service:
# mem_limit: 512m
# cpus: '1.0'

# Rebuild
./deploy.sh rebuild
```

### Issue: Out of disk space

```bash
# Check disk usage
df -h

# Clean up old images
docker image prune -a

# Clean up volumes
docker volume prune

# Check backups (remove old ones)
ls -lah /opt/planmorph/backups/
rm /opt/planmorph/backups/old-backup.sql
```

### Issue: DNS not resolving

```bash
# Test DNS
nslookup yourdomain.com

# If not working:
# 1. Wait 24-48 hours for propagation
# 2. Check DigitalOcean DNS records
# 3. Verify nameserver settings at registrar
# 4. Use Google DNS for test
nslookup -root_servers yourdomain.com
```

---

## Maintenance

### Daily

```bash
# Check status
./deploy.sh status

# Review logs
docker compose logs --tail=100
```

### Weekly

```bash
# Backup database
./deploy.sh backup-db

# Check disk space
df -h

# Monitor resource usage
docker stats --no-stream
```

### Monthly

```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Rebuild images with latest code
git pull origin main
./deploy.sh rebuild

# Prune old Docker images
docker image prune -a

# Review and archive logs
```

### Quarterly

```bash
# Test backup/restore
./deploy.sh backup-db
# Later...
./deploy.sh restore-db backup_20260211_020000.sql

# Review and update SSL certificates
sudo certbot certificates

# Security audit
docker compose logs | grep ERROR | tail -50
```

---

## Useful Commands

```bash
# View logs
docker compose logs -f                    # All services
docker compose logs -f api                # Specific service
docker compose logs --tail=50 api         # Last 50 lines

# Check status
./deploy.sh status                        # Service status
docker compose ps                         # Container list
docker stats                              # Resource usage

# Database operations
./deploy.sh backup-db                     # Backup
./deploy.sh restore-db <file>             # Restore
./deploy.sh migrate                       # Run migrations

# Restart services
./deploy.sh restart                       # Restart all
docker compose restart api                # Restart one

# Stop/Start
./deploy.sh down                          # Stop all
./deploy.sh up production                 # Start all with nginx

# SSH into container
docker compose exec api bash              # API container
docker compose exec postgres psql -U planmorph planmorph  # Database

# View environment
cat .env
grep "PAYSTACK" .env
```

---

## Performance Tips

1. **Enable Caching** - Configure Nginx caching for static assets
2. **Database Optimization** - Run VACUUM ANALYZE regularly
3. **Monitor Resources** - Watch CPU/memory usage
4. **Scale Up If Needed** - Upgrade droplet if under high load
5. **Enable CDN** - Use DigitalOcean CDN for static files
6. **Compress Assets** - GZIP is already enabled in Nginx
7. **Optimize Images** - Resize images before upload

---

## Security Checklist

- [ ] SSH key-based authentication enabled
- [ ] Firewall configured (only 80, 443, 22)
- [ ] SSL certificate installed and valid
- [ ] .env file permissions set to 600
- [ ] Regular backups enabled
- [ ] Monitoring and alerts configured
- [ ] Email notifications setup
- [ ] API rate limiting enabled (in Nginx)
- [ ] Security headers configured
- [ ] Database password is strong
- [ ] JWT secret key is strong
- [ ] Paystack keys are from production account (when ready)

---

## Final Checklist

- [ ] Droplet created and accessible
- [ ] Docker and Docker Compose installed
- [ ] Repository cloned
- [ ] Environment configured (.env file)
- [ ] DigitalOcean Spaces bucket created
- [ ] Domain DNS configured
- [ ] SSL certificates installed
- [ ] Application deployed (`./deploy.sh up production`)
- [ ] Services verified and healthy
- [ ] Health endpoints responding
- [ ] Frontend accessible
- [ ] Admin panel accessible
- [ ] Email sending tested
- [ ] Backups configured
- [ ] Monitoring setup

---

## Next Steps After Deployment

1. **Test Everything**
   - Create admin account
   - Register architect/engineer
   - Upload designs
   - Make test purchase
   - Request construction service

2. **Monitor Performance**
   - Watch resource usage
   - Check for errors in logs
   - Verify email sending
   - Test payment gateway

3. **Configure Services**
   - Add more email templates if needed
   - Adjust rate limiting
   - Configure monitoring alerts
   - Setup backup retention

4. **Scale & Optimize**
   - Monitor traffic patterns
   - Upgrade droplet if needed
   - Enable CDN for images
   - Optimize database queries

---

## Support & Resources

**DigitalOcean Documentation:**
- https://docs.digitalocean.com/
- https://docs.digitalocean.com/products/app-platform/
- https://docs.digitalocean.com/products/spaces/

**Docker Documentation:**
- https://docs.docker.com/
- https://docs.docker.com/compose/

**PlanMorph Documentation:**
- `DEPLOYMENT_GUIDE.md`
- `README_DEPLOYMENT.md`
- `DEPLOYMENT_SUMMARY.md`

---

**Deployment Time:** 30-45 minutes
**Estimated Monthly Cost:** $12-24 USD
**Status:** Production Ready ✅

You're all set! Your PlanMorph application is now running on DigitalOcean! 🎉
