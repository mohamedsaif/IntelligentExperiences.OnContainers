# IoT Edge

[Azure IoT Edge](https://docs.microsoft.com/en-us/azure/iot-edge/) is an Internet of Things (IoT) service that builds on top of IoT Hub. This service is meant for customers who want to analyze data on devices, or "at the edge," instead of in the cloud. By moving parts of your workload to the edge, your devices can spend less time sending messages to the cloud and react more quickly to events.

Gateways in IoT Edge solutions provide device connectivity and edge analytics to IoT devices that otherwise wouldn't have those capabilities.

Azure IoT Edge can be used to satisfy all needs for an IoT gateway regardless of whether they are related to connectivity, identity, or edge analytics.

Gateway patterns in this article only refer to characteristics of downstream device connectivity and device identity, not how device data is processed on the gateway.

![iot-edge-gateway](assets/edge-as-gateway.png)

## Installing IoT Edge

For the purpose of this workshop, we will use the Transparent design pattern for simplicity.

We will be building our IoT Edge on Linux to support advance future deployment to a Linux based real device (like Raspberry Pi).

>NOTE: In a production scenario, Windows devices should only run Windows containers. However, a common development scenario is to use a Windows computer to build IoT Edge modules for Linux devices. The IoT Edge runtime for Windows allows you to run Linux containers for **testing and development** purposes. Check the links later for the supported deployments instructions.

Full documentation on how to [install IoT Edge on Windows can be found here](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-windows-with-linux)

You can use Azure virtual machine as your **IoT Edge Device** if for some reason you can't use the development machine to install IoT Edge.

```bash

az vm create \
    --resource-group $RG \
    --name EdgeDeviceVM \
    --image MicrosoftWindowsDesktop:Windows-10:rs5-pro:latest \
    --admin-username azureuser \
    --admin-password {password} \
    --size Standard_DS1_v2

```

Navigate to your new Windows virtual machine in the Azure portal:

- Select **Connect**.
- On the RDP tab, select Download RDP File.
- Open this file with Remote Desktop Connection to connect to your Windows virtual machine using the administrator name and password you specified with the ```az vm create``` command.

Look for more details getting started check this [quick start](https://docs.microsoft.com/en-us/azure/iot-edge/quickstart).

Check [IoT Edge supported systems](https://docs.microsoft.com/en-us/azure/iot-edge/support) for more details.

### Docker Desktop

#### Development

To effectively develop IoT Edge solutions on a dev machine, you need to have Docker Desktop installed and running on your target machine.

#### Edge Runtime

It is worth mentioning this is also a critical to have Docker Desktop if you are using Windows as Edge device and building Linux containers. Docker Desktop must be in Linux containers mode.

If you are using Mac/Linux, you are good to go already :)

### Manual Device Provisioning

We will use a manually registered edge device connection to provision and connect our new IoT Edge device.

```bash

DEVICE_ID="EdgeCam"

# Create new Edge Device in IoT Hub
az iot hub device-identity create \
    --device-id $DEVICE_ID \
    --hub-name $IOT_HUB_NAME \
    --edge-enabled

# List devices in IoT Hub. You should EdgeCam device with disconnected state
az iot hub device-identity list --hub-name $IOT_HUB_NAME

# Retrieve device connection string. Take note of that as we will use it during the runtime provisioning
EDGE_DEVICE_CONNECTION=$(az iot hub device-identity show-connection-string \
    --device-id $DEVICE_ID \
    --hub-name $IOT_HUB_NAME \
    --query connectionString -o tsv)
echo $EDGE_DEVICE_CONNECTION

```

### Installing IoT Edge Runtime

The IoT Edge runtime is deployed on all IoT Edge devices.
It has three components:

1. The **IoT Edge security** daemon starts each time an IoT Edge device boots and bootstraps the device by starting the other components.
2. The **IoT Edge agent** which manages deployment and monitoring of modules on the IoT Edge device, including the IoT Edge hub.
3. The **IoT Edge hub** handles communications between modules on the IoT Edge device, and between the device and IoT Hub.

#### Deploy on Windows Device

Powershell script can be used to easily execute all the required commands to install IoT Edge on Windows.

>NOTE: The steps in this section all take place on your IoT Edge device, so you want to connect to that virtual machine now via remote desktop if you opted to use one.

Start a new PowerShell 64 session (x86 version will not work. You can know as the name will include x86) as an administrator.

>NOTE: I prefer using PowerShell ISE as it has a UI component that allows you to easily organize your code execution.
![powershell-ise](assets/powershell.png)

```powershell

# Deploy IoT Edge runtime
# The Deploy-IoTEdge command downloads and deploys the IoT Edge Security Daemon and its dependencies.
# You might need to reboot. Buckle up :)
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

### Updating IoT Edge Runtime

If you want to update the IoT Edge Runtime in the future, you can simply use the following:

```powershell

. {Invoke-WebRequest -useb https://aka.ms/iotedge-win} | Invoke-Expression; `
Update-IoTEdge -ContainerOs Linux

```

### Uninstalling IoT Edge

Just run the following command:

```powershell

. {Invoke-WebRequest -useb aka.ms/iotedge-win} | Invoke-Expression; `
Uninstall-IoTEdge

```

This command removes the IoT Edge runtime, along with your existing configuration and the Moby engine data.

## Supported IoT Edge Deployment for Production

For production scenarios, please consider one of the following deployment:

### IoT Edge on Linux

Linux on Linux containers deployment details can be [found here](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux)

### IoT Edge on Windows

Linux on Linux containers deployment details can be [found here](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-windows)