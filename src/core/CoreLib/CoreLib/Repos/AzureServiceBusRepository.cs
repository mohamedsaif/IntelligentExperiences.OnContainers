using CoreLib.Abstractions;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreLib.Repos
{
    public class AzureServiceBusRepository : IDisposable, IServiceBusRepository<ISubscriptionClient, MessageHandlerOptions, Message>
    {
        ITopicClient topicClient;
        ISubscriptionClient subscriptionClient;

        public ISubscriptionClient SubscriptionClient
        {
            get
            {
                return subscriptionClient;
            }
        }
        public AzureServiceBusRepository(string connectionString, string topicName, string subscriptionName)
        {
            topicClient = new TopicClient(connectionString, topicName);
            if (!string.IsNullOrEmpty(subscriptionName))
                subscriptionClient = new SubscriptionClient(connectionString, topicName, subscriptionName);
        }

        public async Task<bool> PublishMessage(Message message)
        {
            // Create a message and add it to the queue.
            await topicClient.SendAsync(message);
            return true;
        }

        /// <summary>
        /// Register a method to be called when a new message(s) is published to the subscribed topic
        /// </summary>
        /// <param name="handler">
        /// An async function that takes a Message and a CancelationToken to process received message.
        /// Don't forget to complete the message so it will not be received again (in ReceiveMode.PeekLock mode) via calling subscriptionClient.CompleteAsync
        /// </param>
        /// <param name="options">Set the default behavior of the subscription client and defines an exception call back function</param>
        public void RegisterMessageHandler(Func<Message, CancellationToken, Task> handler, MessageHandlerOptions options)
        {
            subscriptionClient.RegisterMessageHandler(handler, options);
        }

        public void Dispose()
        {
            try
            {
                if (topicClient != null)
                    topicClient.CloseAsync();
            }
            catch
            {

            }

            try
            {
                if (subscriptionClient != null)
                    subscriptionClient.CloseAsync();
            }
            catch
            {

            }
        }
    }
}
