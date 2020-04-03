using CoreLib.Abstractions;
using Microsoft.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Repos
{
    public interface IAzureServiceBusRepository : IServiceBusRepository<ISubscriptionClient, MessageHandlerOptions, Message>
    {
        
    }
}
