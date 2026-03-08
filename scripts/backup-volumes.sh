#!/usr/bin/env bash
#
# Backup Routsky database and uploads volume.
#
# Creates timestamped backups:
#   - PostgreSQL dump (compressed .sql.gz)
#   - Uploads directory tarball
#
# Usage:
#   bash scripts/backup-volumes.sh [backup_directory]
#
# Recommended: run daily via cron
#   0 3 * * * cd /opt/routsky && bash scripts/backup-volumes.sh

set -euo pipefail

BACKUP_DIR="${1:-./backups}"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
COMPOSE_FILE="docker-compose.prod.yml"

mkdir -p "$BACKUP_DIR"

# ── Database Backup ──
DB_BACKUP="$BACKUP_DIR/routsky_db_${TIMESTAMP}.sql.gz"
echo "==> Backing up PostgreSQL to $DB_BACKUP ..."
docker compose -f "$COMPOSE_FILE" exec -T db \
  pg_dump -U postgres routsky_db | gzip > "$DB_BACKUP"
echo "    Database backup complete ($(du -h "$DB_BACKUP" | cut -f1))"

# ── Uploads Backup ──
UPLOADS_BACKUP="$BACKUP_DIR/routsky_uploads_${TIMESTAMP}.tar.gz"
echo "==> Backing up uploads volume to $UPLOADS_BACKUP ..."
docker run --rm \
  -v routsky_routsky-uploads:/data:ro \
  -v "$(pwd)/$BACKUP_DIR":/backup \
  alpine tar czf "/backup/routsky_uploads_${TIMESTAMP}.tar.gz" -C /data .
echo "    Uploads backup complete ($(du -h "$UPLOADS_BACKUP" | cut -f1))"

# ── Cleanup: keep last 7 days ──
echo "==> Pruning backups older than 7 days..."
find "$BACKUP_DIR" -name "routsky_*" -mtime +7 -delete 2>/dev/null || true

echo "==> Backup complete: $TIMESTAMP"
