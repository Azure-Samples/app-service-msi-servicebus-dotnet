using Microsoft.AspNetCore.Mvc;
using Azure.Messaging.ServiceBus;
using Azure.Identity;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using WebAppServiceBus.Models;

namespace WebAppServiceBus.Controllers
{
    public class HomeController : Controller
    {
        public ServiceBusConfiguration Config { get; }
        public ServiceBusClient client;
        private readonly Task _startProcessingTask;

        public HomeController(IOptions<ServiceBusConfiguration> serviceBusConfig)
        {
            Config = serviceBusConfig.Value;
            client = new ServiceBusClient(Config.Namespace, new DefaultAzureCredential());
            _startProcessingTask = ReceivedMessageStore.InitializeAsync(Config, client);
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
            
            ServiceBusSender sender = client.CreateSender(Config.Queue);
            ServiceBusMessage message = new ServiceBusMessage(messageInfo.MessageToSend);
            await sender.SendMessageAsync(message).ConfigureAwait(false);
            await sender.CloseAsync().ConfigureAwait(false);
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Receive()
        {
            await _startProcessingTask.ConfigureAwait(false);

            ServiceBusMessageData messageInfo = new ServiceBusMessageData();

            List<string> receivedMessages = ReceivedMessageStore.GetReceivedMessages();
            if (receivedMessages.Count > 0)
            {
                messageInfo.MessagesReceived = string.Join(Environment.NewLine, receivedMessages);
            }
            else
            {
                messageInfo.MessagesReceived = "No messages from queue received yet!";
            }

            return View("Index", messageInfo);
        }
    }
}
