
# Have a look at the available versions first :)
az aks get-versions -l ${LOCATION} -o table

# To get the latest "production" supported version use the following (even if preview flag is activated):
AKS_VERSION=$(az aks get-versions -l ${LOCATION} --query "orchestrators[?isPreview==null].{Version:orchestratorVersion} | [-1]" -o tsv)
echo $AKS_VERSION

# Get latest AKS versions (including PREVIEW)
# Note that this command will get the latest preview version if preview flag is activated)
# AKS_VERSION=$(az aks get-versions -l ${LOCATION} --query 'orchestrators[-1].orchestratorVersion' -o tsv)
# echo $AKS_VERSION

# Save the selected version
echo export AKS_VERSION=$AKS_VERSION >> ./crowdanalytics

# Name our cluster
CLUSTER_NAME=$PREFIX-aks

# Giving a friendly name to our default node pool
AKS_DEFAULT_NODEPOOL=npdefault

# AKS Service Principal
# Docs: https://github.com/MicrosoftDocs/azure-docs/blob/master/articles/aks/kubernetes-service-principal.md
# AKS provision Azure resources based on the cluster needs, 
# like automatic provision of storage or creating public load balancer
# Also AKS needs to communicate with AzureRM APIs through that SP
# You can use the automatically generated SP if you omitted the SP configuration in AKS creation process

# To update existing AKS cluster SP, use the following command (when needed):
# az aks update-credentials \
#     --resource-group $RG \
#     --name $CLUSTER_NAME \
#     --reset-service-principal \
#     --service-principal $AKS_SP_ID \
#     --client-secret $AKS_SP_PASSWORD

az aks create \
    --resource-group $RG \
    --name $CLUSTER_NAME \
    --location $LOCATION \
    --kubernetes-version $AKS_VERSION \
    --generate-ssh-keys \
    --enable-addons monitoring \
    --load-balancer-sku standard \
    --network-plugin azure \
    --network-policy azure \
    --vnet-subnet-id $AKS_SUBNET_ID \
    --nodepool-name $AKS_DEFAULT_NODEPOOL \
    --node-count 3 \
    --max-pods 30 \
    --node-vm-size "Standard_D4s_v3" \
    --vm-set-type VirtualMachineScaleSets \
    --workspace-resource-id $SHARED_WORKSPACE_ID \
    --attach-acr $ACR_ID \
    --service-principal $AKS_SP_ID \
    --client-secret $AKS_SP_PASSWORD

# Connecting to AKS via kubectl
az aks get-credentials --resource-group $RG --name $CLUSTER_NAME

# Test the connection
kubectl get nodes

# Crowd Analytics main services will be deployed to ```crowd-analytics``` namespace. So let's created now:
kubectl create namespace crowd-analytics

# Installing KEDA
# Adding KEDA repo
helm repo add kedacore https://kedacore.github.io/charts
helm repo update
# If you receive permission-denied error running the above commands, you can try them with sudo before each command

# Installing KEDA in keda namespace
kubectl create namespace keda
helm install keda kedacore/keda --namespace keda

# Validating the KEDA instllation
kubectl get po -n keda
# NAME                                               READY   STATUS    RESTARTS   AGE
# keda-operator-dbfbd6bdb-qnn6g                      1/1     Running   0          35s
# keda-operator-metrics-apiserver-8678f8c5d9-nhw2l   1/1     Running   0          35s
