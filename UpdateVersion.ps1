param (
    [Parameter(Mandatory=$true)]
    [string]$ManifestPath
)

try {
    if (-not (Test-Path $ManifestPath)) {
        Write-Error "File not found: $ManifestPath"
        exit 1
    }

    # Generate the 5-digit version (YearLastDigit + Month + Day)
    $dateVersion = Get-Date -Format "yMMdd"
    $dateVersion = $dateVersion.Substring($dateVersion.Length - 5)

    # Load XML
    [xml]$manifest = Get-Content $ManifestPath

    # Use LocalName to find Identity (ignores Namespace prefix issues)
    $identity = $manifest.Package.Identity

    if ($null -eq $identity) {
        Write-Error "Could not find <Identity> node in manifest."
        exit 1
    }

    # Update the 3rd part of the version
    $versionParts = $identity.Version.Split('.')
    $versionParts[2] = $dateVersion
    $newVersion = [string]::Join(".", $versionParts)

    $identity.Version = $newVersion

    $manifest.Save($ManifestPath)
    
    Write-Host "Updated $ManifestPath to $newVersion"
    exit 0
}
catch {
    Write-Error $_.Exception.Message
    exit 1
}