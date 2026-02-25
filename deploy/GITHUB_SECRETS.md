# GitHub Actions — Required Secrets

Go to your repo → **Settings → Secrets and variables → Actions → New repository secret**

| Secret name   | Value                                                     |
|---------------|-----------------------------------------------------------|
| `VPS_HOST`    | Your VPS IP or domain (e.g. `123.45.67.89`)              |
| `VPS_USER`    | SSH user (e.g. `deploy`)                                  |
| `VPS_SSH_KEY` | ed25519 private key — steps below                         |

> `GITHUB_TOKEN` is automatic — no need to add it manually.

---

## Generate a deploy SSH key pair (run locally)

```bash
ssh-keygen -t ed25519 -C "github-actions-deploy" -f ~/.ssh/portfolio_deploy -N ""
```

### Add public key to VPS

```bash
ssh root@YOUR_VPS_IP
echo "$(cat ~/.ssh/portfolio_deploy.pub)" >> /home/deploy/.ssh/authorized_keys
```

### Add private key to GitHub

```bash
cat ~/.ssh/portfolio_deploy   # copy entire output
```

Paste as the value of `VPS_SSH_KEY`.

---

## GitHub Environment (recommended)

Create a **`production`** environment (Settings → Environments):
- Require **manual approval** before deploy runs
- Restrict to `main` branch only

---

## Full pipeline

```
git push origin main
        │
        ▼
┌──────────────────┐   ┌──────────────────┐
│  build-backend   │   │  build-frontend  │  ← run in parallel
│  dotnet publish  │   │  npm run build   │
│  → artifact      │   │  → artifact      │
└────────┬─────────┘   └────────┬─────────┘
         └──────────┬───────────┘
                    ▼
         ┌──────────────────────────┐
         │  deploy                  │
         │  rsync backend → VPS     │
         │  rsync frontend → VPS    │
         │  systemctl restart api   │
         │  nginx reload            │
         │  health check            │
         └──────────────────────────┘
```

## Rollback

SSH into the VPS and re-run a previous workflow via GitHub UI (Actions → select run → Re-run jobs),
or revert the commit and push:

```bash
git revert HEAD && git push origin main
```
