# DigitalOcean Deployment - Quick Reference Checklist

**Time Required:** 45 minutes
**Cost:** $12-24/month
**Difficulty:** Moderate

---

## 🚀 Quick 10-Step Deployment

### ✅ Step 1: Prepare DigitalOcean Account (5 min)
```
[ ] Go to digitalocean.com
[ ] Create account or sign in
[ ] Add payment method
[ ] Generate SSH key (optional but recommended)
[ ] Add SSH key to account
```

**Need SSH Key?**
```bash
# Generate
ssh-keygen -t rsa -b 4096 -f ~/.ssh/do_key

# Get public key
cat ~/.ssh/do_key.pub

# Copy and paste into DigitalOcean → Settings → Security → SSH Keys
```

---

### ✅ Step 2: Create Droplet (5 min)
```
[ ] Click Create → Droplet
[ ] Region: nyc3 (or closest to users)
[ ] OS: Ubuntu 22.04 LTS
[ ] Size: $12/month (2GB RAM)
[ ] Select SSH key
[ ] Create Droplet
[ ] Copy IP Address (you'll need this!)
```

**Your Droplet IP:** `__________.__________.__________.__________ `

---

### ✅ Step 3: Connect & Setup Docker (10 min)
```bash
# SSH into droplet
ssh -i ~/.ssh/do_key root@YOUR_DROPLET_IP

# Once connected, run these commands:

# Update system
sudo apt update && sudo apt upgrade -y

# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh
sudo usermod -aG docker root

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose

# Verify
docker --version
docker compose version

[ ] Docker version 20.10+
[ ] Docker Compose version 2.0+
```

---

### ✅ Step 4: Clone & Configure (5 min)
```bash
# Create application directory
sudo mkdir -p /opt/planmorph
cd /opt/planmorph

# Clone repository
git clone https://github.com/your-org/planmorph.git .

# Make scripts executable
chmod +x deploy.sh

# Copy environment template
cp .env.example .env

# Edit configuration
nano .env

[ ] Repository cloned successfully
[ ] .env file created
```

**Configuration Needed in .env:**
```
POSTGRES_PASSWORD = ___________________________
JWT_SECRET_KEY = ______________________________
SMTP_HOST = ____________________________________
SMTP_USERNAME = ________________________________
SMTP_PASSWORD = ________________________________
PAYSTACK_SECRET_KEY = __________________________
PAYSTACK_PUBLIC_KEY = ___________________________
DO_SPACES_ACCESS_KEY = __________________________
DO_SPACES_SECRET_KEY = __________________________
NEXT_PUBLIC_API_URL = https://yourdomain.com
```

---

### ✅ Step 5: Setup DigitalOcean Spaces (5 min)

```
[ ] Go to DigitalOcean console
[ ] Create → Spaces
[ ] Name: planmorph-designs
[ ] Region: Same as droplet (nyc3)
[ ] ACL: Private
[ ] Create Space
[ ] Generate API tokens (Spaces scope)
[ ] Copy access key & secret key to .env
```

---

### ✅ Step 6: Setup Domain & DNS (5 min)

```
[ ] Point domain nameservers to DigitalOcean
   NS1: ns1.digitalocean.com
   NS2: ns2.digitalocean.com
   NS3: ns3.digitalocean.com

OR in DigitalOcean:
[ ] Go to Networking → Domains
[ ] Add your domain
[ ] Add DNS records:

   Type    Name    Value
   ----    ----    -----
   A       @       YOUR_DROPLET_IP
   A       www     YOUR_DROPLET_IP
   A       api     YOUR_DROPLET_IP

[ ] Test DNS (wait 2-5 min)
    nslookup yourdomain.com
```

**Your Domain:** `________________________________`

---

### ✅ Step 7: Setup SSL Certificate (5 min)

```bash
# Back on your droplet

# Install certbot
sudo apt install -y certbot

# Create SSL directory
sudo mkdir -p /opt/planmorph/nginx/ssl

# Get certificate (replace yourdomain.com with your actual domain)
sudo certbot certonly --standalone \
  -d yourdomain.com \
  -d www.yourdomain.com \
  -d api.yourdomain.com \
  --email your@email.com \
  --agree-tos \
  --no-eff-email

# Copy certificates
sudo cp /etc/letsencrypt/live/yourdomain.com/fullchain.pem /opt/planmorph/nginx/ssl/
sudo cp /etc/letsencrypt/live/yourdomain.com/privkey.pem /opt/planmorph/nginx/ssl/

# Set permissions
sudo chown -R root:root /opt/planmorph/nginx/ssl/
sudo chmod -R 600 /opt/planmorph/nginx/ssl/

# Setup auto-renewal
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer

[ ] SSL certificates installed
[ ] Auto-renewal enabled
```

---

### ✅ Step 8: Deploy Application (5 min)

