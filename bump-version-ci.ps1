# scripts/bump-version-ci.ps1
# CI-only: bumps <Version> in TravelDocFaker/TravelDocFaker.csproj
# based on the latest commit message type (Conventional Commits) and pushes tag vX.Y.Z.
# No prompts. Exits non-zero on error.

$ErrorActionPreference = 'Stop'
function Fail($m) { Write-Error $m; exit 1 }

# --- Resolve repo root and fixed csproj path ---
$repoRoot = (git rev-parse --show-toplevel) 2>$null
if (-not $repoRoot) { $repoRoot = (Get-Location).Path }

$csprojPath = Join-Path $repoRoot 'TravelDocFaker\TravelDocFaker.csproj'
if (-not (Test-Path -LiteralPath $csprojPath)) {
    Fail "Project file not found at: $csprojPath"
}

# --- Load current version ---
[xml]$xml = Get-Content -LiteralPath $csprojPath
$versionNode = $xml.SelectSingleNode('//Project/PropertyGroup/Version')
if (-not $versionNode) {
    $pg = $xml.SelectSingleNode('//Project/PropertyGroup')
    if (-not $pg) { Fail "No <PropertyGroup> found in the .csproj." }
    $versionNode = $xml.CreateElement('Version')
    $versionNode.InnerText = '0.1.0'
    [void]$pg.AppendChild($versionNode)
}

$oldVersion = ($versionNode.InnerText).Trim()
if ($oldVersion -notmatch '^\d+\.\d+\.\d+$') {
    Fail "Invalid <Version> '$oldVersion' (expected MAJOR.MINOR.PATCH)."
}

# --- Latest commit message (subject + body) ---
$commitMsg = (git log -1 --pretty=%B).Trim()
if (-not $commitMsg) { Fail "Could not read latest commit message." }

# --- Decide increment from Conventional Commits ---
$firstLine = ($commitMsg -split "`r?`n")[0]
$body = if ($commitMsg.Length -gt $firstLine.Length) { $commitMsg.Substring($firstLine.Length) } else { "" }

$matches = [regex]::Match($firstLine, '^(?<type>[a-zA-Z]+)(?:\([^\)]*\))?(?<bang>!)?:')
$type = ($matches.Groups['type'].Value.ToLowerInvariant())
$hasBang = ($matches.Success -and $matches.Groups['bang'].Success)
$hasBreakingFooter = ($body -match '(?im)^\s*BREAKING CHANGE\b')

$increment = 'patch'
if ($type -eq 'breaking' -or $hasBang -or $hasBreakingFooter) {
    $increment = 'major'
} elseif ($type -eq 'feat') {
    $increment = 'minor'
}

# --- Compute new version ---
$parts = $oldVersion.Split('.')
[int]$major = $parts[0]
[int]$minor = $parts[1]
[int]$patch = $parts[2]

switch ($increment) {
    'major' { $major++; $minor = 0; $patch = 0 }
    'minor' { $minor++; $patch = 0 }
    'patch' { $patch++ }
}

$newVersion = "$major.$minor.$patch"

# --- Write back ---
$versionNode.InnerText = $newVersion
$xml.Save($csprojPath)

Write-Host "Version bumped: $oldVersion -> $newVersion (increment: $increment)"

# --- Git identity (for CI) ---
if (-not (git config user.email 2>$null)) { git config user.email "actions@github.com" | Out-Null }
if (-not (git config user.name  2>$null)) { git config user.name  "github-actions[bot]" | Out-Null }

# --- Commit + tag + push ---
# Add all changes (new, modified, deleted)
git add -A

git commit -m "chore: bump version to $newVersion"
if ($LASTEXITCODE -ne 0) { Fail "git commit failed." }

git tag "v$newVersion"
if ($LASTEXITCODE -ne 0) { Fail "git tag failed." }

git push
if ($LASTEXITCODE -ne 0) { Fail "git push failed." }

git push origin "v$newVersion"
if ($LASTEXITCODE -ne 0) { Fail "git push tag failed." }

Write-Host "Pushed commit and tag v$newVersion."
