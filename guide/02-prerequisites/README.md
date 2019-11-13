![banner](assets/banner.png)

# Creating Azure Prerequisites

Now we are ready to setup our initial Azure resources that will be needed.

>NOTE: You can use the Azure Portal to perform these actions, but I prefer to use scripts as it offer repeatable steps and clear status.

All scripts to provision the entire resources in this guide will have the prefix **azure-prerequisites-XYZ.sh** under [scripts](**../../src/scripts) folder

>NOTE: You need to have Azure CLI installed to be able to execute the below commands. If you don't have that in hand, you can use [Azure Cloud Shell](https://docs.microsoft.com/en-us/azure/cloud-shell/overview) directly from Azure Portal.
![cloud-shell](assets/cloud-shell.png)

## Azure CLI sign in

Fire up your favorite bash terminal and execute the following commands:

```bash

#***** Login to Azure Subscription *****
# A browser window will open to complete the authentication :)
az login

# You can also login using Service Principal (replace values in <>)
# az login --service-principal --username APP_ID --password PASSWORD --tenant TENANT_ID

az account set --subscription "YOUR-SUBSCRIPTION-NAME"

#Make sure the active subscription is set correctly
SUBSCRIPTION_ACCOUNT=$(az account show)
echo $SUBSCRIPTION_ACCOUNT

# Get the tenant ID
TENANT_ID=$(echo $SUBSCRIPTION_ACCOUNT | jq -r .tenantId)
# or use TENANT_ID=$(az account show --query tenantId -o tsv)
echo $TENANT_ID
echo export TENANT_ID=$TENANT_ID >> ~/.bashrc

# Get the subscription ID
SUBSCRIPTION_ID=$(echo $SUBSCRIPTION_ACCOUNT | jq -r .id)
# or use TENANT_ID=$(az account show --query tenantId -o tsv)
echo $SUBSCRIPTION_ID
echo export SUBSCRIPTION_ID=$SUBSCRIPTION_ID >> ~/.bashrc

clear

#***** END Login to Azure Subscription *****

```

## Setting up deployment variables

I use variables to easily change my deployment parameters across multiple scripts and sessions.

```bash

# Setup some variables for reusability
# Please update the values if you need to use other values but make sure these are unique
# Also make sure to use only lower case to avoid conflict with recourses that requires that.

PREFIX="ie${RANDOM}"
RG="${PREFIX}-rg"
LOCATION="westeurope"
FRAMES_STORAGE="${PREFIX}framesstg"
COSMOSDB_ACCOUNT="${PREFIX}telemetrydb"
SB_NAMESPACE="${PREFIX}-ns"
CS_ACCOUNT="${PREFIX}-cs"
CONTAINER_REGISTRY_NAME="${PREFIX}contosoacr"
VNET_NAME="${PREFIX}-network"
WORKSPACE_NAME="${PREFIX}-logs"

#If you wish to have these values persist across sessions use:
echo export PREFIX=$PREFIX >> ~/.bashrc
echo export RG=$RG >> ~/.bashrc
echo export LOCATION=$LOCATION >> ~/.bashrc
echo export FRAMES_STORAGE=$FRAMES_STORAGE >> ~/.bashrc
echo export COSMOSDB_ACCOUNT=$COSMOSDB_ACCOUNT >> ~/.bashrc
echo export SB_NAMESPACE=$SB_NAMESPACE >> ~/.bashrc
echo export CS_ACCOUNT=$CS_ACCOUNT >> ~/.bashrc
echo export CONTAINER_REGISTRY_NAME=$CONTAINER_REGISTRY_NAME >> ~/.bashrc
echo export VNET_NAME=$VNET_NAME >> ~/.bashrc
echo export WORKSPACE_NAME=$WORKSPACE_NAME >> ~/.bashrc

```

## Creating Resource Group

Resource Group is your virtual folder that we will provision all of our solution resources.

```bash

# Create a resource group
az group create --name $RG --location $LOCATION

```

## Storage Account