```bash
# Ensure you're in the right directory
cd /opt/planmorph

# Deploy with production profile (includes Nginx!)
./deploy.sh up production

# This will take 2-3 minutes to build and start all services
# Watch the output for any errors

[ ] Build completed successfully
[ ] All services started
```

---

### ✅ Step 9: Verify Services (5 min)

```bash
# Check status (wait 2 minutes)
./deploy.sh status

# Should show all services as "healthy" or "running"

# Check API health
curl https://yourdomain.com/api/health
# Response: {"status":"healthy"}

# Check in browser
https://yourdomain.com           # Frontend
https://api.yourdomain.com/api   # API docs

[ ] All services healthy
[ ] Frontend accessible
[ ] API responding
[ ] No SSL errors
```

---

### ✅ Step 10: Setup Backups (5 min)

```bash
# Add to crontab (automated daily backups)
crontab -e

# Add these lines:
0 2 * * * cd /opt/planmorph && ./deploy.sh backup-db >> /var/log/planmorph-backup.log 2>&1

# Manual backup
./deploy.sh backup-db

[ ] Crontab configured
[ ] First backup completed
```

---

## 🎉 Deployment Complete!

**Access Your Application:**

| Component | URL |
|-----------|-----|
| Frontend | https://yourdomain.com |
| API | https://api.yourdomain.com |
| API Docs | https://api.yourdomain.com/swagger |
| Admin (Blazor) | https://admin.yourdomain.com |

---

## 📊 Common Tasks

### View Logs
```bash
./deploy.sh logs                # All services
./deploy.sh logs api            # Specific service
```

### Restart Services
```bash
./deploy.sh restart             # Restart all
docker compose restart api      # Restart one
```

### Backup & Restore
```bash
./deploy.sh backup-db           # Backup
./deploy.sh restore-db <file>   # Restore
```

### Check Status
```bash
./deploy.sh status              # Service status
docker stats                    # Resource usage
```

---

## ⚠️ Troubleshooting

### Services won't start?
```bash
docker compose logs
./deploy.sh logs postgres   # Check database
./deploy.sh logs api        # Check API
```

### SSL certificate error?
```bash
sudo certbot renew --force-renewal
docker compose restart nginx
```

### Database not connecting?
```bash
docker compose restart postgres
./deploy.sh migrate
```

### Port already in use?
```bash
sudo lsof -i :80
sudo lsof -i :443
```

---

## 💾 Important Commands Reference

```bash
# Stop services
./deploy.sh down

# Restart services
./deploy.sh restart

# Rebuild images
./deploy.sh rebuild

# Complete clean (CAREFUL!)
./deploy.sh clean

# Database migration
./deploy.sh migrate

# SSH into services
docker compose exec api bash
docker compose exec postgres psql -U planmorph planmorph
```

---

## 🔒 Security Reminder

```bash
# Protect .env file
chmod 600 /opt/planmorph/.env

# Use strong passwords
POSTGRES_PASSWORD = should be 32+ random characters
JWT_SECRET_KEY = should be 32+ random characters

# Always use HTTPS in production
# Let's Encrypt certificates are free and auto-renew
```

---

## 📈 Next Steps

1. **Test Everything**
   - Create admin account
   - Upload a design
   - Test payment (use Paystack test mode)
   - Request construction service

2. **Configure Monitoring**
   - Add monitoring script
   - Setup email alerts
   - Monitor disk space regularly

3. **Optimize Performance**
   - Monitor resource usage
   - Upgrade droplet if needed (from $12 to $24+)
   - Enable CDN for images

4. **Setup Backups**
   - Daily automated backups configured
   - Test restore process monthly
   - Keep backups for 30 days

---

## 📞 Need Help?

**Deployment Issues:**
- Check logs: `./deploy.sh logs`
- Review `DIGITALOCEAN_DEPLOYMENT_GUIDE.md` (detailed guide)

**Configuration Issues:**
- Check `.env` file: `cat /opt/planmorph/.env`
- Verify credentials in DigitalOcean console

**Performance Issues:**
- Check resources: `docker stats`
- Review logs for errors: `./deploy.sh logs | grep ERROR`

---

**Congratulations! Your PlanMorph application is now running on DigitalOcean! 🚀**

---

## Summary Stats

| Item | Value |
|------|-------|
| Total Time | 45 minutes |
| Uptime | 99.9% (DigitalOcean SLA) |
| Estimated Cost | $12-24/month |
| Storage Included | 50GB |
| Auto-Backups | Yes (configured) |
| SSL Certificate | Free (Let's Encrypt) |
| Email Service | SMTP (your choice) |
| File Storage | DigitalOcean Spaces |
| Payment Gateway | Paystack |
| Status | ✅ Production Ready |

---

**Version:** 1.0
**Date:** February 11, 2026
**Last Updated:** Production Ready ✅
