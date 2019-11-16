# As part of your Azure networking, you need to plan the address spaces and subnet.
# Planning tasks should think about providing enough CIDR range to your services and making sure there are no conflicts with other networks that will be connected to this one (like on-premise networks)

# Networking address space requires careful design exercise that you should go through.
# For simplicity I'm using /16 for the address space with /24 for each service

# Virtual Network address space
VNET_ADDRESS_SPACE="10.42.0.0/16"

# AKS primary subnet
AKSSUBNET_NAME="${PREFIX}-akssubnet"

# AKS exposed services subnet
SVCSUBNET_NAME="${PREFIX}-svcsubnet"

# Application Gateway subnet
AGW_SUBNET_NAME="${PREFIX}-appgwsubnet"

# Azure Firewall Subnet name must be AzureFirewallSubnet
FWSUBNET_NAME="AzureFirewallSubnet"

# Virutal nodes subnet (will be managed by Azure Container Instances)
VNSUBNET_NAME="${PREFIX}-vnsubnet"

# IP ranges for each subnet
AKSSUBNET_IP_PREFIX="10.42.1.0/24"
SVCSUBNET_IP_PREFIX="10.42.2.0/24"
AGW_SUBNET_IP_PREFIX="10.42.3.0/24"
FWSUBNET_IP_PREFIX="10.42.4.0/24"
VNSUBNET_IP_PREFIX="10.42.5.0/24"

# Creating our main virtual network
az network vnet create \
    --resource-group $RG \
    --name $VNET_NAME \
    --address-prefixes $VNET_ADDRESS_SPACE \
    --subnet-name $AKSSUBNET_NAME \
    --subnet-prefix $AKSSUBNET_IP_PREFIX

# Create subnet for kubernetes exposed services (usually by internal loadbalancer)
# Good security practice to isolate exposed services from the internal services
az network vnet subnet create \
    --resource-group $RG \
    --vnet-name $VNET_NAME \
    --name $SVCSUBNET_NAME \
    --address-prefix $SVCSUBNET_IP_PREFIX

# Create subnet for App Gateway
az network vnet subnet create \
    --resource-group $RG \
    --vnet-name $VNET_NAME \
    --name $AGW_SUBNET_NAME \
    --address-prefix $AGW_SUBNET_IP_PREFIX

# Create subnet for Azure Firewall
az network vnet subnet create \
    --resource-group $RG \
    --vnet-name $VNET_NAME \
    --name $FWSUBNET_NAME \
    --address-prefix $FWSUBNET_IP_PREFIX

# Create subnet for Virtual Nodes
az network vnet subnet create \
    --resource-group $RG \
    --vnet-name $VNET_NAME \
    --name $VNSUBNET_NAME \
    --address-prefix $VNSUBNET_IP_PREFIX

# Get the Azure IDs the vNet and AKS Subnet for use with AKS SP role assignment
VNET_ID=$(az network vnet show -g $RG --name $VNET_NAME --query id -o tsv)
AKS_SUBNET_ID=$(az network vnet subnet show -g $RG --vnet-name $VNET_NAME --name $AKSSUBNET_NAME --query id -o tsv)
AKS_SVCSUBNET_ID=$(az network vnet subnet show -g $RG --vnet-name $VNET_NAME --name $SVCSUBNET_NAME --query id -o tsv)
AKS_AGWSUBNET_ID=$(az network vnet subnet show -g $RG --vnet-name $VNET_NAME --name $AGW_SUBNET_NAME --query id -o tsv)
AKS_FWSUBNET_ID=$(az network vnet subnet show -g $RG --vnet-name $VNET_NAME --name $FWSUBNET_NAME --query id -o tsv)
AKS_VNSUBNET_ID=$(az network vnet subnet show -g $RG --vnet-name $VNET_NAME --name $VNSUBNET_NAME --query id -o tsv)

# Make sure that IDs set correctly
echo $VNET_ID
echo $AKS_SUBNET_ID
echo $AKS_SVCSUBNET_ID
echo $AKS_AGWSUBNET_ID
echo $AKS_FWSUBNET_ID
echo $AKS_VNSUBNET_ID

# Saving values for later use
echo export VNET_NAME=$VNET_NAME >> ~/.bashrc
echo export SVCSUBNET_NAME=$SVCSUBNET_NAME >> ~/.bashrc
echo export VNET_ID=$VNET_ID >> ~/.bashrc
echo export AKS_SUBNET_ID=$AKS_SUBNET_ID >> ~/.bashrc
echo export AKS_SVCSUBNET_ID=$AKS_SVCSUBNET_ID >> ~/.bashrc
echo export AKS_AGWSUBNET_ID=$AKS_AGWSUBNET_ID >> ~/.bashrc
echo export AKS_FWSUBNET_ID=$AKS_FWSUBNET_ID >> ~/.bashrc
echo export AKS_VNSUBNET_ID=$AKS_VNSUBNET_ID >> ~/.bashrc