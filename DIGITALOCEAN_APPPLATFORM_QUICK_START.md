# DigitalOcean App Platform - Quick Start (20 minutes)

**Time Required:** 20 minutes
**Difficulty:** Easy
**Cost:** ~$30/month
**No Server Management:** ✅

---

## 🚀 Quick 6-Step Deployment

### ✅ Step 1: Prepare GitHub (2 min)

Ensure your repo has these files in the root:
- `app.yaml` (your app configuration)
- `Dockerfile` files for each service
- `.dockerignore` files

```bash
git add app.yaml .dockerignore
git commit -m "Add App Platform config"
git push origin main
```

**Your Repo:** `https://github.com/___________/planmorph` ✓

---

### ✅ Step 2: Create DigitalOcean Account (3 min)

1. Go to https://www.digitalocean.com
2. Click **Sign Up**
3. Complete registration
4. Add payment method
5. Verify email

**Your Account Email:** `_____________________________` ✓

---

### ✅ Step 3: Start App Creation (2 min)

1. Go to DigitalOcean Console
2. Click **Create** → **App Platform**
3. Click **Create App**
4. Select **GitHub** as source

---

### ✅ Step 4: Connect & Configure (8 min)

#### Authorize GitHub & Select Repo
1. Click **Authorize GitHub**
2. Authorize DigitalOcean (GitHub login)
3. Search and select: `your-org/planmorph`
4. Check: **Autodeploy** (auto-deploy on push)
5. Click **Next**

#### Set Environment Variables (Critical!)

**Generate JWT Secret (run locally):**
```bash
openssl rand -base64 32
```

**In App Platform console, set these:**

| Variable | Value | Get From |
|----------|-------|----------|
| `JWT_SECRET_KEY` | [Result from command above] | Run command ↑ |
| `SMTP_HOST` | `smtp.gmail.com` | Email provider |
| `SMTP_PORT` | `587` | Email provider |
| `SMTP_USERNAME` | `your-email@gmail.com` | Email provider |
| `SMTP_PASSWORD` | [App password] | myaccount.google.com/apppasswords |
| `FROM_EMAIL` | `noreply@yourdomain.com` | Your domain |
| `FROM_NAME` | `PlanMorph` | Your app name |
| `PAYSTACK_SECRET_KEY` | `sk_test_xxx` | dashboard.paystack.com |
| `PAYSTACK_PUBLIC_KEY` | `pk_test_xxx` | dashboard.paystack.com |
| `PAYSTACK_WEBHOOK_SECRET` | [Webhook secret] | dashboard.paystack.com |
| `DO_SPACES_ACCESS_KEY` | [Your token] | cloud.digitalocean.com/account/api/tokens |
| `DO_SPACES_SECRET_KEY` | [Your token] | cloud.digitalocean.com/account/api/tokens |
| `DO_SPACES_BUCKET_NAME` | `planmorph-designs` | Your space name |
| `DO_SPACES_REGION` | `nyc3` | Your region |
| `DO_SPACES_ENDPOINT` | `https://nyc3.digitaloceanspaces.com` | Standard |
| `NEXT_PUBLIC_API_URL` | [Will update later] | Leave as is |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Fixed |

**Database vars:** Leave empty - App Platform auto-fills

Click **Next**

---

### ✅ Step 5: Deploy (2 min)

1. Review all settings one more time
2. Click **Create Resources**
3. Wait 5-10 minutes for deployment

You'll see progress: Building → Provisioning → Deploying → Complete ✅

---

### ✅ Step 6: Verify & Test (3 min)

**Get your app URL:**
1. App deployment completes
2. You'll see URL like: `https://planmorph-xxxxx.ondigitalocean.app`

**Test it works:**
```bash
# Frontend
https://planmorph-xxxxx.ondigitalocean.app

# API health
curl https://planmorph-xxxxx.ondigitalocean.app/api/health
# Should return: {"status":"healthy"}

# Admin
https://planmorph-xxxxx.ondigitalocean.app/admin
```

**All working? You're done! 🎉**

---

## 📋 Quick Commands Reference

### Update Your App
```bash
git add .
git commit -m "Your changes"
git push origin main
# App Platform auto-deploys!
```

### View Logs
1. Go to DigitalOcean console
2. Open your app
3. Click **Logs** tab
4. Select service: API, Client, Admin, Postgres

### Check Status
1. Go to app dashboard
2. See "Overview" section
3. All services should show "Healthy" or "Running"

### Scale Up (if slow)
1. App → Settings
2. Find service (API, Client)
3. Click "..." menu
4. Upgrade instance size ($5 → $12+/month)
5. Click Upgrade

### Add Custom Domain (Optional)
1. App → Settings
2. Scroll to "Domains"
3. Click "Edit"
4. Add: `yourdomain.com`
5. Update DNS at domain registrar
6. Wait 24h for propagation

---

## 💰 Cost Breakdown

| Component | Cost |
|-----------|------|
| API Service (basic) | $5/mo |
| Client Service (basic) | $5/mo |
| Admin Service (basic) | $5/mo |
| PostgreSQL (512MB) | $15/mo |
| **Total** | **$30/mo** |

**No other costs** - SSL, backups, monitoring all included!

---

## ✅ Success Checklist

- [ ] GitHub repo prepared
- [ ] DigitalOcean account created
- [ ] App created on App Platform
- [ ] Environment variables set
- [ ] Application deployed
- [ ] Services all showing "Healthy"
- [ ] Frontend loads without SSL errors
- [ ] API health endpoint responds
- [ ] Email test sent successfully
- [ ] Logs show no errors

---

## 🆘 Common Issues

### **Problem:** Services not healthy
```
→ Go to Logs tab
→ Look for error messages
→ Check environment variables
→ Make code fix locally
→ Push to GitHub (auto-redeploys)
```

### **Problem:** Database connection error
```
→ Go to your app dashboard
→ Click Postgres component
→ View connection credentials
→ Verify they match app.yaml
   (should be: ${db.username}:${db.password}@${db.host}:${db.port}/${db.name})
```

### **Problem:** SSL certificate not valid
```
→ Wait 2-3 minutes
→ Hard refresh (Ctrl+Shift+R)
→ Try incognito mode
→ Check Settings → Components → HTTPS status
```

### **Problem:** Need to rollback after bad deploy
```
→ Go to app → Deployments tab
→ Find last good deployment
→ Click ... menu → Rollback
→ Confirms and restores old version
```

---

## 🔐 Security Reminder

- **Never commit `.env` files** - Set variables in App Platform console
- **Protect credentials** - Use strong random passwords (32+ chars)
- **Database auto-backup** - Daily backups kept 7 days
- **SSL automatic** - Free Let's Encrypt, auto-renewed

---

## 📖 Need More Help?

- **Detailed guide:** See `DIGITALOCEAN_APPPLATFORM_DEPLOYMENT_GUIDE.md`
- **General deployment:** See `DEPLOYMENT_GUIDE.md`
- **Architecture overview:** See `README_DEPLOYMENT.md`

---

## 🎉 You're Live!

Your PlanMorph app is now running on DigitalOcean App Platform with:
- ✅ Automatic deployments on push
- ✅ Daily backups
- ✅ Free SSL certificates
- ✅ Built-in monitoring
- ✅ Zero server management
- ✅ Cost from $30/month

**Congratulations! 🚀**

