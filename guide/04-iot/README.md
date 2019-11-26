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

# IoT Edge

[Azure IoT Edge](https://docs.microsoft.com/en-us/azure/iot-edge/) is an Internet of Things (IoT) service that builds on top of IoT Hub. This service is meant for customers who want to analyze data on devices, or "at the edge," instead of in the cloud. By moving parts of your workload to the edge, your devices can spend less time sending messages to the cloud and react more quickly to events.

Gateways in IoT Edge solutions provide device connectivity and edge analytics to IoT devices that otherwise wouldn't have those capabilities.

Azure IoT Edge can be used to satisfy all needs for an IoT gateway regardless of whether they are related to connectivity, identity, or edge analytics.

Gateway patterns in this article only refer to characteristics of downstream device connectivity and device identity, not how device data is processed on the gateway.

![iot-edge-gateway](assets/edge-as-gateway.png)

## IoT Edge Device Options

The Crowd Analytics platform depends on having incoming camera feeds from IoT devices. IoT Edge runtime provides powerful way to not only manage these devices (auto provision, security, telemetry,..), give internet capability to devices that don't have (like a non-IP camera), but also allowing them to become intelligent as well.

For example having a simple face detection module running on the edge, the device will decide if it needs to send back the image or not.

>NOTE: In production, the recommended approach is to use Azure IoT Edge runtime to manage each camera device.

In the workshop, I opted to use a client that will access the camera feed and send it back to IoT Hub on the development machine. This will a little bit more simple.

I will link additional reads and documentations about [IoT Edge](IOT-EDGE.md) here.

## Manual Device Provisioning

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

## IoT Edge Device.. Workshop Style 🐱‍👓

In the workshop, I've wanted to have the IoT Hub Client as close as possible to a production development. I've decided to put 2 components to work:

1. **Camera Device:** A simple HTML+JS website that uses your dev machine camera and save it to disk using a predefine FPS (frame per second). It will always capture and save the image with the same name creating a simulated camera stream.
2. **IoT Edge Device:** instead of deploying IoT Edge Runtime following the provided guides, we will use our star, Kubernetes. As the webcam write files locally to dev machine, we will run a [Minikube](https://kubernetes.io/docs/setup/minikube/) cluster locally using Docker Desktop. We will then deploy the IoT Edge runtime on the Minikube.

>NOTE: This actually reflect one of the key benefit of IoT Edge, connecting none-internet capable device (our HTML site) to IoT Hub and act as the gateway between them.

What will be doing is:

1. Having local Kubernetes cluster up and running
2. Install Helm on the cluster
3. Add IoT Edge helm repo
4. Install IoT Edge helm repo
5. Validate the installation

### Local Kubernetes

Check out Minikube documentation for further information about having it installed.

For the workshop, we will leverage Docker Desktop built-in single Kubernetes cluster.

If you didn't already enabled it, head to Docker Desktop Settings -> Kubernetes and enable it.

![kubernetes](assets/k8s.png)

For sure you already have a kubectl installed and probably connected to your AKS cluster. All what we need to do is to switch kubectl context to our ```docker-for-desktop``` Kubernetes cluster:

```bash

kubectl config get-contexts
kubectl config use-context docker-for-desktop

# Let's see how many nodes in our huge Kubernetes cluster :D
kubectl get nodes
# You should get something similar to:
# NAME             STATUS   ROLES    AGE   VERSION
# docker-desktop   Ready    master   11m   v1.14.7

```

>NOTE: For more documentation check [Docker Docs](https://docs.docker.com/docker-for-windows/#kubernetes)

### Installing Helm

Just head to [Helm Docs](https://helm.sh/docs/intro/install/) to get documentation related to the installing for helm CLI.

>NOTE: You can also check [AKS-Adv-Provision](https://github.com/mohamedsaif/AKS-Adv-Provision) on GitHub for more details about helm and more (just search for helm).

Initialize helm (to install teller) on our Kubernetes cluster.

```bash

helm init
# You should get something like:
# $HELM_HOME has been configured at [some path/.helm].
# Tiller (the Helm server-side component) has been installed into your Kubernetes Cluster.

```

### Adding IoT Edge Helm Repo

Simple, execute the following command:

```shell

helm repo add edgek8s https://edgek8s.blob.core.windows.net/helm/
helm repo update

```

### Install IoT Edge

Helm makes deployment on Azure a smooth experience. Just execute the following commands to get started:

```shell

# Make sure you have your edge device connection string set correctly
helm install \
--name edge-cam \
--set "deviceConnectionString=${EDGE_DEVICE_CONNECTION}" \
edgek8s/edge-kubernetes

# You should recieve a detailed deployment report form teller:
# NAME:   edge-cam
# LAST DEPLOYED: Tue Nov 26 21:28:18 2019
# NAMESPACE: default
# STATUS: DEPLOYED

# RESOURCES:
# ==> v1/ClusterRoleBinding
# NAME                       AGE
# edge-cam-service-account  42s

# ==> v1/ConfigMap
# NAME                                             DATA  AGE
# edge-cam-edge-kubernetes-iotedged-proxy-config  1     42s

# ==> v1/Deployment
# NAME       READY  UP-TO-DATE  AVAILABLE  AGE
# edgeagent  0/1    1           0          42s
# iotedged   1/1    1           1          42s

# ==> v1/Namespace
# NAME                          STATUS  AGE
# msiot-ie22919-iothub-edgecam  Active  42s

# ==> v1/Pod(related)
# NAME                        READY  STATUS             RESTARTS  AGE
# edgeagent-6bd7bbfbfc-vxml7  0/2    ContainerCreating  0         42s
# iotedged-6f659595d-tg9tm    1/1    Running            0         42s

# ==> v1/Secret
# NAME                                       TYPE    DATA  AGE
# edge-cam-edge-kubernetes-iotedged-config  Opaque  1     42s

# ==> v1/Service
# NAME      TYPE       CLUSTER-IP     EXTERNAL-IP  PORT(S)              AGE
# iotedged  ClusterIP  10.103.42.156  <none>       35000/TCP,35001/TCP  42s

# ==> v1/ServiceAccount
# NAME                       SECRETS  AGE
# edge-cam-service-account  1        42s


# NOTES:
# Thank you for installing edge-kubernetes.

# Your release is named edge-cam.

# To learn more about the release, try:

#   $ helm status edge-cam
#   $ helm get edge-cam

# Your resources have been deployed to the namespace "msiot-ie22919-iothub-edgecam"

```

Take a note of the used namespace above to deploy the edge runtime components.

>NOTE: Installation use the following convention ```msiot-<iothub-name>-<edgedevice-name>``` in the namespace

### Validate

```shell

kubectl get po -n REPLCAE_WITH_NAMESPACE
# NAME                         READY   STATUS    RESTARTS   AGE
# edgeagent-6bd7bbfbfc-vxml7   2/2     Running   0          5m4s
# iotedged-6f659595d-tg9tm     1/1     Running   0          5m4s

```
