#!/bin/bash

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REGISTRY="${REGISTRY:-192.168.1.178:5000}"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

print_info()    { echo -e "${GREEN}[INFO]${NC} $1"; }
print_error()   { echo -e "${RED}[ERROR]${NC} $1"; }
print_warning() { echo -e "${YELLOW}[WARNING]${NC} $1"; }

if ! docker info > /dev/null 2>&1; then
    print_error "Docker is not running."
    exit 1
fi

print_info "Checking registry at ${REGISTRY}..."
if ! curl -s "http://${REGISTRY}/v2/" > /dev/null; then
    print_warning "Registry at ${REGISTRY} may not be accessible."
    read -p "Continue anyway? (y/n) " -n 1 -r
    echo
    [[ $REPLY =~ ^[Yy]$ ]] || exit 1
fi

FAILED=0

# API — Dockerfile is at the solution root, context is solution root
print_info "Building game-collection-api..."
if docker build -t "${REGISTRY}/game-collection-api:latest" "$SCRIPT_DIR"; then
    print_info "Pushing game-collection-api..."
    docker push "${REGISTRY}/game-collection-api:latest"
    print_info "game-collection-api done"
else
    print_error "Failed to build game-collection-api"
    FAILED=1
fi

echo ""

# Client — Dockerfile is in GameCollectionManager.Client/, context is solution root
print_info "Building game-collection-client..."
if docker build -f "$SCRIPT_DIR/GameCollectionManager.Client/Dockerfile" \
               -t "${REGISTRY}/game-collection-client:latest" "$SCRIPT_DIR"; then
    print_info "Pushing game-collection-client..."
    docker push "${REGISTRY}/game-collection-client:latest"
    print_info "game-collection-client done"
else
    print_error "Failed to build game-collection-client"
    FAILED=1
fi

echo ""
echo "========================================="
if [ $FAILED -eq 0 ]; then
    print_info "Both images built and pushed successfully."
    print_info "Deploy with: kubectl apply -f k8s/deployment.yaml"
else
    print_error "One or more builds failed."
    exit 1
fi
