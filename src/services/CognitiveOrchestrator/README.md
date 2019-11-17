# Cognitive Orchestrator

This is a regular Azure Function C# project triggered by Azure Service Bus topic.

## Quick Tips

### Creating new Azure Function in VS Code

You can leverage the Azure Functions VS Code extension to easily create new function.

You can also use Azure Functions Core tools to do as well through ```func new```

### Adding Docker Support

To generate a docker file on your existing Azure Function project, just run the following command (make sure you are in the Function project root directory):

```bash

func init . --docker-only

```

### Deploy KEDA Runtime to Kubernetes

In order to leverage KEDA in any Kubernetes cluster, you need to deploy KEDA components first.

Azure Functions Core Tools make it easy using the following command:

```bash

func kubernetes install --namespace keda

```

>NOTE: Make sure your ```kubectl``` context is set to the target kubernetes cluster

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

```

>NOTE: If you deployed Azure Firewall in a secure AKS deployment, make sure that a rule to allow osiris images to be pulled from osiris.azurecr.io/osiris:6b69328 or the pods will fail to start

If you faced issues in deploying KEDA, you can remove the deployment and consult the [KEDA documentations](https://keda.sh/)

```bash

func kubernetes remove --namespace keda

```

### Generating Kubernetes Deployment Manifest

Deploying to Kubernetes is also super simple, and Azure Functions Core Tools help by generating (or executing) the deployment yaml file.

```bash

func kubernetes deploy --name cognitive-orchestrator --registry $CONTAINER_REGISTRY_NAME.azurecr.io/services --dotnet --dry-run > deploy-updated.yaml

```

Check out the generated deployment file and update it accordingly.

>NOTE: If you are pushing to private Azure Container Registry, please make sure you are authenticated correctly. Refer to [prerequisites guidelines](../../../guide/02-prerequisites/README.md) for more information.

For example, if you need to leverage AKS Virtual Nodes capability, you can amend the tolerations to allow deployment to both cluster nodes and virtual nodes:

```yaml

tolerations:
  - operator: Exists

```

>NOTE: To use AKS Virtual Nodes, you need first to enable it on your AKS cluster. Check out the [documentation here using Azure CLI](https://docs.microsoft.com/en-us/azure/aks/virtual-nodes-cli)
