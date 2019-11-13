![banner](assets/banner.png)

# IoT Hub and IoT Edge Device

Azure IoT Hub is a managed service, hosted in the cloud, that acts as a central message hub for bi-directional 
communication between your IoT application and the devices it manages. You can use Azure IoT Hub to build 
IoT solutions with reliable and secure communications between millions of IoT devices and a cloud-hosted 
solution backend. You can connect virtually any device to IoT Hub.

[Read more about IoT Hub](https://docs.microsoft.com/en-us/azure/iot-hub/about-iot-hub)

Azure IoT Edge is a fully managed service built on Azure IoT Hub. Deploy your cloud workloads—artificial intelligence, Azure and third-party services, or your own business logic—to run on Internet of Things (IoT) edge devices via standard containers. By moving certain workloads to the edge of the network, your devices spend less time communicating with the cloud, react more quickly to local changes, and operate reliably even in extended offline periods.

[Read more about IoT Edge](https://azure.microsoft.com/en-us/services/iot-edge/)

## Variables

```bash

IOT_HUB_NAME="${PREFIX}-iothub"

```

## IoT Hub Provisioning

```bash

az iot hub create \
    --name $IOT_HUB_NAME \
    --resource-group $RG \
    --sku S1

```

## IoT Edge

Gateways in IoT Edge solutions provide device connectivity and edge analytics to IoT devices that otherwise wouldn't have those capabilities. Azure IoT Edge can be used to satisfy all needs for an IoT gateway regardless of whether they are related to connectivity, identity, or edge analytics. Gateway patterns in this article only refer to characteristics of downstream device connectivity and device identity, not how device data is processed on the gateway.

![iot-edge-gateway](assets/edge-as-gateway.png)

### Installing IoT Edge

For the purpose of this workshop, we will use the Transparent design pattern for simplicity.

We will be building our IoT Edge on Linux to support advance future deployment to a Linux based real device (like Raspberry Pi).

>NOTE: In a production scenario, Windows devices should only run Windows containers. However, a common development scenario is to use a Windows computer to build IoT Edge modules for Linux devices. The IoT Edge runtime for Windows allows you to run Linux containers for **testing and development** purposes. Check the links later for the supported deployments instructions.

Full documentation on how to [install IoT Edge on Windows can be found here](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-windows-with-linux)

#### Docker Desktop

To run IoT Edge on a Windows machine and develop Linux containers, all what you need to have Docker Desktop installed and running on your target machine.

It is worth mentioning if you are using Mac, you are good to go already :)

#### Manual Device Provisioning

We will use a manually registered edge device connection to provision and connect our new IoT Edge device.

```bash

DEVICE_ID="devcam"

# Create new Edge Device in IoT Hub
az iot hub device-identity create \
    --device-id $DEVICE_ID \
    --hub-name $IOT_HUB_NAME \
    --edge-enabled

# List devices in IoT Hub
az iot hub device-identity list --hub-name $IOT_HUB_NAME

# Retrieve device connection string
az iot hub device-identity show-connection-string \
    --device-id $DEVICE_ID \
    --hub-name $IOT_HUB_NAME

```

#### Installing on Windows

Powershell script can be used to easily execute all the required commands to install IoT Edge on Windows.

Start a new PowerShell 64 session (32 version will not work) as an administrator.

```powershell

# Deploy IoT Edge runtime
# The Deploy-IoTEdge command downloads and deploys the IoT Edge Security Daemon and its dependencies.
. {Invoke-WebRequest -useb https://aka.ms/iotedge-win} | Invoke-Expression; `
Deploy-IoTEdge -ContainerOs Linux

# Initialize IoT Edge
. {Invoke-WebRequest -useb https://aka.ms/iotedge-win} | Invoke-Expression; `
Initialize-IoTEdge -DeviceConnectionString CONNECTION

```

When prompted, provide the device connection string that you retrieved previous step. The device connection string associates the physical device with a device ID in IoT Hub.

>NOTE:The device connection string takes the following format, and should not include quotation marks: ```HostName={IoT hub name}.azure-devices.net;DeviceId={device name};SharedAccessKey={key}```

Once completed, you can check the status of the IoT Edge service:

```powershell

# IoT Edge service be listed as running
Get-Service iotedge

# List running modules
iotedge list

```

>NOTE: A known Windows operating system issue prevents transition to sleep and hibernate power states when IoT Edge modules (process-isolated Windows Nano Server containers) are running. This issue impacts battery life on the device.

> As a workaround, use the command ```Stop-Service iotedge``` to stop any running IoT Edge modules before using these power states.

#### Updating IoT Edge Runtime

If you want to update the IoT Edge Runtime in the future, you can simply use the following:

```powershell

. {Invoke-WebRequest -useb https://aka.ms/iotedge-win} | Invoke-Expression; `
Update-IoTEdge -ContainerOs Linux

```

#### Uninstalling IoT Edge

Just run the following command:

```powershell

. {Invoke-WebRequest -useb aka.ms/iotedge-win} | Invoke-Expression; `
Uninstall-IoTEdge

```

This command removes the IoT Edge runtime, along with your existing configuration and the Moby engine data.

### Supported IoT Edge Deployment for Production

For production scenarios, please consider one of the following deployment:

#### IoT Edge on Linux

Linux on Linux containers deployment details can be [found here](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux)

#### IoT Edge on Windows

Linux on Linux containers deployment details can be [found here](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-windows)