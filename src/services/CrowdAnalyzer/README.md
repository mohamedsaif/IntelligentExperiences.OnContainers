# CamFrameAnalyzer

## Needed settings for [local.settings.json]

You need to create a new local.settings.json file in the root of the service and add/configure the following in order to be able to set the function locally and generate the KEDA deployment file correctly:

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
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "APPINSIGHTS_INSTRUMENTATIONKEY": "",
    "origin": "CrowdDemographics.V1.0.0",
    "checkForDbConsistency": false,
    "serviceBusConnection": "",
    "camFrameStorageConnection": "",
    "cognitiveKey": "",
    "cognitiveEndpoint": "https://westeurope.api.cognitive.microsoft.com/",
    "cosmosDbEndpoint": "",
    "cosmosDbKey": "",
    "faceWorkspaceDataFilter": "Contoso.CrowdAnalytics",
    "demographicsWindowMins": 60,
    "KEDA_SERVICE_BUS_CONNECTION": ""
  }
}

```

>NOTE: In the prerequisites provisioning, you created a topic specific service bus connection string. Update ```KEDA_SERVICE_BUS_CONNECTION``` value with the topic specific connection while the ```serviceBusConnection``` will use the namespace level connection.

## KEDA Deployment File Generation

```bash

func kubernetes deploy --name crowd-analyzer --registry $CONTAINER_REGISTRY_NAME.azurecr.io/crowdanalytics --dotnet --dry-run > deploy-updated.yaml

```
