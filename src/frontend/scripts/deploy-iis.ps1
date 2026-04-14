# Build the SPA (API URL baked in) and copy dist to an IIS folder.
# Run from src/frontend:  .\scripts\deploy-iis.ps1 -DeployPath 'C:\inetpub\wwwroot\CleanApi'
#
# Public path is hardcoded in vite.config.ts (IIS /CleanApi/).
#
# Default web.config avoids <rewrite> so IIS works without the URL Rewrite module (fixes 500.19 0x8007000d).
# Use -UrlRewrite if you installed https://www.iis.net/downloads/microsoft/url-rewrite (needed for deep links with client-side routing).

param(
    [Parameter(Mandatory = $true)]
    [string] $DeployPath,

    [string] $ApiBase = "https://localhost:7288",

    [switch] $UrlRewrite
)

$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$dist = Join-Path $root "dist"
$base = $ApiBase.Trim().TrimEnd("/")

Push-Location $root
try {
    if ($base) { $env:VITE_API_BASE = $base }
    npm run build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
finally {
    Remove-Item Env:VITE_API_BASE -ErrorAction SilentlyContinue
    Pop-Location
}

if (-not (Test-Path $dist)) { throw "No dist folder after build." }

$d = $DeployPath.TrimEnd("\")
New-Item -ItemType Directory -Path $d -Force | Out-Null
robocopy $dist $d /MIR /NFL /NDL /NJH /NJS /NC /NS | Out-Null
if ($LASTEXITCODE -ge 8) { throw "robocopy failed: $LASTEXITCODE" }

$webConfigPath = Join-Path $d "web.config"
$utf8NoBom = New-Object System.Text.UTF8Encoding $false

if ($UrlRewrite) {
    $xml = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <rewrite>
      <rules>
        <rule name="SpaFallback" stopProcessing="true">
          <match url=".*" />
          <conditions logicalGrouping="MatchAll">
            <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
            <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
          </conditions>
          <action type="Rewrite" url="index.html" />
        </rule>
      </rules>
    </rewrite>
  </system.webServer>
</configuration>
'@
} else {
    $xml = @'
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <system.webServer>
    <defaultDocument>
      <files>
        <clear />
        <add value="index.html" />
      </files>
    </defaultDocument>
  </system.webServer>
</configuration>
'@
}

[System.IO.File]::WriteAllText($webConfigPath, $xml.TrimStart(), $utf8NoBom)

Write-Host "Deployed to $d (API: $base; rewrite: $($UrlRewrite.IsPresent))"
