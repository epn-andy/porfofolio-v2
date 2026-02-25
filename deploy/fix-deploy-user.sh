#!/usr/bin/env bash
# deploy/fix-deploy-user.sh
#
# Run this on the VPS if the 'deploy' user was created without SSH access:
#   sudo bash deploy/fix-deploy-user.sh
#
# It fixes the shell and home directory, then prints the next step.

set -euo pipefail
DEPLOY_USER="deploy"

echo "==> Fixing deploy user shell and home directory..."

# Give the user a real shell so SSH works
usermod -s /bin/bash "${DEPLOY_USER}"

# Create home dir if missing
if [ ! -d "/home/${DEPLOY_USER}" ]; then
  mkhomedir_helper "${DEPLOY_USER}"
fi

# Fix .ssh directory
mkdir -p /home/${DEPLOY_USER}/.ssh
touch /home/${DEPLOY_USER}/.ssh/authorized_keys
chown -R ${DEPLOY_USER}:${DEPLOY_USER} /home/${DEPLOY_USER}/.ssh
chmod 700 /home/${DEPLOY_USER}/.ssh
chmod 600 /home/${DEPLOY_USER}/.ssh/authorized_keys

# Ensure sudoers entry exists
if [ ! -f /etc/sudoers.d/portfolio-deploy ]; then
  cat > /etc/sudoers.d/portfolio-deploy << EOF
${DEPLOY_USER} ALL=(ALL) NOPASSWD: /bin/systemctl restart portfolio-api, /bin/systemctl reload nginx
EOF
  chmod 440 /etc/sudoers.d/portfolio-deploy
  echo "    Sudoers entry created."
else
  cat > /etc/sudoers.d/portfolio-deploy << EOF
${DEPLOY_USER} ALL=(ALL) NOPASSWD: /bin/systemctl restart portfolio-api, /bin/systemctl reload nginx
EOF
  chmod 440 /etc/sudoers.d/portfolio-deploy
  echo "    Sudoers entry updated."
fi

echo ""
echo "======================================================"
echo " deploy user fixed!"
echo ""
echo " Now add your GitHub Actions SSH public key:"
echo ""
echo "   echo 'ssh-ed25519 AAAA...YOUR_KEY... github-actions-deploy' \\"
echo "     >> /home/${DEPLOY_USER}/.ssh/authorized_keys"
echo ""
echo " Then test from your local machine:"
echo "   ssh -i ~/.ssh/portfolio_deploy deploy@YOUR_VPS_IP 'echo OK'"
echo "======================================================"
