![banner](assets/banner.png)

# Dev Environment Setup

These are the steps of one of the ways to start developing and deploying cloud-native solutions targeting Azure.

## Azure Account

To complete this how-to, you need an Azure subscription. If you don't have an Azure subscription, create a [free account](https://azure.microsoft.com/free) before you begin.

## Azure CLI

Download and install [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) for your relevant OS.

## Docker Desktop

[Docker Desktop](https://www.docker.com/products/docker-desktop) is the way to go to build cloud-ready on your desktop.

## VS Code

Download and install [VS Code](https://code.visualstudio.com/)

### VS Code Extension

I would highly recommend installing the following extensions:

- [Kubernetes](https://marketplace.visualstudio.com/items?itemName=ms-kubernetes-tools.vscode-kubernetes-tools)
- [Azure IoT Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools)

### Azure Functions Core Tools

As we will be using [KEDA (Kubernetes-based Event Driven Autoscaling)](https://github.com/kedacore/keda) to deploy event driven APIs as part of the platform services, we will be using Azure Functions Core tools to help with the needed development time and kubernetes deployment tooling.

Follow [the installation guide](https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local) to make sure it is installed on your target dev environment.

>NOTE: At the time of writing the code, I used version 2.7.1846

## Next step

Congratulations on completing this section. Let's move to the next step:

[Next Step](/guide/02-prerequisites/README.md)