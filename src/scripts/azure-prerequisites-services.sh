# You can append the --no-wait to Azure CLI commands so you will not wait for the resource to finish
# provisioning. This is helpful when you are creating not directly dependent resources

# Create a resource group
az group create --name $RG --location $LOCATION

# Creating Azure Storage account to store camera frames for post processing
az storage account create \
    -n $FRAMES_STORAGE \
    -g $RG \
    -l $LOCATION \
    --sku Standard_LRS

# Creating Cosmos DB account to store all system data
# be patient as this could take a few mins :)
az cosmosdb create \
    -n $COSMOSDB_ACCOUNT \
    -g $RG \
    --default-consistency-level Eventual

# For distributed async integration, we will be using Azure Service Bus
az servicebus namespace create \
   --resource-group $RG \
   --name $SB_NAMESPACE \
   --location $LOCATION \
   --sku Standard

# Creating multi-service cognitive account to access all the great pre-built AI models
az cognitiveservices account create \
    -n $CS_ACCOUNT \
    -g $RG \
    -l $LOCATION \
    --kind CognitiveServices \
    --sku S0 \
    --yes

# We will use Azure Container Registry to store all of our system container images
az acr create \
    -g $RG \
    -n $CONTAINER_REGISTRY_NAME \
    --sku Basic

# If you wish to enable local dev push to ACR, you need to authenticate
# Note that Docker daemon must be running before executing this command
az acr login --name $CONTAINER_REGISTRY_NAME

####
# Networking portioning is covered in a separate script file named [azure-prerequisites-network.sh]
####

# Creating Log Analytics workspace
# We will use Azure Resource Manager json template to deploy the workspace.
# Make sure that the active directory is set to scripts (where the .json file is located)

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

# Check the created workspace details :)
echo $WORKSPACE | jq

# Creating App Insights for each app
# In addition to Azure Monitor for containers, you can deploy app insights to your application code
# App Insights support many platforms like .NET, Java, and NodeJS.
# Docs: https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview
# Check Kubernetes apps with no instrumentation and service mesh: https://docs.microsoft.com/en-us/azure/azure-monitor/app/kubernetes
# Create App Insights to be used within your apps:

APP_NAME="${PREFIX}-cognitive-orch-insights"
PARAMS='{"Application_Type":"web"}'
APPINSIGHTS_KEY=$(az resource create \
    --resource-group $RG \
    --resource-type "Microsoft.Insights/components" \
    --name $APP_NAME \
    --location $LOCATION \
    --properties "${PARAMS}" \
    | grep -Po "\"InstrumentationKey\": \K\".*\"")
echo $APPINSIGHTS_KEY
