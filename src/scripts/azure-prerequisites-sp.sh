#***** Prepare Service Principal for AKS *****

# AKS Service Principal
# Docs: https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/aks/kubernetes-service-principal.md
# AKS provision Azure resources based on the cluster needs, 
# like automatic provision of storage or creating public load balancer
# Also AKS needs to communicate with AzureRM APIs through that SP
# You can use the automatically generated SP if you omitted the SP configuration in AKS creation process

# Create a SP to be used by AKS
AKS_SP_NAME="${PREFIX}-aks-sp"
AKS_SP=$(az ad sp create-for-rbac -n $AKS_SP_NAME --skip-assignment)
# As the json result stored in AKS_SP, we use some jq Kung Fu to extract the values
# jq documentation: (https://shapeshed.com/jq-json/#how-to-pretty-print-json)
echo $AKS_SP | jq
AKS_SP_ID=$(echo $AKS_SP | jq -r .appId)
AKS_SP_PASSWORD=$(echo $AKS_SP | jq -r .password)
echo $AKS_SP_ID
echo $AKS_SP_PASSWORD

# OR you can retrieve back existing SP any time:
# AKS_SP=$(az ad sp show --id http://$AKS_SP_NAME)
# AKS_SP_ID=$(echo $AKS_SP | jq -r .appId)
# AKS_SP_PASSWORD="REPLACE_SP_PASSWORD"

# Don't have the password, get new password for SP (careful not to void in-use SP account)
# AKS_SP=$(az ad sp credential reset --name $AKS_SP_ID)
# AKS_SP_ID=$(echo $AKS_SP | jq -r .appId)
# AKS_SP_PASSWORD=$(echo $AKS_SP | jq -r .password)
# echo $AKS_SP_ID
# echo $AKS_SP_PASSWORD

# Save the new variables
echo export AKS_SP_NAME=$AKS_SP_NAME >> ~/.bashrc
echo export AKS_SP_ID=$AKS_SP_ID >> ~/.bashrc
echo export AKS_SP_PASSWORD=$AKS_SP_PASSWORD >> ~/.bashrc

# Get also the AAD object id for the SP for later use
AKS_SP_OBJ_ID=$(az ad sp show --id ${AKS_SP_ID} --query objectId -o tsv)
echo $AKS_SP_OBJ_ID
echo export AKS_SP_OBJ_ID=$AKS_SP_OBJ_ID >> ~/.bashrc

# As we used --skip-assignment, we will be assigning the SP to various services later
# These assignment like ACR, vNET and other resources that will require AKS to access
# az role assignment create --assignee $AKS_SP_ID --scope <resourceScope> --role Contributor

# To update existing AKS cluster SP, use the following command (when needed):
# az aks update-credentials \
#     --resource-group $RG \
#     --name $CLUSTER_NAME \
#     --reset-service-principal \
#     --service-principal $AKS_SP_ID \
#     --client-secret $AKS_SP_PASSWORD

#***** END Prepare Service Principal for AKS *****

### SP Permission Assignment

# The SP account created above don't have any permissions (--skip-assignment). Now we need to give it permissions as we go to make sure that lowest permissions level is always maintained.

# Current resources that SP requires access to:

# - Virtual Network
# - Container Registry

# Assign AKS SP permissions to the vnet
# Granular permission also can be granted through the Network Contributor role
# Docs: https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/role-based-access-control/built-in-roles.md#network-contributor
az role assignment create --assignee $AKS_SP_ID --scope $AKS_SUBNET_ID --role "Network Contributor"
az role assignment create --assignee $AKS_SP_ID --scope $AKS_SVCSUBNET_ID --role "Network Contributor"
az role assignment create --assignee $AKS_SP_ID --scope $AKS_VNSUBNET_ID --role "Network Contributor"

# Get the Resource ID for the ACR
ACR_ID=$(az acr show --name $CONTAINER_REGISTRY_NAME --resource-group $RG --query id --output tsv)
echo $ACR_ID

# Create the role assignment to allow AKS authenticating against the ACR
az role assignment create --assignee $AKS_SP_ID --role AcrPull --scope $ACR_ID

### Review Permissions Assigned

# Review the current SP assignments
az role assignment list \
    --all \
    --assignee $AKS_SP_ID \
    --output json \
    | jq '.[] | {"principalName":.principalName, "roleDefinitionName":.roleDefinitionName, "scope":.scope}'
