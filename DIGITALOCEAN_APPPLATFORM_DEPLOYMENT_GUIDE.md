# PlanMorph - DigitalOcean App Platform Deployment Guide

**Date:** February 12, 2026
**Platform:** DigitalOcean App Platform (Managed Service)
**Time Required:** 20-30 minutes
**Difficulty:** Easy (No server management required)
**Status:** ✅ Production Ready

---

## Table of Contents

1. [Why App Platform?](#why-app-platform)
2. [Prerequisites](#prerequisites)
3. [Step 1: Prepare Your GitHub Repository](#step-1-prepare-your-github-repository)
4. [Step 2: Create DigitalOcean Account & Access App Platform](#step-2-create-digitalocean-account--access-app-platform)
5. [Step 3: Create New App](#step-3-create-new-app)
6. [Step 4: Connect GitHub Repository](#step-4-connect-github-repository)
7. [Step 5: Configure Services](#step-5-configure-services)
8. [Step 6: Configure Database](#step-6-configure-database)
9. [Step 7: Configure Environment Variables](#step-7-configure-environment-variables)
10. [Step 8: Configure Domain & DNS](#step-8-configure-domain--dns)
11. [Step 9: Deploy Application](#step-9-deploy-application)
12. [Step 10: Verify Deployment](#step-10-verify-deployment)
13. [Monitoring & Maintenance](#monitoring--maintenance)
14. [Scaling & Cost Optimization](#scaling--cost-optimization)
15. [Troubleshooting](#troubleshooting)

---

## Why App Platform?

### Benefits of DigitalOcean App Platform vs Droplets

| Feature | App Platform | Droplet |
|---------|---|---|
| **Server Management** | ✅ Fully Managed | ❌ Manual |
| **Scaling** | ✅ Automatic | ❌ Manual |
| **SSL Certificates** | ✅ Automatic & Free | ✅ Free but manual |
| **Deployments** | ✅ Git-triggered auto-deploy | ✅ Manual script |
| **Monitoring** | ✅ Built-in with logs | ⚠️ Third-party required |
| **Database Backups** | ✅ Automatic daily | ⚠️ Manual scripts |
| **Pricing** | Pay per service | Pay per droplet |
| **Time to Deploy** | 15-20 minutes | 45-60 minutes |
| **DevOps Knowledge** | ❌ Minimal | ✅ Advanced |

### PlanMorph on App Platform

With App Platform, your PlanMorph application will:
- ✅ Deploy automatically when you push to GitHub
- ✅ Scale automatically during traffic spikes
- ✅ Have SSL certificates automatically renewed
- ✅ Get daily automated backups
- ✅ Have built-in logs and monitoring
- ✅ No need to manage servers, Docker, or SSL

---

## Prerequisites

You will need:

- ✅ **DigitalOcean Account** - Sign up at https://www.digitalocean.com
- ✅ **GitHub Account with PlanMorph Repository** - Must have `app.yaml` in root
- ✅ **GitHub Personal Access Token** - For connecting to DigitalOcean
- ✅ **Custom Domain Name** (optional, can use DigitalOcean domain)
- ✅ **Environment Credentials:**
  - Database password (32+ random characters)
  - JWT secret key (32+ characters)
  - Email credentials (Gmail/SendGrid/AWS SES)
  - Paystack API keys
  - DigitalOcean Spaces API keys

---

## Step 1: Prepare Your GitHub Repository

### 1.1 Ensure app.yaml Exists

Your repository's root directory should contain `app.yaml`:

```bash
# Check if app.yaml exists
cat app.yaml

# Should show the app configuration with services: api, client, admin
```

### 1.2 Ensure Dockerfile Exists for Each Service

```bash
# Check Dockerfiles exist
ls -la PlanMorph.Api/Dockerfile
ls -la PlanMorph.Admin/Dockerfile
ls -la planmorph-client/Dockerfile
```

### 1.3 Verify .dockerignore Files

```bash
# Check .dockerignore files exist
ls -la .dockerignore
ls -la PlanMorph.Api/.dockerignore
ls -la planmorph-client/.dockerignore
```

### 1.4 Commit and Push

```bash
cd /path/to/planmorph
git add app.yaml .dockerignore
git commit -m "Add App Platform configuration"
git push origin main
```

---

## Step 2: Create DigitalOcean Account & Access App Platform

### 2.1 Create Account (if needed)

1. Go to https://www.digitalocean.com
2. Click **Sign Up**
3. Complete registration with your email
4. Add payment method
5. Verify email

### 2.2 Access App Platform

1. Go to DigitalOcean Console
2. Click **Create** in top right
3. Click **App Platform**
4. You're now in App Platform dashboard

---

## Step 3: Create New App

### 3.1 Start App Creation

1. Click **Create App**
2. You'll see the app creation wizard
3. Select **GitHub** as source (this allows auto-deploy on push)

---

## Step 4: Connect GitHub Repository

### 4.1 Authorize DigitalOcean with GitHub

1. Click **Authorize GitHub**
2. GitHub login page will appear
3. Authorize DigitalOcean to access your repositories
4. You'll be redirected back to DigitalOcean

### 4.2 Select Repository

1. In "Which repository would you like to deploy?" search box:
   - Type: `planmorph`
   - Select your PlanMorph repository
2. Check "Autodeploy" if you want production deployments on every push to main
3. Click **Next**

### 4.3 Repository Information

Verify:
- Repository: `your-org/planmorph`
- Branch: `main`
- Source: DigitalOcean App Spec (app.yaml)

Click **Next**

---

## Step 5: Configure Services

The app.yaml file defines 3 services that will be automatically configured:

### 5.1 Review Service Configuration

You should see:

1. **API Service**
   - Name: `api`
   - Port: 80 (internal)
   - HTTP endpoint: `/api`
   - Health check: `/health`

2. **Admin Service**
   - Name: `admin`
   - Port: 80 (internal)
   - HTTP endpoint: `/admin`
   - Health check: `/`

3. **Client Service**
   - Name: `client`
   - Port: 3000 (internal)
   - HTTP endpoint: `/` (root)
   - Health check: `/`

**DO NOT change these settings** - they're optimized in app.yaml

Click **Next**

---

## Step 6: Configure Database

### 6.1 Database Settings

You should see:

- **Name:** `db`
- **Engine:** PostgreSQL 16
- **Version:** 16
- **Production Mode:** Enabled

**DO NOT remove or modify** - the database is required for the application

Click **Next**

---

## Step 7: Configure Environment Variables

### 7.1 Critical Security Step

This is where you'll set all your credentials. **DO NOT commit these to GitHub** - only set them in App Platform console.

### 7.2 Required Environment Variables

Go through and set these values:

#### Database (Auto-generated)
- `db.username` - Auto-filled
- `db.password` - Auto-filled
- `db.host` - Auto-filled
- `db.port` - Auto-filled
- `db.name` - Auto-filled

#### Application Settings (You need to set these)

```
ASPNETCORE_ENVIRONMENT = Production
ASPNETCORE_URLS = http://+:80
```

#### JWT Authentication
```
JWT_SECRET_KEY = [Generate: openssl rand -base64 32]
```

**Generate command (run locally):**
```bash
openssl rand -base64 32
```

Paste the output into the field.

#### Email (SMTP) - Choose one option

**Option 1: Gmail (for testing)**
```
SMTP_HOST = smtp.gmail.com
SMTP_PORT = 587
SMTP_USERNAME = your-email@gmail.com
SMTP_PASSWORD = [Generate here: myaccount.google.com/apppasswords]
FROM_EMAIL = noreply@yourdomain.com
FROM_NAME = PlanMorph
```

**Option 2: SendGrid (recommended for production)**
```
SMTP_HOST = smtp.sendgrid.net
SMTP_PORT = 587
SMTP_USERNAME = apikey
SMTP_PASSWORD = SG.[Your SendGrid API key from dashboard.sendgrid.com]
FROM_EMAIL = noreply@yourdomain.com
FROM_NAME = PlanMorph
```

#### Payment Gateway (Paystack)
```
PAYSTACK_SECRET_KEY = [Get from: https://dashboard.paystack.com/settings/developer]
PAYSTACK_PUBLIC_KEY = [Get from: https://dashboard.paystack.com/settings/developer]
PAYSTACK_WEBHOOK_SECRET = [Get from: https://dashboard.paystack.com/settings/developer]
```

**For Testing:** Use `sk_test_*` and `pk_test_*` keys
**For Production:** Use `sk_live_*` and `pk_live_*` keys

#### File Storage (DigitalOcean Spaces)
```
DO_SPACES_ACCESS_KEY = [Get from: https://cloud.digitalocean.com/account/api/tokens]
DO_SPACES_SECRET_KEY = [Get from: https://cloud.digitalocean.com/account/api/tokens]
DO_SPACES_BUCKET_NAME = planmorph-designs
DO_SPACES_REGION = nyc3
DO_SPACES_ENDPOINT = https://nyc3.digitaloceanspaces.com
```

#### Frontend API URL
```
NEXT_PUBLIC_API_URL = https://yourdomain.com
(Or use the App Platform generated URL initially)
```

### 7.3 How to Get Each Credential

**For Database Credentials:**
- Don't fill these - App Platform provides them automatically
- They'll appear in your environment when deployed

**For JWT Secret:**
```bash
# On your local machine
openssl rand -base64 32
# Copy the output and paste into JWT_SECRET_KEY field
```

**For Gmail:**
1. Go to https://myaccount.google.com/apppasswords
2. Select "Mail" and "Windows (Custom)"
3. Generate app password
4. Copy and paste into SMTP_PASSWORD

**For SendGrid:**
1. Go to https://dashboard.sendgrid.com
2. Navigate to API Keys
3. Create new API key
4. Copy and paste into SMTP_PASSWORD (with `SG.` prefix)

**For Paystack:**
1. Go to https://dashboard.paystack.com/settings/developer
2. Copy test keys or live keys
3. Paste into the respective fields

**For DigitalOcean Spaces:**
1. Go to https://cloud.digitalocean.com/account/api/tokens
2. Click "Generate New Token"
3. Name: "PlanMorph App"
4. Scopes: Spaces (read + write)
5. Generate and copy credentials

### 7.4 Add Variables in App Platform Console

1. Scroll down to "Environment" section
2. Click **Edit** (pencil icon)
3. For each variable:
   - Click **Add Variable**
   - Enter name (e.g., `JWT_SECRET_KEY`)
   - Enter value
   - Click **Save**

**Complete all required variables before proceeding**

Click **Next**

---

## Step 8: Configure Domain & DNS

### 8.1 Custom Domain (Optional)

If you have a custom domain:

1. Leave this step for now
2. After deployment, you'll add it in the app settings

### 8.2 App Platform Provided URL

By default, you'll get a URL like:
```
https://planmorph-xxxxx.ondigitalocean.app
```

You can use this immediately for testing.

Click **Next**

---

## Step 9: Deploy Application

### 9.1 Review Configuration

You should see:

- ✅ Source: GitHub (your-org/planmorph, main branch)
- ✅ Services: API, Admin, Client (from app.yaml)
- ✅ Database: PostgreSQL 16
- ✅ Environment Variables: All set
- ✅ Autodeploy: Enabled (recommended)

### 9.2 Deploy

Click **Create Resources**

This will:
1. Create the application
2. Create the managed PostgreSQL database
3. Build Docker images for all 3 services
4. Deploy all services
5. Setup SSL/TLS certificates
6. Configure routing

**Estimated time: 5-10 minutes**

### 9.3 Watch Deployment Progress

You'll see a "Deployment in progress" screen with:
- Step 1: Building services
- Step 2: Provisioning database
- Step 3: Deploying services
- Step 4: Setting up networking

Once complete, you'll see a green checkmark.

---

## Step 10: Verify Deployment

### 10.1 Get App URL

1. Click on your app name in the console
2. You should see "Live App" section
3. Click the URL or look for endpoints
4. Default URL: `https://planmorph-xxxxx.ondigitalocean.app`

### 10.2 Test Services

**Frontend (Root)**
```bash
curl https://planmorph-xxxxx.ondigitalocean.app
# Should return HTML homepage
```

**API Health**
```bash
curl https://planmorph-xxxxx.ondigitalocean.app/api/health
# Should return: {"status":"healthy"}
```

**Admin Panel**
```bash
https://planmorph-xxxxx.ondigitalocean.app/admin
# Should load Blazor admin interface
```

### 10.3 Test Email (Optional)

1. Go to frontend: `https://planmorph-xxxxx.ondigitalocean.app`
2. Create test account
3. Check your email for verification message
4. If received, email is working! ✅

### 10.4 Check Logs

1. In DigitalOcean App Platform console
2. Click **Logs** tab
3. Select service: API, Client, or Admin
4. View real-time logs for each service

---

## Monitoring & Maintenance

### 11.1 Daily Monitoring

**Via DigitalOcean Console:**

1. Open App Platform dashboard
2. Click your app
3. View sections:
   - **Overview:** Status, resource usage, deployment history
   - **Logs:** Real-time logs for each service
   - **Metrics:** CPU, memory, disk usage charts
   - **Settings:** Configuration, environment variables

**Check Daily:**
- ✅ All services showing "Healthy" or "Running"
- ✅ No errors in logs
- ✅ Resource usage normal (CPU < 80%, Memory < 80%)

### 11.2 Database Backups

App Platform automatically:
- ✅ Creates daily backups (kept for 7 days)
- ✅ Performs point-in-time recovery
- ✅ Creates transaction logs

**To restore from backup:**
1. Go to **Databases** in DigitalOcean console
2. Select your database
3. Click **Backups** tab
4. Click **Restore** on desired backup

### 11.3 Automatic Deployments

When you push to the main branch:

```bash
git add .
git commit -m "Your changes"
git push origin main
```

App Platform will:
1. Detect the push
2. Build new Docker images
3. Run health checks
4. Deploy new version (rolling update - no downtime)
5. Send deployment notification

**Check deployment status:**
1. Go to app dashboard
2. Click **Deployments** tab
3. View status, logs, and history

---

## Scaling & Cost Optimization

### 12.1 Understanding Pricing

App Platform pricing is per-service, not per-droplet:

| Component | Size | Cost/Month |
|-----------|------|-----------|
| API Service | basic | $5 |
| Client Service | basic | $5 |
| Admin Service | basic | $5 |
| PostgreSQL | basic (512MB) | $15 |
| **Total** | | **$30/month** |

### 12.2 Auto-Scaling (Optional)

1. Go to your app in console
2. Click **Settings**
3. Enable "Auto-Scale" for services that need it
4. Set min/max instances (e.g., 1-3)
5. Define CPU threshold (e.g., scale up at 80% CPU)

### 12.3 Upgrade Database (if needed)

1. Go to **Databases** in DigitalOcean
2. Select your PlanMorph database
3. Click **Settings**
4. Under "Plan," click **Change**
5. Select larger plan and confirm
6. Database scales with zero downtime

### 12.4 Monitoring Costs

1. Go to **Billing** in DigitalOcean
2. View monthly costs by service
3. See forecast for current month
4. Set billing alerts (recommended: set at $50/month)

---

## Troubleshooting

### Issue: Services showing "Not Running"

```bash
# Check service status
# Go to Logs tab to see error messages
# Look for deployment errors
```

**Common causes:**
- Dockerfile build failure → Check Docker syntax
- Environment variable missing → Add to App Platform console
- Port conflict → Verify Dockerfile exposes correct ports
- Health check failure → Check if API is responding

**Fix:**
1. Make code changes locally
2. Commit and push to GitHub
3. App Platform automatically redeploys
4. Check logs for new errors

---

### Issue: Database Connection Failed

**Error message:** "Connection refused" or "Cannot connect to database"

**Fix:**
1. Go to App Platform console
2. Click your app
3. Click **Postgres** database component
4. View connection credentials:
   - Host (copy this)
   - Port (usually 25060)
   - Database name
   - Username
   - Password

5. Verify these match your app.yaml:
```yaml
ConnectionStrings__DefaultConnection: ${db.username}:${db.password}@${db.host}:${db.port}/${db.name}
```

If still failing:
```bash
# Check logs
# Go to app → Logs → Select "api" service
# Look for "database" or "connection" errors
```

---

### Issue: SSL Certificate Error

**Error:** "Connection not private" or "Certificate invalid"

**Note:** App Platform handles SSL automatically - it should work immediately.

**Fix:**
1. Wait 2-3 minutes (certificate generation can take time)
2. Hard refresh browser (Ctrl+Shift+R or Cmd+Shift+R)
3. Try in incognito/private mode
4. Check app console:
   - Go to **Settings**
   - Under "Components," check HTTPS status
   - Should show: "Certificate active and valid"

---

### Issue: Images Not Uploading to DigitalOcean Spaces

**Error:** "Access denied" or "Upload failed"

**Fix:**
1. Verify DigitalOcean Spaces credentials in App Platform:
   - `DO_SPACES_ACCESS_KEY`
   - `DO_SPACES_SECRET_KEY`
   - `DO_SPACES_BUCKET_NAME`
   - `DO_SPACES_REGION`
2. Verify bucket exists and is not full
3. Check app logs for specific error
4. Recreate tokens if old:
   - Go to DigitalOcean → API Tokens
   - Revoke old token
   - Create new token with Spaces scope
   - Update in App Platform console

---

### Issue: High Memory Usage

**Observed:** Memory usage > 90%

**Fix:**
1. Check which service is using memory:
   - Go to **Metrics** tab
   - Look for memory spikes
   - Identify service (API, Client, Admin)

2. Check if database is too large:
   ```bash
   # This would be shown in Database details
   # Go to Databases → Your database → Usage
   ```

3. Options:
   - Upgrade service size (more RAM)
   - Upgrade database size
   - Optimize application code
   - Check for memory leaks in API logs

---

### Issue: Application Not Responding After Deployment

**Steps to recover:**
1. Go to App Platform console
2. Click your app
3. Click **Deployments**
4. Find the last successful deployment
5. Click the three dots (•••)
6. Click **Rollback to this Deployment**
7. Confirm rollback

This will revert your app to the last working version while you debug the issue.

---

## Maintenance Schedule

### Daily (5 minutes)
- [ ] Check app dashboard status
- [ ] Review any critical logs

### Weekly (15 minutes)
- [ ] Check resource usage trends
- [ ] Verify email notifications working
- [ ] Review deployment history

### Monthly (30 minutes)
- [ ] Review cost breakdown
- [ ] Check for any error patterns
- [ ] Update environment variables if needed
- [ ] Review database size and performance

### Quarterly (1 hour)
- [ ] Test database backup/restore
- [ ] Review security settings
- [ ] Check for available updates
- [ ] Plan scaling needs based on growth

---

## Key Differences from Droplet Deployment

| Task | Droplet | App Platform |
|------|---------|---|
| **SSH Access** | ❌ Manual SSH required | ✅ No SSH needed |
| **Docker Management** | ❌ Manual docker commands | ✅ Automatic |
| **SSL Renewal** | ⚠️ Manual with certbot | ✅ Automatic |
| **Backups** | ⚠️ Manual scripts | ✅ Automatic daily |
| **Scaling** | ❌ Manual upgrade needed | ✅ Automatic up to limits |
| **Deployments** | ⚠️ Manual SSH + scripts | ✅ Git push triggers auto-deploy |
| **Monitoring** | ⚠️ Third-party tools | ✅ Built-in |
| **Updates** | ❌ Manual apt upgrade | ✅ App Platform handles OS |

---

## Next Steps After Deployment

### 1. Configure Custom Domain (Optional)

1. Go to your app in App Platform
2. Click **Settings**
3. Scroll to "Domains"
4. Click **Edit**
5. Enter your domain: `yourdomain.com`
6. Update your domain's DNS:
   - Go to domain registrar
   - Set **CNAME** or **A record** to App Platform URL
   - Wait 24 hours for propagation

### 2. Test All Features

- [ ] Register architect account
- [ ] Register engineer account
- [ ] Upload design
- [ ] Request construction service
- [ ] Test email notifications
- [ ] Test payment gateway (use test mode)

### 3. Setup Environment Monitoring

1. Go to **Alerts** in DigitalOcean
2. Create alert for high CPU (> 80%)
3. Create alert for high memory (> 80%)
4. Create alert for disk usage (> 80%)
5. Set notification email

### 4. Document Configuration

Save these for future reference:
- [ ] App URL
- [ ] Database hostname
- [ ] Backup recovery procedures
- [ ] Custom domain (if added)

---

## Support Resources

**DigitalOcean App Platform Docs:**
- https://docs.digitalocean.com/products/app-platform/
- https://docs.digitalocean.com/reference/app-platform-spec/

**PlanMorph Documentation:**
- See DOCUMENTATION_INDEX.md
- See DEPLOYMENT_GUIDE.md
- See README_DEPLOYMENT.md

**Getting Help:**
- DigitalOcean Community: https://www.digitalocean.com/community
- DigitalOcean Support: https://www.digitalocean.com/support
- GitHub Issues: Your repository issues section

---

## Success Criteria

After deployment, verify:

- ✅ Frontend loads at https://yourdomain.com (or app URL)
- ✅ API responds at https://yourdomain.com/api/health
- ✅ Admin panel at https://yourdomain.com/admin
- ✅ SSL certificate valid (green lock icon)
- ✅ Automatic backups running
- ✅ Email notifications working
- ✅ Payment gateway responding
- ✅ File uploads to DigitalOcean Spaces working
- ✅ All services showing "Healthy"
- ✅ No errors in logs

---

**Deployment Status:** ✅ Complete!
**Your app is now running on DigitalOcean App Platform!** 🎉

For any questions or issues, refer to the troubleshooting section or DigitalOcean documentation.

