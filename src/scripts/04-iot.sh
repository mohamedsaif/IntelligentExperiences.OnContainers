# Adding IoT extension to Azure CLI (for IoT specific commands)
az extension add --name azure-cli-iot-ext

# Be patient as this would take a few mins
IOT_HUB_NAME="${PREFIX}-iothub"
az iot hub create \
    --name $IOT_HUB_NAME \
    --resource-group $RG \
    --sku S1

# If you wish to get the IoT Hub connection string (maybe to connect a Device Explorer to it) use the following:
# This command get the default policy and primary key connection string
az iot hub show-connection-string --name $IOT_HUB_NAME

## Enabling File Upload feature in IoT Hub
# Docs: https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-configure-file-upload-cli
# Get the storage account to be used by IoT Hub
# Note: refer back to the prerequisites guide for details
echo $FRAMES_STORAGE_CONN
echo $FRAMES_STORAGE_CONTAINER

# Enable File Upload feature by linking IoT Hub to a Azure Storage account.
# Set storage connection string
az iot hub update \
    --name $IOT_HUB_NAME \
    --fileupload-storage-connectionstring "$FRAMES_STORAGE_CONN" \
    --fileupload-storage-container-name "$FRAMES_STORAGE_CONTAINER"

az iot hub update \
    --name $IOT_HUB_NAME \
    --add properties.storageEndpoints.'$default'.connectionString="$FRAMES_STORAGE_CONN"

# Set storage container name
az iot hub update \
    --name $IOT_HUB_NAME \
    --set properties.storageEndpoints.'$default'.containerName=$FRAMES_STORAGE_CONTAINER

# Shared Access Signature Time to Live: 1 hour (a device have 1 hour to upload or the temp key will expire)
az iot hub update \
    --name $IOT_HUB_NAME \
    --set properties.storageEndpoints.'$default'.sasTtlAsIso8601=PT1H0M0S

# Enable upload notification
az iot hub update --name $IOT_HUB_NAME \
  --set properties.enableFileUploadNotifications=true

# Maximum number of times the IoT Hub attempts to deliver a file upload notification. Set to 10 by default
az iot hub update --name $IOT_HUB_NAME \
  --set properties.messagingEndpoints.fileNotifications.maxDeliveryCount=100

# Notification Time to Live (one day by default)
az iot hub update --name $IOT_HUB_NAME \
  --set properties.messagingEndpoints.fileNotifications.ttlAsIso8601=PT1H0M0S

# Review the IoT Hub updated settings:
az iot hub show --name $IOT_HUB_NAME

echo $SB_TOPIC_ORCH_CONNECTION

# We will need the subscription id:
SUB_ID=$(az account show --query id -o tsv)

# Setup some variables
ENDPOINT_NAME="cognitive-request-sb-topic"
ENDPOINT_TYPE="ServiceBusTopic"
ROUTE_NAME="cognitive-request-sb-topic-route"
CONDITION='$body.TargetAction="CamFrameAnalysis"'

# Register routing endpoint for the Service Bus topic.
# This uses the Service Bus topic connection string.
az iot hub routing-endpoint create \
  --connection-string $SB_TOPIC_ORCH_CONNECTION \
  --endpoint-name $ENDPOINT_NAME \
  --endpoint-resource-group $RG \
    --endpoint-subscription-id $SUB_ID \
  --endpoint-type $ENDPOINT_TYPE \
  --hub-name $IOT_HUB_NAME \
  --resource-group $RG

# Set up the message route for the Service Bus queue endpoint.
az iot hub route create \
  --name $ROUTE_NAME \
  --hub-name $IOT_HUB_NAME \
  --source-type devicemessages \
  --resource-group $RG \
  --endpoint-name $ENDPOINT_NAME \
  --enabled \
  --condition $CONDITION

WEB_DEVICE_ID="WebCam001"

# Create new Edge Device in IoT Hub
az iot hub device-identity create \
    --device-id $WEB_DEVICE_ID \
    --hub-name $IOT_HUB_NAME

# List devices in IoT Hub. You should EdgeCam device with disconnected state
az iot hub device-identity list --hub-name $IOT_HUB_NAME

# Retrieve device connection string. Take note of that as we will use it during the runtime provisioning
WEBCAM_DEVICE_CONNECTION=$(az iot hub device-identity show-connection-string \
    --device-id $WEB_DEVICE_ID \
    --hub-name $IOT_HUB_NAME \
    --query connectionString -o tsv)
echo $WEBCAM_DEVICE_CONNECTION

# Saving variables
echo export WEBCAM_DEVICE_CONNECTION=$WEBCAM_DEVICE_CONNECTION >> ./crowdanalytics