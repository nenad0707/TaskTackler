# This script sets up Azure resources

# Connect to Azure account
Connect-AzAccount

# Set the subscription context
$context = Get-AzSubscription -SubscriptionName "Azure subscription 1"
Set-AzContext -Subscription $context.Id

# Set the default resource group
$resourceGroupName = "tasktackler-app"
Set-AzDefault -ResourceGroupName $resourceGroupName

# Set the GitHub organization and repository names
$githubOrganizationName = 'nenad0707'
$githubRepositoryName = 'TaskTackler'

# Create a new Azure AD application
$applicationRegistration = New-AzADApplication -DisplayName 'tasktacklerapp'

# Create a new Azure AD application federated credential
New-AzADAppFederatedCredential `
  -Name 'tasktacklerapp-branch' `
  -ApplicationObjectId $applicationRegistration.Id `
  -Issuer 'https://token.actions.githubusercontent.com' `
  -Audience 'api://AzureADTokenExchange' `
  -Subject "repo:$($githubOrganizationName)/$($githubRepositoryName):ref:refs/heads/main"

# Get the resource group
$resourceGroup = Get-AzResourceGroup -Name $resourceGroupName

# Create a new Azure AD service principal
New-AzADServicePrincipal -AppId $applicationRegistration.AppId

# Assign the Contributor role to the application
New-AzRoleAssignment `
  -ApplicationId $applicationRegistration.AppId `
  -RoleDefinitionName Contributor `
  -Scope $resourceGroup.ResourceId

# Get the Azure context
$azureContext = Get-AzContext

# Write the Azure secrets to the console
Write-Host "AZURE_CLIENT_ID: $($applicationRegistration.AppId)"
Write-Host "AZURE_TENANT_ID: $($azureContext.Tenant.Id)"
Write-Host "AZURE_SUBSCRIPTION_ID: $($azureContext.Subscription.Id)"
