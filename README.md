# Azure Functions Setup Guide

## Refer details of AZ-204 Exam 
[AZ-204-Developing-Solutions-for-Microsoft-Azure](https://github.com/learnsmartcoding/AZ-204-Developing-Solutions-for-Microsoft-Azure)

## Installing PowerShell on Windows
- Follow the instructions in the [Installing PowerShell on Windows](https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell-on-windows?view=powershell-7.4) guide.
- Download the MSI package from [here](https://github.com/PowerShell/PowerShell/releases/download/v7.4.1/PowerShell-7.4.1-win-x64.msi).

## Installing Azure CLI
- Follow the steps outlined in the [Install Azure CLI on Windows](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli#install-or-update) guide.
- Download the latest MSI file from [here](https://aka.ms/installazurecliwindowsx64).

## Authenticating Azure Account in PowerShell
- If you haven't already authenticated to your Azure account in the current PowerShell session, the script will prompt you to do so when it encounters the `Connect-AzAccount` command.

## Installing Azure PowerShell Module (Az)
1. Install NuGet package provider:
    ```powershell
    Install-PackageProvider -Name NuGet -Force
    ```
2. Install Az module:
    ```powershell
    Install-Module -Name Az -AllowClobber -Scope CurrentUser
    ```
    Select 'A' to trust and install the Azure module when prompted.

## Creating Azure Function App Resources using PowerShell
- Follow the instructions provided in the [Create function app resources in Azure using PowerShell](https://learn.microsoft.com/en-us/azure/azure-functions/create-resources-azure-powershell) guide.

## Creating a Function App for Serverless Code Execution
- Refer to the [Create a function app for serverless code execution](https://learn.microsoft.com/en-us/azure/azure-functions/scripts/functions-cli-create-serverless) guide for step-by-step instructions.
