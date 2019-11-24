![banner](assets/banner.png)

# IoT Hub and IoT Edge Device

Azure IoT Hub is a managed service, hosted in the cloud, that acts as a central message hub for bi-directional 
communication between your IoT application and the devices it manages. You can use Azure IoT Hub to build 
IoT solutions with reliable and secure communications between millions of IoT devices and a cloud-hosted 
solution backend. You can connect virtually any device to IoT Hub.

[Read more about IoT Hub](https://docs.microsoft.com/en-us/azure/iot-hub/about-iot-hub)

Azure IoT Edge is a fully managed service built on Azure IoT Hub. Deploy your cloud workloads—artificial intelligence, Azure and third-party services, or your own business logic—to run on Internet of Things (IoT) edge devices via standard containers. By moving certain workloads to the edge of the network, your devices spend less time communicating with the cloud, react more quickly to local changes, and operate reliably even in extended offline periods.

[Read more about IoT Edge](https://azure.microsoft.com/en-us/services/iot-edge/)

## Azure CLI Extension for IoT

To enable additional functionality of Azure CLI, you need to make sure that IoT extension is installed:

```bash

az extension add --name azure-cli-iot-ext

```

>NOTE: This is important steps to execute IoT Hub specific commands like creating a device identity.

## IoT Hub Provisioning

```bash

# Be patient as this would take a few mins
IOT_HUB_NAME="${PREFIX}-iothub"
az iot hub create \
    --name $IOT_HUB_NAME \
    --resource-group $RG \
    --sku S1

# If you wish to get the IoT Hub connection string (maybe to connect a Device Explorer to it) use the following:
# This command get the default policy and primary key connection string
az iot hub show-connection-string --name $IOT_HUB_NAME

```

## IoT Hub Device Options

The Crowd Analytics platform depends on having incoming camera feeds from IoT devices. IoT Edge runtime provides powerful way to not only manage these devices (auto provision, security, telemetry,..) but also allowing them to become intelligent as well.

For example having a simple face detection module running on the edge, the device will decide if it needs to send back the image or not.

>NOTE: In production, the recommended approach is to use Azure IoT Edge runtime to manage each camera device.

In the workshop, I opted to use a client that will access the camera feed and send it back to IoT Hub on the development machine. This will a little bit more simple.

I will link additional reads and documentations about [IoT Edge](IOT-EDGE.md) here.
