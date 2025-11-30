#!/bin/bash
set -euo pipefail

# ------------------------------------------------------------
# CONFIGURATION
# ------------------------------------------------------------
DEPLOY_USER="deploy"
SSH_PORT=50022
APP_DIR="/opt/cashify"
REPO_URL="https://github.com/iamarifdev/Cashify.git"
PUBLIC_SSH_KEY="${HOME}/.ssh/authorized_keys"

echo "🚀 Starting server bootstrap for cashify..."

# ------------------------------------------------------------
# ROOT CHECK
# ------------------------------------------------------------
if [[ $EUID -ne 0 ]]; then
  echo "❌ This script must be run as root. Try: sudo bash setup-server.sh"
  exit 1
fi

# ------------------------------------------------------------
# UPDATE SYSTEM
# ------------------------------------------------------------
echo "📦 Updating system packages..."
apt-get update -y
apt-get upgrade -y

# ------------------------------------------------------------
# INSTALL BASE TOOLS
# ------------------------------------------------------------
echo "📦 Installing base packages..."
apt-get install -y ca-certificates curl wget ufw git build-essential fail2ban

# ------------------------------------------------------------
# CREATE DEPLOY USER
# ------------------------------------------------------------
echo "👤 Checking deploy user..."
if id "$DEPLOY_USER" &>/dev/null; then
    echo "ℹ️ User '$DEPLOY_USER' already exists."
else
    echo "🟢 Creating deploy user..."
    useradd -m -s /bin/bash "$DEPLOY_USER"
fi

# ------------------------------------------------------------
# SUDO WITHOUT PASSWORD
# ------------------------------------------------------------
echo "🔐 Configuring passwordless sudo for deploy user..."
if [[ ! -f "/etc/sudoers.d/${DEPLOY_USER}" ]]; then
    echo "${DEPLOY_USER} ALL=(ALL) NOPASSWD:ALL" >/etc/sudoers.d/${DEPLOY_USER}
    chmod 440 /etc/sudoers.d/${DEPLOY_USER}
else
    echo "ℹ️ Sudoers entry already exists."
fi

# ------------------------------------------------------------
# COPY ROOT/UBUNTU AUTHORIZED KEYS TO DEPLOY USER
# ------------------------------------------------------------
echo "🔑 Setting up SSH keys for deploy user..."
mkdir -p /home/$DEPLOY_USER/.ssh
chmod 700 /home/$DEPLOY_USER/.ssh

AUTHORIZED_KEY_SRC="/home/ubuntu/.ssh/authorized_keys"
AUTHORIZED_KEY_DEST="/home/$DEPLOY_USER/.ssh/authorized_keys"

if [[ -f "$AUTHORIZED_KEY_SRC" ]]; then
    cp "$AUTHORIZED_KEY_SRC" "$AUTHORIZED_KEY_DEST"
else
    echo "⚠️ No authorized_keys found for ubuntu user."
fi

chmod 600 "$AUTHORIZED_KEY_DEST"
chown -R $DEPLOY_USER:$DEPLOY_USER /home/$DEPLOY_USER/.ssh

# ------------------------------------------------------------
# INSTALL DOCKER ENGINE + DOCKER COMPOSE V2
# ------------------------------------------------------------
echo "🐳 Installing Docker Engine + Docker Compose v2..."

# Remove old versions
apt-get remove -y docker docker.io docker-engine docker-compose-plugin || true

# Add Docker’s official GPG key
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg \
    | tee /etc/apt/keyrings/docker.asc >/dev/null
chmod a+r /etc/apt/keyrings/docker.asc

# Add official Docker repo
echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.asc] \
  https://download.docker.com/linux/ubuntu noble stable" \
  | tee /etc/apt/sources.list.d/docker.list >/dev/null

apt-get update -y

# Install docker engine & compose plugin
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

systemctl enable docker
systemctl start docker

# Add deploy user to docker group
usermod -aG docker "$DEPLOY_USER"

# ------------------------------------------------------------
# APP DIRECTORY SETUP
# ------------------------------------------------------------
echo "📁 Preparing application directory: $APP_DIR"

if [[ ! -d "$APP_DIR" ]]; then
    echo "🟢 Creating app folder & cloning repo..."
    mkdir -p "$APP_DIR"
    git clone "$REPO_URL" "$APP_DIR"
else
    echo "ℹ️ App directory exists. Pulling updates..."
    (cd "$APP_DIR" && git pull || true)
fi

chown -R $DEPLOY_USER:$DEPLOY_USER "$APP_DIR"

# ------------------------------------------------------------
# UFW FIREWALL SETUP
# ------------------------------------------------------------
echo "🛡 Configuring UFW firewall..."

ufw allow $SSH_PORT/tcp
ufw allow 80/tcp
ufw allow 443/tcp

ufw --force enable

# ------------------------------------------------------------
# FAIL2BAN SETUP
# ------------------------------------------------------------
echo "🛡 Configuring Fail2Ban..."

cat >/etc/fail2ban/jail.local <<EOF
[sshd]
enabled = true
port = $SSH_PORT
logpath = /var/log/auth.log
maxretry = 5
bantime = 10m
findtime = 10m
EOF

systemctl restart fail2ban

# ------------------------------------------------------------
# SSH PORT CHANGE (WITH SAFETY GUARANTEE)
# ------------------------------------------------------------
echo "🔐 Changing SSH port to $SSH_PORT safely..."

SSHD_CONFIG="/etc/ssh/sshd_config"

if ! grep -q "Port $SSH_PORT" "$SSHD_CONFIG"; then
    sed -i "s/^#Port 22/Port $SSH_PORT/" "$SSHD_CONFIG" || true
    sed -i "s/^Port 22/Port $SSH_PORT/" "$SSHD_CONFIG" || true
fi

# Test SSH config before restarting
echo "🧪 Testing SSH configuration..."
if sshd -t; then
    echo "🟢 SSH config OK. Restarting SSH..."
    systemctl restart ssh
else
    echo "❌ SSH config invalid. Not restarting SSH to avoid lockout."
fi

echo ""
echo "✅ Setup complete!"
echo "➡️ SSH now available via:"
echo ""
echo "ssh -i your-key.pem -p $SSH_PORT ubuntu@<server-ip>"
echo "ssh -i your-key.pem -p $SSH_PORT deploy@<server-ip>"
echo ""
echo "🚀 Server is now ready for GitHub Actions deployment."
