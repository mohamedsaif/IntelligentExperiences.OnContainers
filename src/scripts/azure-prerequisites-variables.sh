PREFIX="ie${RANDOM}"
RG="${PREFIX}-rg"
LOCATION="westeurope"
FRAMES_STORAGE="${PREFIX}framesstg"
COSMOSDB_ACCOUNT="${PREFIX}telemetrydb"
SB_NAMESPACE="${PREFIX}-ns"
CS_ACCOUNT="${PREFIX}-cs"
CONTAINER_REGISTRY_NAME="${PREFIX}contosoacr"
VNET_NAME="${PREFIX}-network"
WORKSPACE_NAME="${PREFIX}-logs"

#If you wish to have these values persist across sessions use:
echo export PREFIX=$PREFIX >> ~/.bashrc
echo export RG=$RG >> ~/.bashrc
echo export LOCATION=$LOCATION >> ~/.bashrc
echo export FRAMES_STORAGE=$FRAMES_STORAGE >> ~/.bashrc
echo export COSMOSDB_ACCOUNT=$COSMOSDB_ACCOUNT >> ~/.bashrc
echo export SB_NAMESPACE=$SB_NAMESPACE >> ~/.bashrc
echo export CS_ACCOUNT=$CS_ACCOUNT >> ~/.bashrc
echo export CONTAINER_REGISTRY_NAME=$CONTAINER_REGISTRY_NAME >> ~/.bashrc
echo export VNET_NAME=$VNET_NAME >> ~/.bashrc
echo export WORKSPACE_NAME=$WORKSPACE_NAME >> ~/.bashrc