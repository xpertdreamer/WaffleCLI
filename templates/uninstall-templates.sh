#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}🗑️ Uninstalling WaffleCLI Templates...${NC}"

dotnet new uninstall ./wafflecli-basic

echo -e "${GREEN}✅ Templates uninstalled!${NC}"