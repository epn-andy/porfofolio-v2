#!/usr/bin/env bash
# deploy/setup-server.sh
#
# ONE-TIME server bootstrap for Ubuntu 24.04 VPS (bare-metal deployment).
# Run as root or sudo:  sudo bash deploy/setup-server.sh your-domain.com

set -euo pipefail

DOMAIN="${1:?Usage: $0 <your-domain.com>}"
DEPLOY_USER="deploy"
DB_NAME="portfolio_db"
DB_USER="portfolio_user"
DB_PASS="$(openssl rand -base64 32)"
JWT_SECRET="$(openssl rand -base64 48)"

echo "==> [1/7] System update"
apt-get update -qq && apt-get upgrade -y -qq

echo "==> [2/7] Install Nginx & Certbot"
apt-get install -y -qq nginx certbot python3-certbot-nginx

echo "==> [3/7] Install .NET 10 runtime"
wget -q https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb \
  -O /tmp/packages-microsoft-prod.deb
dpkg -i /tmp/packages-microsoft-prod.deb
apt-get update -qq
apt-get install -y -qq dotnet-runtime-10.0 aspnetcore-runtime-10.0

echo "==> [4/7] Install PostgreSQL"
apt-get install -y -qq postgresql postgresql-contrib

sudo -u postgres psql -c "CREATE USER ${DB_USER} WITH PASSWORD '${DB_PASS}';" 2>/dev/null || true
sudo -u postgres psql -c "CREATE DATABASE ${DB_NAME} OWNER ${DB_USER};" 2>/dev/null || true

echo "==> [5/7] Create directories & deploy user"
mkdir -p /var/www/portfolio /var/www/portfolioapi /etc/portfolio

# Deploy user (used by GitHub Actions rsync + systemctl)
id "${DEPLOY_USER}" &>/dev/null || useradd --system --no-create-home --shell /usr/sbin/nologin "${DEPLOY_USER}"
mkdir -p /home/${DEPLOY_USER}/.ssh
touch /home/${DEPLOY_USER}/.ssh/authorized_keys
chown -R ${DEPLOY_USER}:${DEPLOY_USER} /home/${DEPLOY_USER}/.ssh
chmod 700 /home/${DEPLOY_USER}/.ssh && chmod 600 /home/${DEPLOY_USER}/.ssh/authorized_keys

# Let deploy user restart service and reload nginx
cat > /etc/sudoers.d/portfolio-deploy << EOF
${DEPLOY_USER} ALL=(ALL) NOPASSWD: /bin/systemctl restart portfolio-api, /bin/systemctl reload nginx
EOF
chmod 440 /etc/sudoers.d/portfolio-deploy

# Write permissions on web dirs
chown -R ${DEPLOY_USER}:www-data /var/www/portfolio /var/www/portfolioapi
chmod -R 775 /var/www/portfolio /var/www/portfolioapi

echo ""
echo "    Add your GitHub Actions SSH public key:"
echo "    echo '<pubkey>' >> /home/${DEPLOY_USER}/.ssh/authorized_keys"
echo ""

echo "==> [6/7] Write /etc/portfolio/env"
cat > /etc/portfolio/env << EOF
ConnectionStrings__DefaultConnection=Host=localhost;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASS}
JwtSettings__SecretKey=${JWT_SECRET}
JwtSettings__Issuer=PortfolioAPI
JwtSettings__Audience=PortfolioApp
JwtSettings__ExpiryMinutes=1440
AllowedOrigins__0=https://${DOMAIN}
EOF
chmod 600 /etc/portfolio/env
chown root:www-data /etc/portfolio/env

echo "    Secrets saved to /etc/portfolio/env"

echo "==> [7/7] Install systemd service, Nginx vhost & TLS"

# Systemd service
cp /tmp/portfolio-v2/deploy/portfolio-api.service /etc/systemd/system/
systemctl daemon-reload
systemctl enable portfolio-api

# Nginx vhost
sed "s/YOUR_DOMAIN/${DOMAIN}/g" /tmp/portfolio-v2/deploy/nginx.conf \
  > /etc/nginx/sites-available/portfolio
ln -sf /etc/nginx/sites-available/portfolio /etc/nginx/sites-enabled/portfolio
rm -f /etc/nginx/sites-enabled/default
nginx -t

# TLS via Let's Encrypt
certbot --nginx -d "${DOMAIN}" -d "www.${DOMAIN}" \
  --non-interactive --agree-tos --email "admin@${DOMAIN}" --redirect

systemctl reload nginx

echo ""
echo "======================================================"
echo " Setup complete for ${DOMAIN}"
echo " Secrets: /etc/portfolio/env"
echo ""
echo " Next steps:"
echo "   1. Add SSH public key to /home/${DEPLOY_USER}/.ssh/authorized_keys"
echo "   2. Add GitHub Secrets (see deploy/GITHUB_SECRETS.md)"
echo "   3. Run EF migrations:"
echo "      cd /var/www/portfolioapi && dotnet PortfolioAPI.dll --migrate"
echo "   4. Seed admin:  dotnet PortfolioAPI.dll --seed"
echo "   5. Push to main → auto-deploys!"
echo "======================================================"
