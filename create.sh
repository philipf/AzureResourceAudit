#az login
#az account set -s (subcriptionId/Name)

subid=`az account show --query id -o tsv`
l=eastus2
g=rg-audits-prod
staname=staauditprod
appname=resource-audit-ab
eventsub=event-audit

az group create -g $g -l $l

az storage account create -l $l -g $g -n $staname --sku Standard_LRS --kind StorageV2 --access-tier Hot

# cn=`az storage account show-connection-string --name $staname --query connectionString -o tsv`

az functionapp create \
    --runtime dotnet \
    -c $l \
    -g $g \
    --name $appname \
    --os-type Linux \
    -s $staname \
    --functions-version 3 

# From the root of src directory
func azure functionapp publish $appname

# https://github.com/Azure/azure-cli/issues/12092
# Creating event grid topic subscription to Azure Function
az extension remove --n eventgrid
az extension add --n eventgrid

sourcesubid=$subid

az eventgrid event-subscription create --name $eventsub \
    --source-resource-id /subscriptions/$sourcesubid \
    --endpoint-type azurefunction \
    --endpoint /subscriptions/$subid/resourceGroups/$g/providers/Microsoft.Web/sites/$appname/functions/AuditSubscriptionEvents \
    --included-event-types Microsoft.Resources.ResourceWriteSuccess Microsoft.Resources.ResourceDeleteSuccess Microsoft.Resources.ResourceActionSuccess

#az group delete -g $g 