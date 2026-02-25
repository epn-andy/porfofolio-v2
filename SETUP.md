# �� Deployment Setup Guide

Complete step-by-step instructions to get the portfolio live on your Ubuntu 24.04 VPS.

**Local machine: Windows | Server: Ubuntu 24.04**

---

## Prerequisites

Before starting, make sure you have:
- [ ] A Ubuntu 24.04 VPS with root access
- [ ] A domain name pointed to your VPS IP (A record)
- [ ] This repo pushed to GitHub
- [ ] [Git for Windows](https://git-scm.com/download/win) installed (includes Git Bash)
- [ ] [Windows Terminal](https://aka.ms/terminal) recommended

> **All local commands below use PowerShell or Git Bash.** Git Bash is recommended for SSH commands since it includes `ssh-keygen`, `ssh`, and `scp` out of the box.

---

## Part 1 — Local Machine (Windows)

### Step 1: Generate a deploy SSH key pair

Open **Git Bash** (right-click your project folder → "Open Git Bash here") and run:

```bash
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/portfolio_deploy -N ""
```

This creates two files in `C:\Users\YOUR_USERNAME\.ssh\`:
- `portfolio_deploy`     → **private key** (goes to GitHub)
- `portfolio_deploy.pub` → **public key** (goes to VPS)

To view your public key (you'll need it in Step 4):

```bash
cat ~/.ssh/portfolio_deploy.pub
```

To view your private key (you'll need it in Step 7):

```bash
cat ~/.ssh/portfolio_deploy
```

---

## Part 2 — VPS Server (Ubuntu 24.04)

### Step 2: SSH into your VPS

Open **Git Bash** or **PowerShell** and run:

```bash
ssh root@YOUR_VPS_IP
```

> If this is your first time connecting, type `yes` when asked about the host fingerprint.

### Step 3: Clone the repo & run the bootstrap script

Once you are inside the VPS (your terminal prompt will change), run:

```bash
git clone https://github.com/YOUR_GITHUB_USERNAME/porfofolio-v2.git /tmp/portfolio-v2
sudo bash /tmp/portfolio-v2/deploy/setup-server.sh your-domain.com
```

> Replace `your-domain.com` with your actual domain (e.g. `eryandhi.dev`).  
> Make sure your domain DNS A record already points to your VPS IP before running this — Certbot needs it to issue the TLS certificate.

This script automatically:
- Installs Nginx, .NET 10 runtime, PostgreSQL
- Creates the `deploy` user for GitHub Actions
- Creates `/var/www/portfolio` and `/var/www/portfolioapi` directories
- Generates secure random secrets → saves to `/etc/portfolio/env`
- Installs the systemd service for the .NET API
- Configures Nginx with your domain + HTTP→HTTPS redirect
- Issues a free TLS certificate via Let's Encrypt

### Step 4: Add the SSH public key to the deploy user

Still inside the VPS terminal, run:

```bash
mkdir -p /home/deploy/.ssh
echo "PASTE_YOUR_PUBLIC_KEY_HERE" >> /home/deploy/.ssh/authorized_keys
chmod 600 /home/deploy/.ssh/authorized_keys
chown -R deploy:deploy /home/deploy/.ssh
```

Replace `PASTE_YOUR_PUBLIC_KEY_HERE` with the output of `cat ~/.ssh/portfolio_deploy.pub`
that you ran in Step 1. It looks like:

```
ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAA... github-actions-deploy
```

> **Tip:** Keep two terminal windows open — one connected to the VPS, one local Git Bash — so you can copy-paste easily.

---

## Part 3 — GitHub

### Step 5: Add GitHub Secrets

Go to your GitHub repo → **Settings** → **Secrets and variables** → **Actions** → **New repository secret**

Add these 3 secrets one by one:

#### `VPS_HOST`
Your VPS IP address, e.g.:
```
123.45.67.89
```

#### `VPS_USER`
```
deploy
```

#### `VPS_SSH_KEY`
Open **Git Bash** locally and run:
```bash
cat ~/.ssh/portfolio_deploy
```
Copy the **entire** output — including the header and footer lines:
```
-----BEGIN OPENSSH PRIVATE KEY-----
...
-----END OPENSSH PRIVATE KEY-----
```
Paste the whole thing as the secret value.

### Step 6: (Recommended) Create a production environment with approval gate

Go to **Settings** → **Environments** → **New environment** → name it exactly `production`

Configure it:
- ✅ Check **Required reviewers** → add your GitHub username
- Under **Deployment branches** → select **Selected branches** → add `main`

This means every deploy to your live server requires your manual approval click — preventing accidental pushes from going straight to production.

---

## Part 4 — First Deploy

### Step 7: Push to main and trigger the pipeline

In **PowerShell** or **Git Bash**, from your project folder:

```powershell
git add .
git commit -m "initial deployment"
git push origin main
```

Then go to your repo → **Actions** tab. You will see the workflow running:

```
build-backend  ──┐
                  ├──▶  deploy (waits for your approval if you did Step 6)
build-frontend ──┘
```

If you set up the production environment, click **Review deployments** → tick `production` → **Approve and deploy**.

### Step 8: Verify the deploy succeeded

Open **Git Bash** or **PowerShell** and run:

```bash
# Check the API responds
curl https://your-domain.com/api/articles

# Check the API service is running on the VPS
ssh deploy@YOUR_VPS_IP "sudo systemctl status portfolio-api"
```

Then open your browser and go to `https://your-domain.com` — you should see the portfolio.

---

## Part 5 — After First Deploy

### Step 9: Run database migrations

SSH back into the VPS **as root** (or deploy user):

```bash
ssh root@YOUR_VPS_IP
cd /var/www/portfolioapi
dotnet PortfolioAPI.dll --migrate
```

This creates all the database tables (Articles, Projects, JobHistory, Admins).

### Step 10: Seed the admin user

Still on the VPS, edit the env file to temporarily add your plain-text password:

```bash
sudo nano /etc/portfolio/env
```

Add these two lines at the bottom (use your own email and a strong password):

```
AdminSettings__Email=admin@your-domain.com
AdminSettings__PlainPassword=YourStrongPasswordHere
```

Save and exit (`Ctrl+O`, `Enter`, `Ctrl+X`), then run the seeder:

```bash
cd /var/www/portfolioapi
dotnet PortfolioAPI.dll --seed
```

**Important — remove the plain-text password immediately after:**

```bash
sudo nano /etc/portfolio/env
# Delete the AdminSettings__PlainPassword line
# Save: Ctrl+O, Enter, Ctrl+X
sudo systemctl restart portfolio-api
```

### Step 11: Log into the admin dashboard

Open your browser and go to:

```
https://your-domain.com/admin/login
```

Sign in with the email and password you set in Step 10.

---

## Ongoing — How to deploy updates

Every time you make changes, just push to `main`:

```powershell
git add .
git commit -m "describe your changes"
git push origin main
```

GitHub Actions will automatically build and deploy to your VPS. If you set up the production environment, approve the deploy in the Actions tab.

---

## Troubleshooting

### API is not starting or returning errors

SSH into the VPS and check the logs:

```bash
ssh root@YOUR_VPS_IP
sudo journalctl -u portfolio-api -n 100 --no-pager
```

### Nginx is returning 502 or 404

```bash
# Test nginx config for syntax errors
sudo nginx -t

# Check nginx logs
sudo journalctl -u nginx -n 50 --no-pager
```

### Check that secrets/env are loaded correctly

```bash
sudo cat /etc/portfolio/env
```

### TLS certificate renewal (runs automatically, but test it manually)

```bash
sudo certbot renew --dry-run
```

### SSH connection refused from GitHub Actions

Make sure the public key was added correctly:

```bash
ssh root@YOUR_VPS_IP
cat /home/deploy/.ssh/authorized_keys
```

It should contain a line starting with `ssh-ed25519 ...`.

### Reset/check the deploy user permissions

```bash
sudo cat /etc/sudoers.d/portfolio-deploy
```

Should contain:
```
deploy ALL=(ALL) NOPASSWD: /bin/systemctl restart portfolio-api, /bin/systemctl reload nginx
```
