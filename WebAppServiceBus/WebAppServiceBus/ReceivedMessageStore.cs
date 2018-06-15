using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WebAppServiceBus.Models;

namespace WebAppServiceBus
{
    public static class ReceivedMessageStore
    {
        private static QueueClient _receiveClient = null;
        private static List<string> _receivedMessages = new List<string>();

        public static void Initialize(ServiceBusConfiguration config)
        {
            if (_receiveClient != null)
            {
                return;
            }

            TokenProvider tokenProvider = TokenProvider.CreateManagedServiceIdentityTokenProvider();

            _receiveClient = new QueueClient($"sb://{config.Namespace}.servicebus.windows.net/", config.Queue, tokenProvider, receiveMode: ReceiveMode.ReceiveAndDelete);
            _receiveClient.RegisterMessageHandler(
                (message, cancellationToken) =>
                {
                    _receivedMessages.Add($"MessageId:{message.MessageId}, Seq#:{message.SystemProperties.SequenceNumber}, data:{Encoding.UTF8.GetString(message.Body)}");
                    return Task.CompletedTask;
                },
                (exceptionEvent) =>
                {
                    _receivedMessages.Add($"Exception: \"{exceptionEvent.Exception.Message}\" {exceptionEvent.ExceptionReceivedContext.EntityPath}");
                    return Task.CompletedTask;
                });
        }

        public static List<string> GetReceivedMessages()
        {
            List<string> messages = new List<string>(_receivedMessages);
            _receivedMessages.Clear();
            return messages;
        }
    }
}
