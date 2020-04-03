using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoreLib.Abstractions
{
    /// <summary>
    /// Abstraction for handling pub/sub service bus operations
    /// </summary>
    /// <typeparam name="T">Subscription Client</typeparam>
    /// <typeparam name="U">Message Handling Options</typeparam>
    /// <typeparam name="M">Message Type</typeparam>
    public interface IServiceBusRepository<T,U,M>
    {
        T SubscriptionClient { get; }

        Task<bool> PublishMessage(M message);
        void RegisterMessageHandler(Func<M, CancellationToken, Task> handler, U options);
    }
}
