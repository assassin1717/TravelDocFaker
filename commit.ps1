param(
    [string]$Type,
    [string]$Scope,
    [string]$ShortDesc,
    [string]$LongDesc
)

# -------------------------
# Tipos de commit permitidos
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
# Entrada interativa (se faltar algo)
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
# Calcular incremento da versão
# -------------------------
$increment = switch ($Type) {
    "feat"     { "minor" }
    "breaking" { "major" }
    default    { "fix"; break } # "fix" e todos os outros caem em patch
}
if ($increment -eq "fix") { $increment = "patch" }

# -------------------------
# Localizar e ler o .csproj
# -------------------------
$csprojPath = Join-Path $PSScriptRoot 'TravelDocFaker\TravelDocFaker.csproj'
if (-not (Test-Path -LiteralPath $csprojPath)) {
    Write-Error "Nenhum .csproj encontrado em $csprojPath"
    exit 1
}

[xml]$xml = Get-Content -LiteralPath $csprojPath

# Encontrar (ou criar) o nó <Version>
$versionNode = $xml.SelectSingleNode('//Project/PropertyGroup/Version')
if (-not $versionNode) {
    $pg = $xml.SelectSingleNode('//Project/PropertyGroup')
    if (-not $pg) {
        Write-Error "Não foi encontrado <PropertyGroup> no .csproj."
        exit 1
    }
    $versionNode = $xml.CreateElement('Version')
    $versionNode.InnerText = '0.1.0'
    $null = $pg.AppendChild($versionNode)
}

$oldVersion = ($versionNode.InnerText).Trim()

if ($oldVersion -notmatch '^\d+\.\d+\.\d+$') {
    Write-Error "Valor de <Version> inválido: '$oldVersion' (esperado: MAJOR.MINOR.PATCH)"
    exit 1
}

# -------------------------
# Incrementar versão
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

# Gravar de volta
$xml.Save($csprojPath)

Write-Host "Version updated: $oldVersion → $newVersion"

# -------------------------
# Mensagem de commit
# -------------------------
$scopeFormatted = if ($Scope -and $Scope.Trim() -ne "") { "($Scope)" } else { "" }

$commitHeader = "${Type}${scopeFormatted}: $ShortDesc"
$commitPreview = if ($LongDesc -and $LongDesc.Trim() -ne "") {
    "$commitHeader`n`n$LongDesc"
} else {
    $commitHeader
}

Write-Host "`nCommit gerado:"
Write-Host "------------------------------------"
Write-Host $commitPreview
Write-Host "------------------------------------"

# -------------------------
# Confirmar e executar git
# -------------------------
$confirm = Read-Host "Confirm commit and tag version $newVersion? (s/n)"
if ($confirm.ToLower() -eq "s") {
    git add .

    # Passar mensagens como array de argumentos evita problemas de quoting
    $gitArgs = @('-m', $commitHeader)
    if ($LongDesc -and $LongDesc.Trim() -ne "") {
        $gitArgs += @('-m', $LongDesc)
    }

    git commit @gitArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha no git commit."
        exit 1
    }

    # Atualiza o branch e a tag
    git push
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha no git push."
        exit 1
    }

    git tag "v$newVersion"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha ao criar a tag."
        exit 1
    }

    git push origin "v$newVersion"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Falha ao enviar a tag."
        exit 1
    }

    Write-Host "✅ Commit e tag 'v$newVersion' enviados!"
} else {
    Write-Host "❌ Commit cancelado."
}