[Azure Storage Account](https://docs.microsoft.com/en-us/azure/storage/common/storage-account-overview) offers a cloud storage for your blobs, files, disks,... We will used it to store captured frames from the connected cameras.

```bash

az storage account create \
    -n $FRAMES_STORAGE \
    -g $RG \
    -l $LOCATION \
    --sku Standard_LRS

```

## Cosmos DB

```bash

az cosmosdb create \
    -n $COSMOSDB_ACCOUNT \
    -g $RG \
    --default-consistency-level Eventual

```

## Service Bus

Service Bus will be used to offer scalable distributed messaging platform.

```bash

az servicebus namespace create \
   --resource-group $RG \
   --name $SB_NAMESPACE \
   --location $LOCATION \
   --sku Standard

```

## Cognitive Service

Azure Cognitive Services are a set of pre-trained AI models that solve common AI requirements like vision, text and speech.

You can create a single multi-service account (support multiple cognitive services with a single key) or you can create an account for each required service

>NOTE: Be aware that Cognitive Services is not available in all regions at the time of writing this document. You can check the availability of [Azure services by region here](https://azure.microsoft.com/global-infrastructure/services/?products=cognitive-services)

[Read more here](https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account-cli?tabs=windows)

```bash

# Creating a multi-service cognitive services account
az cognitiveservices account create \
    -n $CS_ACCOUNT \
    -g $RG \
    -l $LOCATION \
    --kind CognitiveServices \
    --sku S0 \
    --yes

# Discover the available cognitive services via this command (values can be used with --kind)
az cognitiveservices account list-kinds

# Sample on how to create an account for a specific cognitive service like Face:
# az cognitiveservices account create \
    # -n $CS_FACE \
    # -g $RG \
    # -l $LOCATION \
    # --kind Face \
    # --sku S0 \
    # --yes

```

## Container Registry

As you build your containerized solution, you need a reliable and enterprise ready container registry, we will be using Azure Container Registry to accomplish that.

```bash

az acr create \
    -g $RG \
    -n $CONTAINER_REGISTRY_NAME \
    --sku Basic

```

## Virtual Network

Networking is an important part of your cloud-native platform that look after services routing, security and other important aspects of the deployment.

```bash

# As part of your Azure networking, you need to plan the address spaces and subnet.
# Planning tasks should think about providing enough CIDR range to your services and making sure there are no conflicts with other networks that will be connected to this one (like on-premise networks)

# Networking address space requires careful design exercise that you should go through.
# For simplicity I'm using /16 for the address space with /24 for each service

# Virtual Network address space
VNET_ADDRESS_SPACE="10.42.0.0/16"

# AKS primary subnet
AKSSUBNET_NAME="${PREFIX}-akssubnet"

# AKS exposed services subnet
SVCSUBNET_NAME="${PREFIX}-svcsubnet"

# Application Gateway subnet
AGW_SUBNET_NAME="${PREFIX}-appgwsubnet"

# Azure Firewall Subnet name must be AzureFirewallSubnet
FWSUBNET_NAME="AzureFirewallSubnet"

# Virutal nodes subnet (will be managed by Azure Container Instances)
VNSUBNET_NAME="${PREFIX}-vnsubnet"

# IP ranges for each subnet
AKSSUBNET_IP_PREFIX="10.42.1.0/24"
SVCSUBNET_IP_PREFIX="10.42.2.0/24"
AGW_SUBNET_IP_PREFIX="10.42.3.0/24"
FWSUBNET_IP_PREFIX="10.42.4.0/24"
VNSUBNET_IP_PREFIX="10.42.5.0/24"

# First we create our virtual network
az network vnet create \
    --resource-group $RG \
    --name $VNET_NAME \
    --address-prefixes $VNET_ADDRESS_SPACE \
    --subnet-name $AKSSUBNET_NAME \
    --subnet-prefix $AKSSUBNET_IP_PREFIX

```

## Log Analytics Workspace (AKS and Firewall Telemetry)

Azure Log Analytics Workspace is part of Azure Monitor and offers scalable storage and queries for our systems telemetry.

[Read more here](https://docs.microsoft.com/en-us/azure/azure-monitor/log-query/get-started-portal)

```bash

# We will use Azure Resource Manager json template to deploy the workspace.

# First we update the workspace template with our custom name and location (using Linux stream edit)
sed logs-workspace-deployment.json \
    -e s/WORKSPACE-NAME/$WORKSPACE_NAME/g \
    -e s/DEPLOYMENT-LOCATION/$LOCATION/g \
    > logs-workspace-deployment-updated.json

# Have a look at the updated deployment template:
cat logs-workspace-deployment-updated.json

# Deployment can take a few mins
WORKSPACE=$(az group deployment create \
    --resource-group $RG \
    --name $PREFIX-logs-workspace-deployment \
    --template-file logs-workspace-deployment-updated.json)

echo $WORKSPACE

```

## Application Insights

Getting application performance telemetry is essential to keep track of how your code is performing (in dev or prod).

App Insights offers app-specific telemetry that are tightly integrated with your code through several SDKs (one for Java, .NET,...)

>NOTE: You need to execute the below script of each app that you want to integrate with App Insights. The script capture the instrumentation key that you need to update the relevant app configuration with.

```bash

# In addition to Azure Monitor for containers, you can deploy app insights to your application code
# App Insights support many platforms like .NET, Java, and NodeJS.
# Docs: https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview
# Check Kubernetes apps with no instrumentation and service mesh: https://docs.microsoft.com/en-us/azure/azure-monitor/app/kubernetes
# Create App Insights to be used within your apps:

APP_NAME="${PREFIX}-myapp-insights"
APPINSIGHTS_KEY=$(az resource create \
    --resource-group ${RG} \
    --resource-type "Microsoft.Insights/components" \
    --name ${APP_NAME} \
    --location ${LOCATION} \
    --properties '{"Application_Type":"web"}' \
    | grep -Po "\"InstrumentationKey\": \K\".*\"")
echo $APPINSIGHTS_KEY

```

## Next step

Congratulations on completing this section. Let's move to the next step:

[Next Step]()