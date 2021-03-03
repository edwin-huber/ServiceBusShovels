#!/bin/bash

# Function app and storage account names must be unique.
storageName=mystorageaccount$RANDOM
# functionAppName=myserverlessfunc$RANDOM
# resourceGroupName=shovelDemo
region=northeurope

# Create a resource group.
az group create --name $resourceGroupName --location $region

# Create an Azure storage account in the resource group.
az storage account create \
  --name $storageName \
  --location $region \
  --resource-group $resourceGroupName \
  --sku Standard_LRS

# Create a serverless function app in the resource group.
# if you do not specify os, it defaults to windows
az functionapp create \
  --name $functionAppName \
  --storage-account $storageName \
  --consumption-plan-location $region \
  --resource-group $resourceGroupName \
  --functions-version 2
  --os-type linux