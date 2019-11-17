using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Abstractions
{
    public interface IQueueRepository<T>
    {
        Task<bool> QueueMessage(T message);
        Task<T> GetMessage();
        Task<bool> DeleteMessage(T message);
        Task<int> GetQueueLength();
    }
}
