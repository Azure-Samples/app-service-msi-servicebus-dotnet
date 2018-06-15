using System.ComponentModel.DataAnnotations;

namespace WebAppServiceBus.Models
{
    public class ServiceBusMessageData
    {
        public string MessageToSend { get; set; }

        public string MessagesReceived { get; set; }
    }
}