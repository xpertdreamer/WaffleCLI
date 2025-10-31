#!/bin/bash

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${GREEN}🚀 Installing WaffleCLI Templates...${NC}"

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo -e "${RED}❌ .NET SDK is not installed. Please install .NET 8.0 or later.${NC}"
    exit 1
fi

echo -e "${YELLOW}📦 Installing basic template...${NC}"
dotnet new install ./wafflecli-basic

# Verify installation
echo -e "\n${GREEN}✅ Verification:${NC}"
dotnet new list WaffleCLI

echo -e "\n${GREEN}🎉 Templates installed successfully!${NC}"
echo -e "\n${CYAN}📝 Usage examples:${NC}"
echo -e "   ${CYAN}dotnet new wafflecli-basic -n MyApp${NC}"