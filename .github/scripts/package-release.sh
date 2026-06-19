#!/usr/bin/env bash
# Package Moonfin.Server release ZIP for CI and release workflows.
set -euo pipefail

ROOT_DIR="${GITHUB_WORKSPACE:-$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)}"
BACKEND_DIR="$ROOT_DIR/backend"
FRONTEND_DIR="$ROOT_DIR/frontend"
PLUGIN_GUID="8c5d0e91-4f2a-4b6d-9e3f-1a7c8d9e0f2b"

# Version must match the built DLL assembly version. Jellyfin displays the assembly
# version in the admin UI (not the plugin-catalog version on its own).
VERSION="${1:-$(grep -oPm1 '(?<=<AssemblyVersion>)[^<]+' "$BACKEND_DIR/Moonfin.Server.csproj")}"
TARGET_ABI="${2:-$(grep 'Jellyfin.Controller' "$BACKEND_DIR/Moonfin.Server.csproj" | grep -oP 'Version="\K[^"]+').0}"
TIMESTAMP="${3:-$(date -u +"%Y-%m-%dT%H:%M:%SZ")}"

RELEASE_DIR="$ROOT_DIR/release"
rm -rf "$RELEASE_DIR"
mkdir -p "$RELEASE_DIR"

cp "$BACKEND_DIR/bin/Release/net8.0/Moonfin.Server.dll" "$RELEASE_DIR/"

if [ -f "$FRONTEND_DIR/index.html" ]; then
  mkdir -p "$RELEASE_DIR/frontend"
  cp -R "$FRONTEND_DIR/." "$RELEASE_DIR/frontend/"
  rm -rf "$RELEASE_DIR/frontend/node_modules"
  rm -f "$RELEASE_DIR/frontend/package.json" "$RELEASE_DIR/frontend/package-lock.json"
fi

cat > "$RELEASE_DIR/meta.json" <<EOF
{
  "category": "General",
  "changelog": "",
  "description": "Moonfin brings a modern TV-style UI to Jellyfin web. Features include: custom navbar, media bar with featured content, Jellyseerr integration, and cross-device settings synchronization.",
  "guid": "${PLUGIN_GUID}",
  "name": "Moonfin",
  "overview": "Custom UI and settings sync for Jellyfin",
  "owner": "RadicalMuffinMan",
  "targetAbi": "${TARGET_ABI}",
  "timestamp": "${TIMESTAMP}",
  "version": "${VERSION}",
  "status": "Active",
  "autoUpdate": true,
  "assemblies": ["Moonfin.Server.dll"]
}
EOF

ZIP_NAME="Moonfin.Server-${VERSION}.zip"
rm -f "$ROOT_DIR/$ZIP_NAME"
(cd "$RELEASE_DIR" && zip -r "$ROOT_DIR/$ZIP_NAME" .)

CHECKSUM=$(md5sum "$ROOT_DIR/$ZIP_NAME" | awk '{print toupper($1)}')

echo "zip_name=$ZIP_NAME"
echo "checksum=$CHECKSUM"
echo "version=$VERSION"
echo "target_abi=$TARGET_ABI"
echo "timestamp=$TIMESTAMP"
