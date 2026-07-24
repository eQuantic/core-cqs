#!/usr/bin/env bash
# semantic-release publish step: push the packed packages (and snupkg symbols) to NuGet.org
# and GitHub Packages. Requires NUGET_KEY (and GITHUB_TOKEN for GitHub Packages).
set -euo pipefail

dotnet nuget push "artifacts/packages/*.nupkg" \
  --api-key "${NUGET_KEY}" \
  --source https://api.nuget.org/v3/index.json \
  --skip-duplicate

if [ -n "${GITHUB_TOKEN:-}" ]; then
  dotnet nuget push "artifacts/packages/*.nupkg" \
    --api-key "${GITHUB_TOKEN}" \
    --source https://nuget.pkg.github.com/eQuantic/index.json \
    --skip-duplicate
fi
