using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace message_sender
{
    /// <summary>
    /// We will want this to start sending messages at regular intervals with changing content to be able to validate the flow
    /// https://github.com/Azure/azure-sdk-for-net/tree/master/sdk/servicebus/Azure.Messaging.ServiceBus
    /// </summary>
    class POCServiceBusClient : IDisposable
    {
        private ServiceBusClient client;
        private ServiceBusSender sender = null;
        private string senderQueueName = "default";
        private static object locker = "locked";
        private bool disposedValue;

        public POCServiceBusClient(string connectionString, string queueName)
        {
            client = new ServiceBusClient(connectionString);
            senderQueueName = queueName;
            getSender(senderQueueName).Wait();
        }

        private async Task getSender(string queueName)
        {
            if (sender == null || sender.IsClosed)
            {
                lock (locker)
                {
                    senderQueueName = queueName;
                    sender = client.CreateSender(senderQueueName);
                }
            }
            else if (senderQueueName != queueName)
            {
                try
                {
                    await sender.CloseAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error Closing Sender");
                    Console.WriteLine(e.Message);
                }

                lock (locker)
                {
                    senderQueueName = queueName;
                }

                sender = client.CreateSender(senderQueueName);
            }
        }

        public void SendMessage(string messageContent)
        {
            if (sender == null || sender.IsClosed)
            {
                getSender(senderQueueName).Wait();
            }
                
            // create a message that we can send. UTF-8 encoding is used when providing a string.
            ServiceBusMessage message = new ServiceBusMessage(messageContent);
            message.ContentType = "application/json";
            message.ApplicationProperties.Add("appid", "mdm" );
            // send the message
            try
            {
                sender.SendMessageAsync(message).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    this.client.DisposeAsync();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~POCServiceBusClient()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        void IDisposable.Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
