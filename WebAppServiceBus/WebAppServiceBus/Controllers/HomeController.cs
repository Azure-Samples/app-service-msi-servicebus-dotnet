using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using WebAppServiceBus.Models;

namespace WebAppServiceBus.Controllers
{
    public class HomeController : Controller
    {
        public ServiceBusConfiguration Config { get; }

        public HomeController(IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            Config = serviceBusConfig.Value;
            ReceivedMessageStore.Initialize(Config);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<ActionResult> Send(ServiceBusMessageData messageInfo)
        {
            if (string.IsNullOrEmpty(messageInfo.MessageToSend))
            {
                return RedirectToAction("Index");
            }
            string connectionString = Config.NamespaceConnectionString;
            string queueName = Config.Queue;
            var client = new ServiceBusClient(connectionString);
            ServiceBusSender sender = client.CreateSender(queueName);
            ServiceBusMessage message = new ServiceBusMessage(messageInfo.MessageToSend);
            await sender.SendMessageAsync(message);
            await sender.CloseAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Receive()
        {
            ServiceBusMessageData messageInfo = new ServiceBusMessageData();

            string connectionString = "Endpoint=sb://test0412.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Lp/kwQRPp38LuGf+dCqPXhn0vKQ1BAS+CZ6SDrw7bTs=";
            string queueName = "zedtestqueues2";
            var client = new ServiceBusClient(connectionString);
            ServiceBusReceiver receiver = client.CreateReceiver(queueName);
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

            if ( receivedMessage.Body.ToString() != null )
            {
                messageInfo.MessagesReceived = receivedMessage.Body.ToString();
            }
            else
            {
                messageInfo.MessagesReceived = "No messages from queue received yet!";
            }

            return View("Index", messageInfo);
        }
    }
}
