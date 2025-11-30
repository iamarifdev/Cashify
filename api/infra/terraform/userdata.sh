#!/bin/bash
set -euo pipefail

LOG_FILE="/var/log/cashify-init.log"
exec > >(tee -a "$LOG_FILE") 2>&1

echo "===== Cashify Instance Bootstrap Started ====="

# -----------------------------
# CONFIG
# -----------------------------
DEPLOY_USER="deploy"
CUSTOM_SSH_PORT=50022
APP_DIR="/opt/cashify"
REPO_URL="https://github.com/iamarifdev/Cashify.git"

SSHD_CONFIG="/etc/ssh/sshd_config"
SSHD_CONFIGD_DIR="/etc/ssh/sshd_config.d"

# Wait for cloud-init to finish (prevents race conditions)
cloud-init status --wait || true


# =====================================================
# 1. SYSTEM PREP
# =====================================================
echo "[1/7] Updating system packages..."
apt-get update -y
apt-get upgrade -y


# =====================================================
# 2. CREATE DEPLOY USER SAFELY
# =====================================================
echo "[2/7] Creating deploy user if missing..."

if ! id "$DEPLOY_USER" >/dev/null 2>&1; then
    adduser --disabled-password --gecos "" "$DEPLOY_USER"
fi

# Passwordless sudo
echo "${DEPLOY_USER} ALL=(ALL) NOPASSWD:ALL" >/etc/sudoers.d/$DEPLOY_USER
chmod 440 /etc/sudoers.d/$DEPLOY_USER

# Copy SSH keys from ubuntu → deploy
mkdir -p /home/$DEPLOY_USER/.ssh
if [[ -f /home/ubuntu/.ssh/authorized_keys ]]; then
    cp /home/ubuntu/.ssh/authorized_keys /home/$DEPLOY_USER/.ssh/authorized_keys
fi

chown -R $DEPLOY_USER:$DEPLOY_USER /home/$DEPLOY_USER/.ssh
chmod 700 /home/$DEPLOY_USER/.ssh
chmod 600 /home/$DEPLOY_USER/.ssh/authorized_keys || true


# =====================================================
# 3. INSTALL DOCKER + COMPOSE (SAFE FOR UBUNTU 24.04)
# =====================================================
echo "[3/7] Installing Docker Engine + Compose v2..."

apt-get remove -y docker docker.io docker-engine docker-compose-plugin || true
install -m 0755 -d /etc/apt/keyrings

curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
    | tee /etc/apt/keyrings/docker.asc >/dev/null
chmod a+r /etc/apt/keyrings/docker.asc

# Add Docker repo
echo \
"deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
 https://download.docker.com/linux/ubuntu noble stable" \
    >/etc/apt/sources.list.d/docker.list

apt-get update -y
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

systemctl enable docker
systemctl start docker

# Allow deploy user to run docker
usermod -aG docker "$DEPLOY_USER"


# =====================================================
# 4. SAFE SSH CONFIGURATION (NO LOCKOUT)
# =====================================================
echo "[4/7] Configuring SSH safely..."

# Ensure BOTH ports exist (idempotent)
grep -q "^Port 22" "$SSHD_CONFIG" || echo "Port 22" >> "$SSHD_CONFIG"
grep -q "^Port $CUSTOM_SSH_PORT" "$SSHD_CONFIG" || echo "Port $CUSTOM_SSH_PORT" >> "$SSHD_CONFIG"

# Ensure AddressFamily inet (prevents IPv6-only binding)
if ! grep -q "^AddressFamily" "$SSHD_CONFIG"; then
    echo "AddressFamily inet" >> "$SSHD_CONFIG"
fi

# Remove cloud-init instance-connect port overrides
for f in $SSHD_CONFIGD_DIR/*.conf; do
    sed -i 's/^Port/#Port/' "$f" || true
done

# Validate before restart
if sshd -t; then
    echo "SSH config valid. Restarting ssh..."
    systemctl restart ssh
else
    echo "SSH config invalid. NOT restarting ssh to avoid lockout!"
fi


# =====================================================
# 5. FIREWALL (UFW) — NEVER BLOCKS SSH 22
# =====================================================
echo "[5/7] Configuring UFW..."

ufw --force reset

ufw default deny incoming
ufw default allow outgoing

# Always allow both ports
ufw allow 22/tcp
ufw allow $CUSTOM_SSH_PORT/tcp

# HTTP/HTTPS
ufw allow 80/tcp
ufw allow 443/tcp

ufw --force enable


# =====================================================
# 6. OPTIONAL — CLONE APP DIRECTORY
# =====================================================
echo "[6/7] Preparing app directory..."

mkdir -p "$APP_DIR"

if [[ ! -d "$APP_DIR/.git" ]]; then
    git clone "$REPO_URL" "$APP_DIR"
else
    cd "$APP_DIR"
    git pull || true
fi

chown -R $DEPLOY_USER:$DEPLOY_USER "$APP_DIR"


# =====================================================
# 7. FINAL CHECKS
# =====================================================
echo "[7/7] Final checks..."

sshd -T | grep port
systemctl status ssh --no-pager || true
ufw status verbose

echo "===== Cashify Instance Bootstrap Finished Successfully ====="
