
# This script helps in making sure all in-scope Azure resource providers are available.

# If you want to list all resource providers with their status, you can use this:
# az provider list --query "[].{Provider:namespace, Status:registrationState}" --out table

# Registring required resource providers for this workshop
az provider register --namespace Microsoft.DocumentDB 
az provider register --namespace Microsoft.ApiManagement
az provider register --namespace Microsoft.CognitiveServices
az provider register --namespace Microsoft.KeyVault
az provider register --namespace Microsoft.ManagedIdentity
az provider register --namespace Microsoft.Storage
az provider register --namespace microsoft.insights
az provider register --namespace Microsoft.Compute
az provider register --namespace Microsoft.Network
az provider register --namespace Microsoft.ContainerRegistry
az provider register --namespace Microsoft.ContainerService
az provider register --namespace Microsoft.OperationalInsights
az provider register --namespace Microsoft.ServiceBus
az provider register --namespace Microsoft.SignalRService
az provider register --namespace Microsoft.EventHub
az provider register --namespace Microsoft.OperationsManagement


# If you want to list all resource providers with their status, you can use this:
# az provider list --query "[].{Provider:namespace, Status:registrationState}" --out table

# To check a particular resource provider status, you can use this (replace Microsoft.ContainerRegistry with other namespace to check):
az provider list --query "[?contains(namespace, 'Microsoft.DocumentDB')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.ApiManagement')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.CognitiveServices')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.KeyVault')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.ManagedIdentity')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.Storage')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'microsoft.insights')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.Compute')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.Network')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.ContainerRegistry')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.ContainerService')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.OperationalInsights')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.ServiceBus')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.SignalRService')].{Provider:namespace, Status:registrationState}" --out table
az provider list --query "[?contains(namespace, 'Microsoft.EventHub')].{Provider:namespace, Status:registrationState}" --out table

# Expected outcome for one provider:
# Provider                     Status
# ---------------------------  ----------
# Microsoft.ContainerRegistry  Registered
