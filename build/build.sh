# !bin/bash

echo "Building WaffleCLI"

dotnet clean

dotnet restore

dotnet build --configuration Release --no-restore

dotnet test --configuration Release --no-build --verbosity normal

dotnet pack src/WaffleCLI/WaffleCLI.csproj --configuration Release --no-build --output nupkgs

echo "Build completed successfully!"