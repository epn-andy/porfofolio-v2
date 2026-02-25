# �� Deployment Setup Guide

Complete step-by-step instructions to get the portfolio live on your Ubuntu 24.04 VPS.

**Local machine: Windows | Server: Ubuntu 24.04**

---

## Prerequisites

Before starting, make sure you have:
- [ ] A Ubuntu 24.04 VPS with root access
- [ ] Domain `nayreisme.dev` DNS pointed to your VPS IP (A record) via Cloudflare
- [ ] Cloudflare SSL mode set to **Full (strict)** in the Cloudflare dashboard
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

### Step 3: Upload Cloudflare Origin Certificate to VPS

Before running the bootstrap script, upload your Cloudflare Origin Certificate to the VPS.

**On the Cloudflare dashboard:**
1. Go to your domain → **SSL/TLS** → **Origin Server**
2. Click **Create Certificate** → keep defaults → **Create**
3. Copy the **Origin Certificate** (`.crt`) and **Private Key** (`.key`) contents

**Upload to VPS** — open **Git Bash** locally and run:

```bash
# SSH into VPS
ssh root@YOUR_VPS_IP

# Create the SSL directory
mkdir -p /etc/nginx/ssl

# Create the cert file (paste Cloudflare cert content, then Ctrl+D)
cat > /etc/nginx/ssl/cloudflare.crt

# Create the key file (paste Cloudflare private key content, then Ctrl+D)
cat > /etc/nginx/ssl/cloudflare.key

chmod 600 /etc/nginx/ssl/cloudflare.key
```

> **Tip:** You can also use `scp` from Git Bash if you saved the files locally:
> ```bash
> scp cloudflare.crt root@YOUR_VPS_IP:/etc/nginx/ssl/
> scp cloudflare.key root@YOUR_VPS_IP:/etc/nginx/ssl/
> ```

### Step 4: Run the bootstrap script

Still inside the VPS terminal, run:

```bash
git clone https://github.com/epn-andy/porfofolio-v2.git /tmp/portfolio-v2
sudo bash /tmp/portfolio-v2/deploy/setup-server.sh
```

This script automatically:
- Installs Nginx, .NET 10 runtime, PostgreSQL
- Creates the `deploy` user for GitHub Actions
- Creates `/var/www/portfolio` and `/var/www/portfolioapi` directories
- Generates secure random secrets → saves to `/etc/portfolio/env`
- Installs the systemd service for the .NET API
- Configures Nginx with `nayreisme.dev` using your Cloudflare cert

### Step 5: Add the SSH public key to the deploy user

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

### Step 6: Add GitHub Secrets

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

### Step 7: (Recommended) Create a production environment with approval gate

Go to **Settings** → **Environments** → **New environment** → name it exactly `production`

Configure it:
- ✅ Check **Required reviewers** → add your GitHub username
- Under **Deployment branches** → select **Selected branches** → add `main`

This means every deploy to your live server requires your manual approval click — preventing accidental pushes from going straight to production.

---

## Part 4 — First Deploy

### Step 8: Push to main and trigger the pipeline

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

### Step 9: Verify the deploy succeeded

Open **Git Bash** or **PowerShell** and run:

```bash
# Check the API responds
curl https://nayreisme.dev/api/articles

# Check the API service is running on the VPS
ssh deploy@YOUR_VPS_IP "sudo systemctl status portfolio-api"
```

Then open your browser and go to `https://nayreisme.dev` — you should see the portfolio.

---

## Part 5 — After First Deploy

### Step 10: Run database migrations

SSH back into the VPS **as root** (or deploy user):

```bash
ssh root@YOUR_VPS_IP
cd /var/www/portfolioapi
dotnet PortfolioAPI.dll --migrate
```

This creates all the database tables (Articles, Projects, JobHistory, Admins).

### Step 11: Seed the admin user

Still on the VPS, edit the env file to temporarily add your plain-text password:

```bash
sudo nano /etc/portfolio/env
```

Add these two lines at the bottom (use your own email and a strong password):

```
AdminSettings__Email=admin@nayreisme.dev
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

### Step 12: Log into the admin dashboard

Open your browser and go to:

```
https://nayreisme.dev/admin/login
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

### GitHub Actions: `Permission denied (publickey,password)` / rsync error 255

This means the `deploy` user on the VPS either has no SSH key, no home directory, or was created with `/usr/sbin/nologin` shell.

**Fix — run this on the VPS:**

```bash
ssh root@YOUR_VPS_IP
sudo bash /tmp/portfolio-v2/deploy/fix-deploy-user.sh
```

Or manually:

```bash
# Fix shell so SSH is allowed
sudo usermod -s /bin/bash deploy

# Fix .ssh directory
sudo mkdir -p /home/deploy/.ssh
sudo touch /home/deploy/.ssh/authorized_keys
sudo chmod 700 /home/deploy/.ssh
sudo chmod 600 /home/deploy/.ssh/authorized_keys
sudo chown -R deploy:deploy /home/deploy/.ssh

# Add your public key
echo "ssh-ed25519 AAAA...YOUR_KEY..." | sudo tee -a /home/deploy/.ssh/authorized_keys
```

**Verify from your local Windows machine (Git Bash):**

```bash
ssh -i ~/.ssh/portfolio_deploy deploy@YOUR_VPS_IP "echo OK"
```

If it prints `OK`, re-run the GitHub Actions workflow.

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

### Cloudflare certificate expired or not loading

Cloudflare Origin Certificates are valid for up to 15 years. If you see SSL errors:
1. Check `/etc/nginx/ssl/cloudflare.crt` and `.key` are in place
2. Verify Cloudflare SSL mode is **Full (strict)** (not Flexible)
3. Run `sudo nginx -t && sudo systemctl reload nginx`

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
