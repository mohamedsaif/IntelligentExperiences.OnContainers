# Azure Functions and KEDA

This is a regular Azure Function C# project triggered by Azure Service Bus topic.

As Kubernetes (AKS) will be our main orchestrator, I've opted to deploy Azure Functions to AKS and leverage the KEDA project.

The primary services of the platform are event driven, KEDA and Azure Functions make it really easy to build such scenarios so you can focus on building the actual services rather than worrying bout the pluming.

>NOTE: Read more about [KEDA and Azure Functions](https://docs.microsoft.com/en-us/azure/azure-functions/functions-kubernetes-keda) here. Also check out this [1.0 Release](https://cloudblogs.microsoft.com/opensource/2019/11/19/keda-1-0-release-kubernetes-based-event-driven-autoscaling/) by Mr. Serverless (Jeff Hollan).

Check out also these samples [kedacore/samples](https://github.com/kedacore/samples)

## Quick Tips

### Azure Functions Core Tools

As part for the prerequisites, you should have installed Azure Functions Core Tools. Please refer back to the setup documentation for further details.

### Creating new Azure Function in VS Code

You can leverage the Azure Functions VS Code extension to easily create new function.

You can also use Azure Functions Core tools to do as well through ```func new```

### Custom NuGet Source

As the project uses a custom feed to consume tailored and none-public packages, you can find a [nuget.config](nuget.config) file adding Azure DevOps Artifacts as source.

You can use the following command to add custom package if you are using VS Code:

```shell

dotnet add package CoreLib -s https://ORGANIZATION.pkgs.visualstudio.com/PROJECT/_packaging/Mo.Packages/nuget/v3/index.json

```

In Visual Studio, the experience is a bit easier. Just go to the settings and add a new custom NuGet source which will then allow you to use the normal **Manage NuGet Packages** project right click action super simple. Just change the search in option from the drop down list in the top right.

### Adding Docker Support

To generate a docker file on your existing Azure Function project, just run the following command (make sure you are in the Function project root directory):

```bash

func init . --docker-only

```

>NOTE: Check the generated Dockerfile and update the path ```/src/dotnet-function-app``` to your relevant function app folder.

It is worth mentioning that the generated docker is doing a multi-stage container building. This means it uses SDK container (fat container) to build your source code, then build another runtime-only container (leaner). This allows you to have platform-independent build (you don't need the machine building the container to have .NET Core SDK installed for example)

### Update local.settings.json

This particular function requires various settings to be present at runtime (like Azure Storage and service bus connections). Updating the local.settings.json will allow the automatically generated Kubernetes deployment to have the needed Kubernetes secrets.

This is a snapshot that you can update and include in your local.settings.json:

```json

{
  "IsEncrypted": false,
  "serviceBus": {
    "prefetchCount": 100,
    "messageHandlerOptions": {
        "autoComplete": true,
        "maxConcurrentCalls": 32,
        "maxAutoRenewDuration": "00:55:00"
    }
  },
  "Values": {
    "AzureWebJobsStorage": "REPLACE",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "serviceBusConnection": "REPLACE"
  }
}


```

### Deploy KEDA Runtime to Kubernetes

In order to leverage KEDA in any Kubernetes cluster, you need to deploy KEDA components first.

>**NOTE:** You can check the documentation of [KEDA deployment](https://keda.sh/deploy/) for more information.

#### Using Helm 3 (recommended)

I'm using Helm 3 to perform the deployment.

```bash

# Adding KEDA repo
helm repo add kedacore https://kedacore.github.io/charts
helm repo update

# Installing KEDA in keda namespace
kubectl create namespace keda
helm install keda kedacore/keda --namespace keda


```

#### Using Azure Functions CLI

Azure Functions Core Tools make it easy using the following command:

```bash

func kubernetes install --namespace keda

```

>NOTE: Make sure your ```kubectl``` context is set to the target kubernetes cluster

### Validate KEDA installation

Review that installation was successful and in running state:

```bash

kubectl get po -n keda

# You should get something like this:
# NAME                                                       READY   STATUS    RESTARTS   AGE
# keda-59748d7c48-l226t                                      1/1     Running   0          26s
# osiris-osiris-edge-activator-6f6c7db4dd-r4xzl              1/1     Running   0          21s
# osiris-osiris-edge-endpoints-controller-74f5cc99f8-h64qg   1/1     Running   0          21s
# osiris-osiris-edge-endpoints-hijacker-5b4776fb9b-8b6s9     1/1     Running   0          20s
# osiris-osiris-edge-proxy-injector-769cbdfcc7-5l8j6         1/1     Running   0          20s
# osiris-osiris-edge-zeroscaler-754f77b757-cqz88             1/1     Running   0          19s

# KEDA create also a new (scaledobjects.keda.k8s.io) CRS which will be used in kubernetes deployments.
kubectl get customresourcedefinition

```

>NOTE: If you deployed Azure Firewall in a secure AKS deployment, make sure that a rule to allow osiris images to be pulled from osiris.azurecr.io/osiris:6b69328 or the pods will fail to start.

If you have challenges, refer to the diagnostic section below.

### Generating Kubernetes Deployment Manifest

Deploying to Kubernetes is also super simple, and Azure Functions Core Tools help by generating (or executing) the deployment yaml file.

```bash

func kubernetes deploy --name cognitive-orchestrator --registry $CONTAINER_REGISTRY_NAME.azurecr.io/services --dotnet --dry-run > deploy-updated.yaml

```

Check out the generated deployment file and update it accordingly.

>NOTE: If you are pushing to private Azure Container Registry, please make sure you are authenticated correctly. Refer to [prerequisites guidelines](../../../guide/02-prerequisites/README.md) for more information.

If you need to leverage AKS Virtual Nodes capability (for a serverless infinite scale), you can amend the tolerations to allow deployment to both cluster nodes and virtual nodes:

```yaml

tolerations:
  - operator: Exists

```

>NOTE: To use AKS Virtual Nodes, you need first to enable it on your AKS cluster. Check out the [documentation here using Azure CLI](https://docs.microsoft.com/en-us/azure/aks/virtual-nodes-cli)

Another important note when using KEDA with Azure Service Bus, you need to have a connection string that scoped at the level of the topic (not at the namespace level). That is why I added a special service bus connection SAS into a separate variable under the secrets deployment ``` KEDA_SERVICE_BUS_CONNECTION ``` which can basically can be the same connection but with ```;EntityPath=TOPIC_NAME``` at the end.

During the prerequisites steps, you already created a SAS policy for each topic to be used as the relative KEDA connection.

>NOTE: You can check the current enhancement issue mentioned above on [GitHub](https://github.com/kedacore/keda/issues/215)

>NOTE: For simplicity, the function app uses only one connection to Service Bus to both receive from one topic and send to another. It can easily done through update the configuration and code to use 2 different connection string for each Service Bus Topics.

One you are satisfied with the generated deployment file, copy the file to the deployment folder. This is to allow Azure DevOps to copy it out so it can be used in the release pipeline.

#### Diagnose KEDA deployment

If something is not going right, you can check directly KEDA logs:

```bash

kubectl logs $REPLACE_WITH_KEDA_POD_NAME -n keda

```

#### Uninstalling KEDA

If you faced issues in deploying KEDA, you can remove the deployment and consult the [KEDA documentations](https://keda.sh/)

>NOTE: [Deploying KEDA](https://keda.sh/deploy/) shows how you can leverage helm in deploying KEDA (instead of using Azure Functions Core Tools).

Using Helm 3:

```bash

helm uninstall -n keda keda
kubectl delete -f https://raw.githubusercontent.com/kedacore/keda/master/deploy/crds/keda.k8s.io_scaledobjects_crd.yaml
kubectl delete -f https://raw.githubusercontent.com/kedacore/keda/master/deploy/crds/keda.k8s.io_triggerauthentications_crd.yaml

```

Using Azure Functions CLI:

```bash

func kubernetes remove --namespace keda

```

#### Delete Evicted Pods

Sometimes when KEDA autoscaler provision many pods, some of them will be evicted due to memory/cpu constraints. This is okay as the remaining active pods will process the queue message.

To delete all evicted pods in all namespaces, you can use the following command:

```bash

kubectl get pods --all-namespaces -ojson \
  | jq -r '.items[] | select(.status.reason!=null) | select(.status.reason | contains("Evicted")) | .metadata.name + " " + .metadata.namespace' \
  | xargs -n2 -l bash -c 'kubectl delete pods $0 --namespace=$1'

```

#### Sample Deployment File

Note that all caps values are to be replace with values related to your deployment.

Also note that this deployment files adds ```tolerations``` to instruct KEDA to leverage AKS Virtual Nodes.

You can find the deployment file used in the DevOps process here [Deployment/k8s-deployment.yaml](Deployment/k8s-deployment.yaml)

```yaml

apiVersion: v1
kind: Secret
metadata:
  name: crowd-analyzer
  namespace: crowd-analytics
data:
  AzureWebJobsStorage: #{funcStorage}#
  FUNCTIONS_WORKER_RUNTIME: ZG90bmV0 # dotnet
  APPINSIGHTS_INSTRUMENTATIONKEY: #{appInsightsKey}#
  serviceBusConnection: #{serviceBusConnection}#
  KEDA_SERVICE_BUS_CONNECTION: #{kedaServiceBusConnection}#
  origin: Q3Jvd2REZW1vZ3JhcGhpY3MuVjEuMC4w # CrowdDemographics.V1.0.0
  checkForDbConsistency: dHJ1ZQ== #true
  cosmosDbEndpoint: #{cosmosDbEndpoint}#
  cosmosDbKey: #{cosmosDbKey}#
  faceWorkspaceDataFilter: Q29udG9zby5Dcm93ZEFuYWx5dGljcw== # Contoso.CrowdAnalytics
  demographicsWindowMins: NjA= # 60
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: crowd-analyzer
  namespace: crowd-analytics
  labels:
    app: crowd-analyzer
spec:
  selector:
    matchLabels:
      app: crowd-analyzer
  template:
    metadata:
      labels:
        app: crowd-analyzer
    spec:
      containers:
      - name: crowd-analyzer
        image: #{acrName}#/crowdanalytics/crowd-analyzer:#{Build.BuildId}#
        imagePullPolicy: IfNotPresent
        env:
        - name: AzureFunctionsJobHost__functions__0
          value: CrowdAnalyzer
        envFrom:
        - secretRef:
            name: crowd-analyzer
      # If your AKS Virtual Nodes and ACR is configured correctly, you can schedule the scaling on virtual nodes
      # imagePullSecrets:
      # - name: acrImagePullSecret
      # nodeSelector:
      #   kubernetes.io/role: agent
      #   beta.kubernetes.io/os: linux
      #   type: virtual-kubelet
      # tolerations:
      # - key: virtual-kubelet.io/provider
      #   operator: Exists
      # - key: azure.com/aci
      #   effect: NoSchedule
---
apiVersion: keda.k8s.io/v1alpha1
kind: ScaledObject
metadata:
  name: crowd-analyzer
  namespace: crowd-analytics
  labels:
    deploymentName: crowd-analyzer
spec:
  scaleTargetRef:
    deploymentName: crowd-analyzer
  pollingInterval: 30  # Optional. Default: 30 seconds
  cooldownPeriod:  300 # Optional. Default: 300 seconds
  minReplicaCount: 0   # Optional. Default: 0
  maxReplicaCount: 100 # Optional. Default: 100
  triggers:
  - type: azure-servicebus
    metadata:
      connection: KEDA_SERVICE_BUS_CONNECTION
      topicName: crowd-analysis 
      subscriptionName: crowd-analyzer
      queueLength: '10' # This will be used to trigger a scale up operation when number of messages exceed this number
---

```
