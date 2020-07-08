using System.Threading.Tasks;

namespace Memstate
{
    public interface IStorageProvider
    {
        /// <summary>
        /// Initialize the storage for use 
        /// </summary>
        /// <returns></returns>
        Task Provision();
        
        IJournalReader CreateJournalReader();

        IJournalWriter CreateJournalWriter();
    }
}