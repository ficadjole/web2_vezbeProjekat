using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface ILibraryService : IService
    {

        Task<Book> BookAvailableAsync(string id,int quantity);
        Task<long> GetCountAsync();

        Task<bool> AddBook(Book book);

        Task<List<Book>> GetAllBooksAsync();

        Task<decimal> ReserveAsync(Guid txId, string bookId, int quantity);

        Task<ConsistencyCheckDTO> ConsistencyCheck(string bookId);
    }
}
