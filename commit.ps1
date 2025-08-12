param(
    [string]$Type,
    [string]$Scope,
    [string]$ShortDesc,
    [string]$LongDesc
)

# -------------------------
# Allowed commit types
# -------------------------
$types = @(
    "feat",
    "fix",
    "docs",
    "style",
    "refactor",
    "perf",
    "test",
    "chore",
    "build",
    "ci",
    "breaking"
)

# -------------------------
# Interactive input if missing
# -------------------------
if (-not $Type) {
    Write-Host "Select commit type:"
    for ($i = 0; $i -lt $types.Count; $i++) {
        Write-Host "$($i+1)) $($types[$i])"
    }
    do {
        $choice = Read-Host "Type number (1-$($types.Count))"
        [int]$choiceNum = 0
        $valid = [int]::TryParse($choice, [ref]$choiceNum) -and $choiceNum -ge 1 -and $choiceNum -le $types.Count
        if ($valid) { $Type = $types[$choiceNum - 1] }
    } until ($valid)
}

$Type = $Type.ToLowerInvariant()

if (-not $Scope) {
    $Scope = Read-Host "Feature name or scope (can be empty)"
}

if (-not $ShortDesc) {
    do {
        $ShortDesc = Read-Host "Short description"
        $ShortDesc = $ShortDesc.Trim()
    } until ($ShortDesc -ne "")
}

if (-not $LongDesc) {
    $LongDesc = Read-Host "Long description (can be empty)"
}

# -------------------------
# Determine version increment
# -------------------------
$increment = switch ($Type) {
    "feat"     { "minor" }
    "breaking" { "major" }
    default    { "fix"; break }
}
if ($increment -eq "fix") { $increment = "patch" }

# -------------------------
# Locate and read .csproj
# -------------------------
$csprojPath = Join-Path $PSScriptRoot 'TravelDocFaker\TravelDocFaker.csproj'
if (-not (Test-Path -LiteralPath $csprojPath)) {
    Write-Error "No .csproj found at $csprojPath"
    exit 1
}

[xml]$xml = Get-Content -LiteralPath $csprojPath

# Find or create <Version> node
$versionNode = $xml.SelectSingleNode('//Project/PropertyGroup/Version')
if (-not $versionNode) {
    $pg = $xml.SelectSingleNode('//Project/PropertyGroup')
    if (-not $pg) {
        Write-Error "No <PropertyGroup> found in the .csproj."
        exit 1
    }
    $versionNode = $xml.CreateElement('Version')
    $versionNode.InnerText = '0.1.0'
    $null = $pg.AppendChild($versionNode)
}

$oldVersion = ($versionNode.InnerText).Trim()

if ($oldVersion -notmatch '^\d+\.\d+\.\d+$') {
    Write-Error "Invalid <Version> value: '$oldVersion' (expected: MAJOR.MINOR.PATCH)"
    exit 1
}

# -------------------------
# Increment version
# -------------------------
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
$versionNode.InnerText = $newVersion

# Save updated .csproj
$xml.Save($csprojPath)

Write-Host "Version updated: $oldVersion → $newVersion"

# -------------------------
# Commit message
# -------------------------
$scopeFormatted = if ($Scope -and $Scope.Trim() -ne "") { "($Scope)" } else { "" }

$commitHeader = "${Type}${scopeFormatted}: $ShortDesc"
$commitPreview = if ($LongDesc -and $LongDesc.Trim() -ne "") {
    "$commitHeader`n`n$LongDesc"
} else {
    $commitHeader
}

Write-Host "`nGenerated commit message:"
Write-Host "------------------------------------"
Write-Host $commitPreview
Write-Host "------------------------------------"

# -------------------------
# Confirm and execute git
# -------------------------
$confirm = Read-Host "Confirm commit and tag version $newVersion? (y/n)"
if ($confirm.ToLower() -eq "y") {
    # Add all changes (new, modified, deleted)
    git add -A

    # Pass messages as array to avoid quoting issues
    $gitArgs = @('-m', $commitHeader)
    if ($LongDesc -and $LongDesc.Trim() -ne "") {
        $gitArgs += @('-m', $LongDesc)
    }

    git commit @gitArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Git commit failed."
        exit 1
    }

    git push
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Git push failed."
        exit 1
    }

    git tag "v$newVersion"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create tag."
        exit 1
    }

    git push origin "v$newVersion"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to push tag."
        exit 1
    }

    Write-Host "Commit and tag 'v$newVersion' pushed successfully."
} else {
    Write-Host "Commit canceled."
}
