using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Abstractions
{
    /// <summary>
    /// Following the repository design pattern which isolates data access behind interface abstractions
    /// </summary>
    /// <typeparam name="T">Entity that hold the data</typeparam>
    public interface IRepository<T> where T : BaseModel
    {
        Task<T> GetByIdAsync(string id);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }
}
