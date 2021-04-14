using Azure.Messaging.ServiceBus;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebAppServiceBus.Models;

namespace WebAppServiceBus
{
    public static class ReceivedMessageStore
    {
        private static List<string> _receivedMessages = new List<string>();

        public static async void Initialize(ServiceBusConfiguration config)
        {
            ServiceBusReceiver receiver = new ServiceBusClient(config.NamespaceConnectionString).CreateReceiver(config.Queue);
            ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();
            _receivedMessages.Add(receivedMessage.Body.ToString());
        }

        public static List<string> GetReceivedMessages()
        {
            List<string> messages = new List<string>(_receivedMessages);
            _receivedMessages.Clear();
            return messages;
        }
    }
}
