# pushes the api secrets from a json file into Azure Key Vault, run after `az login`
# usage: .\push-secrets.ps1 -VaultName atms-kv -SecretsFile C:\somewhere\secrets.prod.json
# the file has the shape of the local user-secrets file but with PRODUCTION values, keep it out of the repo
param(
    [Parameter(Mandatory)] [string] $VaultName,
    [Parameter(Mandatory)] [string] $SecretsFile
)

$ErrorActionPreference = 'Stop'
$secrets = Get-Content $SecretsFile -Raw | ConvertFrom-Json
$tmp = New-TemporaryFile

try {
    foreach ($section in $secrets.PSObject.Properties) {
        foreach ($field in $section.Value.PSObject.Properties) {
            # key vault names cant have ':', the .net provider turns '--' back into ':'
            $name = "$($section.Name)--$($field.Name)"

            # through a file, passwords with & or ! break on the windows command line
            Set-Content -Path $tmp -Value $field.Value -NoNewline -Encoding utf8NoBOM
            az keyvault secret set --vault-name $VaultName --name $name --file $tmp --encoding utf-8 --output none
            if ($LASTEXITCODE -ne 0) { throw "failed on $name" }

            Write-Host "set $name"
        }
    }
}
finally {
    Remove-Item $tmp
}
