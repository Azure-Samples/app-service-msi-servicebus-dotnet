using Azure.Messaging.ServiceBus;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAppServiceBus.Models;

namespace WebAppServiceBus
{
    public static class ReceivedMessageStore
    {
        private static List<string> _receivedMessages = new List<string>();

        public static async Task InitializeAsync(ServiceBusConfiguration config, ServiceBusClient client)
        {
            ServiceBusProcessor processor = client.CreateProcessor(config.Queue);

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            Task MessageHandler(ProcessMessageEventArgs args)
            {
                _receivedMessages.Add($"MessageId:{args.Message.MessageId}, Seq#:{args.Message.SequenceNumber}, data:{args.Message.Body}");
                return Task.CompletedTask;
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                _receivedMessages.Add($"Exception: \"{args.Exception.Message}\" {args.EntityPath}");
                return Task.CompletedTask;
            }

            await processor.StartProcessingAsync();
        }

        public static List<string> GetReceivedMessages()
        {
            List<string> messages = new List<string>(_receivedMessages);
            _receivedMessages.Clear();
            return messages;
        }
    }
}
