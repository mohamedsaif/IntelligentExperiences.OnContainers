![banner](assets/banner.png)

# Dev Environment Setup

These are the steps of one of the ways to start developing and deploying cloud-native solutions targeting Azure.

## Azure Subscription

To complete this how-to, you need an Azure subscription.

You can leverage an organization provided subscription or [Visual Studio subscription](https://my.visualstudio.com/) benefit.

If you don't have an Azure subscription, create a [free account](https://azure.microsoft.com/free) before you begin.

Once you are good with Azure Subscription, you can visit [Azure Portal](https://portal.azure.com) to access your subscription anytime.

## WSL on Windows 10 PC

As you build software that leverage Open Source, and you still in love with your Windows 10 PC, I would highly recommend Enabling [Windows Subsystem for Linux (WSL)](https://docs.microsoft.com/en-us/windows/wsl/install-win10).

Please follow the instruction in the documentation above to get started.

>NOTE: If you want to use the new WSL 2, follow these documentation [Installing WSL 2](https://docs.microsoft.com/en-us/windows/wsl/wsl2-install)

## Git for Windows

If this is the first time to Git on your dev machine, you might need to install Windows Git on your system.

You can download and install Git from [here](https://git-scm.com/downloads)

## Azure CLI

Download and install [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) for your relevant OS.

>NOTE: At the time of writing the code, I used version 2.3.0. Make sure you are running same or later version. Check with ```az --version```

## Kubectl

Kubectl is the CLI for accessing the kubernetes APIs.

You can use Azure CLI to install the kubectl:

```bash

az aks install-cli

```

Follow the [documentation here](https://kubernetes.io/docs/tasks/tools/install-kubectl/) to install on your target OS.

## Helm 3

Helm is the kubernetes native package manager that is widely used by the community.

>NOTE: We need KEDA (Kubernetes Event-Drive Autoscaler) for our platform. KEDA will be installed on AKS through Helm 3.

To install Helm, you can run the following:

```bash

# Helm 3 Installation Docs (https://helm.sh/docs/intro/install/)

# Check first if you have helm 3 installed
helm version

# If you got command not found, then you need to install it (or if you get helm 2 version you need to upgrade)
# Installing helm 3
curl -sL https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3 | sudo bash

# OR
# wget https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3
# chmod -R +x .
# ./get-helm-3

helm version
# You should get something like:
# version.BuildInfo{Version:"v3.1.2", GitCommit:"d878d4d45863e42fd5cff6743294a11d28a9abce", GitTreeState:"clean", GoVersion:"go1.13.8"}

```

## jq Installation

jq is a json data management lib that can make json manipulation very easy.

jq installation is dependent on your platform OS

Below is the command you can run under Debian based OS (like Ubuntu):

```bash

sudo apt-get install jq

```

For information on installing it on other platforms like Mac OSX, refer to [jq documentation](https://stedolan.github.io/jq/download/)

## .NET Core SDK

If you will be using this workshop to run some components locally, it is highly recommended that you install .NET Core SDK.

As the microservices in the workshop are written in C#, you need to install [.NET Core SDK here](https://dotnet.microsoft.com/download/dotnet-core/sdk-for-vs-code?utm_source=vs-code&amp;utm_medium=referral&amp;utm_campaign=sdk-install)

Below is the installation for Linux:

```bash

# Adding microsoft packages
wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# Installing .NET Core SDK
sudo add-apt-repository universe
sudo apt-get update
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1

```

>NOTE: If you will be running the simulated camera device locally on your machine, you need to have .NET Core SDK as the camera device app is an ASP .NET Core application that runs on your browser.

## Docker Desktop

[Docker Desktop](https://www.docker.com/products/docker-desktop) is the way to go to build cloud-ready on your desktop.

>NOTE: Docker Desktop install a version of kubectl as well


## VS Code

Download and install [VS Code](https://code.visualstudio.com/)

>NOTE: You can also use the full fledge Visual Studio 2019 or later

### VS Code Extension

I would highly recommend installing the following extensions:

- [Remote WSL](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-wsl)
- [C#](https://marketplace.visualstudio.com/items?itemName=ms-vscode.csharp)
- [Kubernetes](https://marketplace.visualstudio.com/items?itemName=ms-kubernetes-tools.vscode-kubernetes-tools)
- [Azure Functions](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-azurefunctions)
- [Azure IoT Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools)

## Azure Functions Core Tools

Follow [the installation guide](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local) to make sure it is installed on your target dev environment.

As we will be using [KEDA (Kubernetes-based Event Driven Autoscaling)](https://github.com/kedacore/keda) to deploy event driven APIs as part of the platform services, we will be using Azure Functions Core tools to help with the needed development time and kubernetes deployment tooling.

>NOTE: At the time of writing the code, I used Azure Functions Core Tools version 2.7.1948

## Azure Account Check

### Azure Permissions

If you are running the workshop under a restricted subscription (provided by your organization), you need to perform the pre-flight permissions checks.

>NOTE: To validate that you are able execute the workshop under restricted subscription, I highly recommend executing the [prerequisites guide steps](../02-prerequisites/README.md) ahead of the workshop.

1. Your Azure Account should have "Owner" permission on a Resource Group
2. Azure Kubernetes Service (AKS) Service Principal: in order to be able to provision AKS, a Service Principal (created or existing) should be provided.
   - AKS Service principal needs to have "Contributor" access on the project resource group.
3. Azure Container Register (ACR) Service Principal: in order to integrate Azure DevOps with ACR, we need a Service Principal (created or existing) should be provided
   - ACR Service principal needs to have "AcrPull" and "AcrPush" permissions on the project ACR

### Resource Providers

Some subscription don't have all Azure services resource providers registered.

For example if the subscription don't have "Microsoft.DocumentDB" provider registered, you will not be able to create Cosmos DB.

A script to make sure all needed resource providers are registered is available here [00-resource-providers.sh](../../src/scripts/00-resource-providers.sh). Note this require a subscription owner account to execute.

>NOTE: Resource providers registration is a one time job at the subscription level and don't have any cost implications (just enabling the subscription to use certain services like ACR, AKS, Cosmos DB,...)

### Azure Subscription Limits

Some accounts (specially trial accounts) has limits on how many vCPU you can provision on the subscription.

AKS will leverage vCPUs for the workers VMs. You need to make sure you have enough vCPU of the target AKS workers nodes size in the target region.

>NOTE: This workshop guid is using (Standard BS Family vCPUs) with SKU (Standard_B2s) which has 2 vCPU x 3 VMs (AKS will have 3 worker nodes) = 6 vCPU will be the total.

Below is some guidance on how you can check and deal with that.

```bash

# To view the current limits for a specific location:
az vm list-usage -l $LOCATION -o table

# Look for the CurrentValue vs. Limit for the following:
# Name                               CurrentValue    Limit
# ---------------------------------  --------------  -------
# Total Regional vCPUs               10              20
# Virtual Machines                   6               25000
# Virtual Machine Scale Sets         1               2500
# Standard BS Family vCPUs           8               20
# Standard DSv3 Family vCPUs         0               20
# Standard DSv2 Family vCPUs         0               20
# Standard Storage Managed Disks     2               50000
# Premium Storage Managed Disks      4               50000

# This workshop guid is using (Standard BS Family vCPUs) with SKU (Standard_B2s) which has 2 vCPU x 3 VMs (AKS will have 3 worker nodes) = 6 vCPU will be the total.

```

You might hit some subscription service provisioning limits during creating the AKS cluster and get something like:

```bash

compute.VirtualMachinesClient#CreateOrUpdate: Failure sending request: StatusCode=0 -- Original Error: autorest/azure: Service returned an error. 
Status=<nil> Code="OperationNotAllowed" Message="Operation results in exceeding quota limits of Core. Maximum allowed: 0, Current in use: 0
, Additional requested: 6.

```

You might want to check other regions to deploy the resources where you might have available limit.

You can also solve this submitting a new support request here: [https://aka.ms/ProdportalCRP/?#create/Microsoft.Support/Parameters/](https://aka.ms/ProdportalCRP/?#create/Microsoft.Support/Parameters/)

Use the following details:
Type: Service and subscription limits (quotas)
Subscription: Select target subscription
Problem type: Compute-VM (cores-vCPUs) subscription limit increases
Click add new quota details (increase from 0 to 10 as the new quota for example)

## Next step

Congratulations on completing this section. Let's move to the next step:

[Next Step](../01-architecture/README.md)
