using Azure.Messaging.ServiceBus;
using System.Collections.Generic;
using System;
using Azure.Identity;
using System.Threading.Tasks;
using WebAppServiceBus.Models;

namespace WebAppServiceBus
{
    public static class ReceivedMessageStore
    {
        private static List<string> _receivedMessages = new List<string>();

        public static async Task InitializeAsync(ServiceBusConfiguration config)
        {
            var options = new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,

                MaxConcurrentCalls = 2
            };

            ServiceBusClient client = new ServiceBusClient(config.Namespace, new DefaultAzureCredential());
            ServiceBusProcessor processor = client.CreateProcessor(config.Queue, options);

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            async Task MessageHandler(ProcessMessageEventArgs args)
            {
                string body = args.Message.Body.ToString();
                Console.WriteLine(body);
                _receivedMessages.Add(body);

                // we can evaluate application logic and use that to determine how to settle the message.
                await args.CompleteMessageAsync(args.Message);
            }

            Task ErrorHandler(ProcessErrorEventArgs args)
            {
                // the error source tells me at what point in the processing an error occurred
                Console.WriteLine(args.ErrorSource);
                // the fully qualified namespace is available
                Console.WriteLine(args.FullyQualifiedNamespace);
                // as well as the entity path
                Console.WriteLine(args.EntityPath);
                Console.WriteLine(args.Exception.ToString());
                return Task.CompletedTask;
            }
            await processor.StartProcessingAsync();
            Console.ReadKey();
        }

        public static List<string> GetReceivedMessages()
        {
            List<string> messages = new List<string>(_receivedMessages);
            _receivedMessages.Clear();
            return messages;
        }
    }
}
