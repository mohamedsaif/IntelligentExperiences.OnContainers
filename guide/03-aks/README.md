![banner](assets/banner.png)

# AKS Cluster Provisioning

Now it is time to provision the star of the show, Azure Kubernetes Service cluster.

>NOTE: I have built an end-to-end [AKS cluster provisioning with advanced configuration](https://aka.ms/aks-adv-provision) that you can check out for more details.

## Essential Cluster Provisioning

>**SCRIPT:** All scripts to provision the entire resources in this guide are in a single script named **03-aks.sh** under [scripts](../../src/scripts) folder. Please note that you need to execute the scripts after copying it to your terminal and move the active folder to src/scripts

### Variables

First we setup some AKS specific variables including the AKS version

```bash

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

```

### Service Principal Account

AKS needs a Service Principal account to authenticate against Azure ARM APIs so it can manage its resources (like worker VMs for example)

```bash

#***** Prepare Service Principal for AKS *****

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

#***** END Prepare Service Principal for AKS *****

```

### AKS Provisioning

```bash

# Be patient as the CLI provision the cluster :) maybe it is time to refresh your cup of coffee
# or append --no-wait then check the cluster provisioning status via:
# az aks list -o table

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

```

### Getting AKS Credentials

Now let's download kubectl context for our AKS cluster and test that it works

```bash

# Connecting to AKS via kubectl
az aks get-credentials --resource-group $RG --name $CLUSTER_NAME

# Test the connection
kubectl get nodes

```

### Crowd Analytics Namespace

Crowd Analytics main services will be deployed to ```crowd-analytics``` namespace. So let's created now:

```bash

kubectl create namespace crowd-analytics

```

### Helm 3

Helm is the kubernetes native package manager that is widely used by the community.

KEDA is one of the framework that support installing it through Helm 3 (or even Helm 2).

To install Helm, you can run the following:

```bash

# Helm 3 Installation Docs (https://helm.sh/docs/intro/install/)
wget https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3
chmod -R +x .
./get-helm-3

# OR
# curl -sL https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3 | sudo bash

helm version
# You should get something like:
# version.BuildInfo{Version:"v3.1.2", GitCommit:"d878d4d45863e42fd5cff6743294a11d28a9abce", GitTreeState:"clean", GoVersion:"go1.13.8"}

```

### KEDA

KEDA offer event driven auto scaler based on how large is the messages in the queue that needs to be handled.

Installing KEDA on AKS is super simple. We will use ```keda``` namespace for our installation:

```bash

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

```

## Next step

Congratulations on completing this section. Let's move to the next step:

[Next Step](../04-iot/README.md)
