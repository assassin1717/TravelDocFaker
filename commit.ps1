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
# Build commit message
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
$confirm = Read-Host "Proceed with commit? (y/n)"
if ($confirm.ToLower() -ne "y") {
    Write-Host "Commit canceled."
    exit 0
}

# Stage all changes (new, modified, deleted)
git add -A
if ($LASTEXITCODE -ne 0) {
    Write-Error "git add failed."
    exit 1
}

# Pass messages as array to avoid quoting issues and colon interpolation quirks
$gitArgs = @('-m', $commitHeader)
if ($LongDesc -and $LongDesc.Trim() -ne "") {
    $gitArgs += @('-m', $LongDesc)
}

git commit @gitArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "git commit failed."
    exit 1
}

# Optional push
$pushConfirm = Read-Host "Push to current branch now? (y/n)"
if ($pushConfirm.ToLower() -eq "y") {
    git push
    if ($LASTEXITCODE -ne 0) {
        Write-Error "git push failed."
        exit 1
    }
    Write-Host "Commit pushed successfully."
} else {
    Write-Host "Commit created locally (not pushed)."
}
