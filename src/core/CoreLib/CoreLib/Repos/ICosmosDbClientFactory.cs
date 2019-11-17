using System.Threading.Tasks;

namespace CoreLib.Repos
{
    public interface ICosmosDbClientFactory
    {
        ICosmosDbClient GetClient(string collectionName);
        Task EnsureDbSetupAsync();
    }
}