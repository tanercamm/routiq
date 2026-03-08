#!/usr/bin/env bash
#
# Bootstrap Let's Encrypt certificates for Routsky.
#
# Nginx requires a certificate to start, but Certbot needs Nginx running for
# the ACME HTTP-01 challenge. This script resolves the chicken-and-egg problem
# by creating a temporary self-signed cert, starting Nginx, obtaining the real
# cert, then reloading Nginx.
#
# Usage:
#   1. Set DOMAIN, API_DOMAIN, and CERTBOT_EMAIL in .env.production
#   2. Ensure DNS A records point to this server
#   3. Run: bash scripts/init-letsencrypt.sh

set -euo pipefail

COMPOSE_FILE="docker-compose.prod.yml"
ENV_FILE=".env.production"

if [ ! -f "$ENV_FILE" ]; then
  echo "ERROR: $ENV_FILE not found. Copy .env.production.example and fill in values."
  exit 1
fi

set -a
source "$ENV_FILE"
set +a

if [ -z "${DOMAIN:-}" ] || [ -z "${API_DOMAIN:-}" ] || [ -z "${CERTBOT_EMAIL:-}" ]; then
  echo "ERROR: DOMAIN, API_DOMAIN, and CERTBOT_EMAIL must be set in $ENV_FILE"
  exit 1
fi

CERT_DIR="./certbot/conf/live/$DOMAIN"

echo "==> Domains: $DOMAIN, $API_DOMAIN"
echo "==> Email:   $CERTBOT_EMAIL"
echo ""

# ── Step 1: Create dummy certificate so Nginx can start ──
echo "==> Creating temporary self-signed certificate..."
docker compose -f "$COMPOSE_FILE" run --rm --entrypoint "" certbot sh -c "\
  mkdir -p /etc/letsencrypt/live/$DOMAIN && \
  openssl req -x509 -nodes -newkey rsa:2048 -days 1 \
    -keyout /etc/letsencrypt/live/$DOMAIN/privkey.pem \
    -out /etc/letsencrypt/live/$DOMAIN/fullchain.pem \
    -subj '/CN=localhost'"

# ── Step 2: Start Nginx with the dummy cert ──
echo "==> Starting nginx-proxy with temporary certificate..."
docker compose -f "$COMPOSE_FILE" up -d nginx-proxy

echo "==> Waiting for Nginx to become ready..."
sleep 5

# ── Step 3: Remove dummy cert and request real ones ──
echo "==> Removing temporary certificate..."
docker compose -f "$COMPOSE_FILE" run --rm --entrypoint "" certbot sh -c "\
  rm -rf /etc/letsencrypt/live/$DOMAIN && \
  rm -rf /etc/letsencrypt/archive/$DOMAIN && \
  rm -rf /etc/letsencrypt/renewal/$DOMAIN.conf"

echo "==> Requesting Let's Encrypt certificate for $DOMAIN and $API_DOMAIN..."
docker compose -f "$COMPOSE_FILE" run --rm certbot certonly \
  --webroot \
  --webroot-path=/var/www/certbot \
  --email "$CERTBOT_EMAIL" \
  --agree-tos \
  --no-eff-email \
  -d "$DOMAIN" \
  -d "$API_DOMAIN"

# ── Step 4: Reload Nginx with the real certificate ──
echo "==> Reloading Nginx with production certificate..."
docker compose -f "$COMPOSE_FILE" exec nginx-proxy nginx -s reload

echo ""
echo "==> SSL certificates installed successfully!"
echo "==> Start the full stack with:"
echo "    docker compose -f $COMPOSE_FILE up -d"
