using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.Messaging.Replication;
using Microsoft.Azure.ServiceBus;
using System.Threading.Tasks;
using StackExchange.Redis;
using Newtonsoft.Json;

namespace adamos.shovel
{
    // Implements a naieve shovel
    // Strings concatenations of tenant names are used as keys in redis
    // presence of key means route is allowed.
    public static class shovel
    {
        [FunctionName("TenantRouter")]
        public static async Task TenantMdmOutToMdmIn(
            [ServiceBusTrigger("tenant_out", Connection = "tenant-out-connection")] Message[] input,
            ILogger log)
        {
            string tenantValue = Environment.GetEnvironmentVariable("tenant-name");
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {input[0]}");
            foreach (var message in input)
            {
                try
                {
                    object appId = "";

                    // sample uses "appid" set as a custom property on the Service Bus message
                    if (message.UserProperties.TryGetValue("appid", out appId))
                    {
                        if (IsRouteAllowed(tenantValue, appId.ToString(), log))
                        {
                            // ToDo: fetch route from keyvault rather than app settings
                            // this can be a SAS to be extra secure, or the appropriate connection string with Root key
                            string targetQueueConnectionString = Environment.GetEnvironmentVariable("targetQueueConnectionString");

                            await ForwardToTargetQueue(message, targetQueueConnectionString, log);

                        }
                        else
                        {
                            log.LogWarning("route not allowed");
                            log.LogWarning(tenantValue, appId.ToString());
                        }
                    }
                    else
                    {
                        // this can be used to debug the message processing
                        log.LogWarning("unable to find appid in message!");
                        log.LogWarning("Message:");
                        log.LogWarning(message.ToString());
                        log.LogWarning(message.ContentType);
                        log.LogWarning("User Properties on message:");
                        foreach (var key in message.UserProperties.Keys)
                        {
                            log.LogWarning(key.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    log.LogWarning(e.Message);
                    log.LogWarning(e.StackTrace);
                }

            }
            return;
        }

        // ToDo: as we might process 100s of message, this should be moved to a connection pool / singleton on the function
        private static bool IsRouteAllowed(string tenant, string targetAppId, ILogger log)
        {
            log.LogWarning("Checking route : " + tenant + targetAppId);

            var connString = Environment.GetEnvironmentVariable("redisConnectionString");
            var redis = ConnectionMultiplexer.Connect(connString);

            IDatabase db = redis.GetDatabase();

            return db.KeyExists(tenant + targetAppId);
        }

        // As this processes multiple messages, it would be better to use connection pooling
        private static async Task ForwardToTargetQueue(Message message, string connectionString, ILogger log, Func<Message, Message> factory = null)
        {

            var forwardedMessage = factory != null ? factory(message) : message.Clone();

            // Maybe need to return an exception
            if (forwardedMessage == null)
            {
                return;
            }

            forwardedMessage.UserProperties[Constants.ReplEnqueuedTimePropertyName] =
                (message.UserProperties.ContainsKey(Constants.ReplEnqueuedTimePropertyName)
                    ? message.UserProperties[Constants.ReplEnqueuedTimePropertyName] + ";"
                    : string.Empty) +
                message.SystemProperties.EnqueuedTimeUtc.ToString("u");
            forwardedMessage.UserProperties[Constants.ReplOffsetPropertyName] =
                (message.UserProperties.ContainsKey(Constants.ReplOffsetPropertyName)
                    ? message.UserProperties[Constants.ReplOffsetPropertyName] + ";"
                    : string.Empty) +
                message.SystemProperties.EnqueuedSequenceNumber.ToString();
            forwardedMessage.UserProperties[Constants.ReplSequencePropertyName] =
                (message.UserProperties.ContainsKey(Constants.ReplSequencePropertyName)
                    ? message.UserProperties[Constants.ReplSequencePropertyName] + ";"
                    : string.Empty) +
                message.SystemProperties.EnqueuedSequenceNumber.ToString();

            QueueClient client = new QueueClient(connectionString, "in");

            // send the message
            await client.SendAsync(forwardedMessage);
            Console.WriteLine($"Sent a single message to the queue: in");
            await client.CloseAsync();
        }
    }
}

