param([string]$FilePath)
$content = Get-Content $FilePath -Raw
$clipDef = '<defs><clipPath id="round"><circle cx="480" cy="480" r="480"/></clipPath></defs><g clip-path="url(#round)">'
$content = $content -replace '(xml:space="preserve">)', "`$1`n$clipDef"
$content = $content -replace '</svg>', '</g></svg>'
[System.IO.File]::WriteAllText($FilePath, $content)
Write-Host "Done: $FilePath"
