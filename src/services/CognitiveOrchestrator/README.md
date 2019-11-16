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

### Generating Kubernetes Deployment Manifest

Deploying to Kubernetes is also super simple, and Azure Functions Core Tools help by generating (or executing) the deployment yaml file.

```bash

func kubernetes deploy --name CognitiveOrchestrator --registry <docker-user-id> --dotnet --dry-run > deploy.yaml

```

Check out the generated deployment file and update it accordingly.

For example, if you need to leverage AKS Virtual Nodes capability, you can amend the tolerations to allow deployment to both cluster nodes and virtual nodes:

```yaml

tolerations:
  - operator: Exists

```

>NOTE: To use AKS Virtual Nodes, you need first to enable it on your AKS cluster. Check out the [documentation here using Azure CLI](https://docs.microsoft.com/en-us/azure/aks/virtual-nodes-cli)
