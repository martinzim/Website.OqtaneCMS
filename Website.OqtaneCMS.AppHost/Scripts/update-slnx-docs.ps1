#!/usr/bin/env pwsh
# Aktualizuje docs folder v Website.OqtaneCMS.slnx podla aktualneho obsahu adresara docs/

$slnxPath = Join-Path $PSScriptRoot "Website.OqtaneCMS.slnx"
$docsPath = Join-Path $PSScriptRoot "docs"

$docFiles = Get-ChildItem -Path $docsPath -File -Recurse |
    Sort-Object FullName |
    ForEach-Object {
        $rel = $_.FullName.Substring($PSScriptRoot.Length).TrimStart('\', '/').Replace('\', '/')
        "    <File Path=`"$rel`" />"
    }

$docsFolder = (@("  <Folder Name=`"/docs/`">") + $docFiles + @("  </Folder>")) -join "`r`n"

$xml = [xml](Get-Content $slnxPath -Raw)
$content = Get-Content $slnxPath -Raw

# Nahrad existujuci /docs/ blok, alebo vloz novy za prvy Folder
if ($content -match '(?s)  <Folder Name="/docs/">.*?</Folder>') {
    $content = $content -replace '(?s)  <Folder Name="/docs/">.*?</Folder>', $docsFolder
} else {
    $content = $content -replace '(  <Folder [^/]*/>)', "`$1`r`n$docsFolder"
}

[System.IO.File]::WriteAllText($slnxPath, $content, [System.Text.Encoding]::UTF8)
Write-Host "Website.OqtaneCMS.slnx aktualizovany: $($docFiles.Count) suborov v /docs/" -ForegroundColor Green
