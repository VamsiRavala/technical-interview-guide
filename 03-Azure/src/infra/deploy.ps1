<#
  Deploys the Promotions Management infrastructure.
  Prereqs: Azure CLI (az), logged in (az login), Bicep CLI (az bicep install).
#>
param(
  [string]$ResourceGroup = 'rg-promotions-dev',
  [string]$Location = 'eastus'
)

$ErrorActionPreference = 'Stop'

Write-Host "Creating resource group $ResourceGroup in $Location..."
az group create --name $ResourceGroup --location $Location | Out-Null

Write-Host "Deploying main.bicep..."
az deployment group create `
  --resource-group $ResourceGroup `
  --template-file "$PSScriptRoot/main.bicep" `
  --parameters "$PSScriptRoot/main.parameters.json" `
  --parameters location=$Location

Write-Host "Done. Read outputs with:" -ForegroundColor Green
Write-Host "  az deployment group show -g $ResourceGroup -n main --query properties.outputs"
