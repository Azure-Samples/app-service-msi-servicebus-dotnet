---
services: app-service, servicebus
platforms: dotnet
author: nonik0
---

# Use Service Bus from App Service with Managed Service Identity

## Background
For authenticating from applications to Service Bus, the approach so far involved creating an application and an associated shared access key in Service Bus, and then using that shared access key to authenticate directly with Service Bus. While this approach works well, there are two shortcomings:
1. The application's shared access key is typically hard coded in source code or configuration files. Developers tend to push the code to source repositories as-is, which leads to credentials in source.
2. The application's shared access key never expires.

With [Managed Service Identity (MSI)](https://docs.microsoft.com/en-us/azure/app-service/app-service-managed-service-identity), both these problems are solved. This sample shows how a Web App can authenticate to Azure Service Bus without the need to explicitly create and manage a Service Bus shared access key.

## Prerequisites
To run and deploy this sample, you need the following:
1. An Azure subscription to create an App Service and a Service Bus Namespace. 
2. [Visual Studio 2017 v15.6](https://blogs.msdn.microsoft.com/visualstudio/2017/12/07/visual-studio-2017-version-15-6-preview/) or later.

## Step 1: Create an App Service with a Managed Service Identity (MSI)
<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fnonik0%2Fapp-service-msi-servicebus-dotnet%2Fmaster%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

Use the "Deploy to Azure" button to deploy an ARM template to create the following resources:
1. App Service with MSI.
2. Service Bus Namespace with a Service Bus Queue

Be sure to create a Service Bus Namespace in one of the Azure regions that have preview support for role-based access control (RBAC): **East US**, **East US 2**, or **West Europe**. 

After using the ARM template to deploy the needed resources, review the resources created using the Azure portal. You should see an App Service and a Service Bus Namespace that contains a Service Bus Queue. You can verify your App Service has Managed Service Identity (MSI) enabled by navigating to the App Service, clicking on "Managed service identity (Preview)" in the left-hand menu, and verifying "Register with Azure Active Directory" is set to "on".

## Step 2: Grant yourself access to the Service Bus Namespace
Using the Azure Portal, go to the Service Bus Namespace's access control tab, and grant yourself **Contributor** access to the Service Bus Namespace. This will allow you to run the application on your local development machine (**Note**: There is currently an issue where inherited permissions will not grant access; you must explicity add permissions here).

To grant access:
1.	Search for your Service Bus Namespace in “Search Resources dialog box” in Azure Portal.
2.	In the left-hand menu, select "Access control (IAM)"
3.	Click on "Add", then select "Contributor" from the dropdown for "Role"
4.	Click on the "Select" field, then search for and select your user account/email
5.	Click on "Save" to complete adding your user account as a new "Contributor" for your Service Bus Namespace

## Step 3: Clone the repo 
Clone the repo to your development machine. 

For the Service Bus client library to interact with a queue or topic, it will need to understand how to connect and authorize with it.  The easiest means for doing so is to use a connection string, which is created automatically when creating a Service Bus namespace.  If you aren't familiar with shared access policies in Azure, you may wish to follow the step-by-step guide to [get a Service Bus connection string](https://docs.microsoft.com/azure/service-bus-messaging/service-bus-quickstart-topics-subscriptions-portal#get-the-connection-string).

Once you have a connection string, you can authenticate your client with it.

```C# Snippet:ServiceBusAuthConnString
// Create a ServiceBusClient that will authenticate using a connection string
string connectionString = "<connection_string>";
ServiceBusClient client = new ServiceBusClient(connectionString);
```

Message sending is performed using the `ServiceBusSender`. Receiving is performed using the `ServiceBusReceiver`.

```csharp    
string connectionString = Config.NamespaceConnectionString;
string queueName = Config.Queue;
var client = new ServiceBusClient(connectionString);

// create the sender
ServiceBusSender sender = client.CreateSender(queueName);

// create a message that we can send.
ServiceBusMessage message = new ServiceBusMessage(messageInfo.MessageToSend);

// send the message
await sender.SendMessageAsync(message);

// create a receiver that we can use to receive the message
ServiceBusReceiver receiver = client.CreateReceiver(queueName);

// the received message is a different type as it contains some service set properties
ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

```

## Step 4: Update the Service Bus Namespace and Service Bus Queue names
In the appsettings.json file, change the values of parameters NamespaceConnectionString and Queue to the ones you just created.


## Step 5: Run the application on your local development machine
When running your sample, it will need to understand how to connect and authorize with it. The easiest means for doing so is to use a connection string, which is created automatically when creating a Service Bus namespace. For local development, the connectionString  will use **Visual Studio**, **Azure CLI**, or **Active Directory Integrated Authentication** to authenticate to Azure AD to get a token. That token will be used to both send and receive data from your Service Bus Queue.

Visual Studio authentication will work if the following conditions are met:
 1. You have installed [Visual Studio 2017 v15.6](https://blogs.msdn.microsoft.com/visualstudio/2017/12/07/visual-studio-2017-version-15-6-preview/) or later.
 2. You are signed in to Visual Studio and have selected an account to use for local development. Use Tools > Options > Azure Service Authentication to choose a local development account. 

Azure CLI will work if the following conditions are met:
 1. You have [Azure CLI 2.0](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) installed. Version 2.0.12 supports the get-access-token option used by AzureServiceTokenProvider. If you have an earlier version, please upgrade. 
 2. You are logged into Azure CLI. You can login using **az login** command.
 
Azure Active Directory authentication will only work if the following conditions are met:
 1. Your on-premise active directory is synced with Azure AD. 
 2. You are running this code on a domain joined machine.   

Since your developer account has access to the Service Bus Namespace, you should be able to send and receive data on your Service Bus Queue using the web app's interface.

You can also use a service principal to run the application on your local development machine. See the section "Running the application using a service principal" in the [documentation for the AppAuthentication library](https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication#running-the-application-using-a-service-principal) for how to do this.
>Note: It is recommended to use your developer context for local development, since you do not need to create or share a service principal for that. If that does not work for you, you can use a service principal, but do not check in the certificate or secret in source repos, and share them securely.

## Step 6: Grant App Service MSI access to the Service Bus Namespace
Follow the steps in Step 2 used to grant yourself "Contributor" access to the Service Bus Namespace. However, instead of granting "Contributor" access to yourself, instead grant "Contributor" access to the App Service's MSI. To find the App Service's MSI in the "Select" field when adding a new role to the Service Bus Namespace, type in the name of the app service you used when deploying in Step 1.

## Step 7: Deploy the Web App to Azure
Use any of the methods outlined on [Deploy your app to Azure App Service](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-deploy) to publish the Web App to Azure. 
After you deploy it, browse to the web app. You should be able to send and receive data on your Service Bus Namespace on the web page as you did when you locally deployed the web app in Step 5. 

## Summary
The web app was successfully able to send and receive data on your Service Bus Namespace using your developer account during development, and using MSI when deployed to Azure, without any code change between local development environment and Azure. 
As a result, you did not have to explicitly create and handle a Service Bus shared access key to authenticate to and call Service Bus. You do not have to worry about renewing the MSI's credentials either, since MSI takes care of that.  

## Note

This sample uses the [Azure.Messaging.ServiceBus](https://www.nuget.org/packages/Azure.Messaging.ServiceBus) package. The same sample using the legacy package Microsoft.Azure.ServiceBus can be found at [Azure-Samples/app-service-msi-servicebus-dotnet at 03be4e05b5803e464d416b66fd729d23bd4220fb (github.com)](https://github.com/Azure-Samples/app-service-msi-servicebus-dotnet/tree/03be4e05b5803e464d416b66fd729d23bd4220fb)

