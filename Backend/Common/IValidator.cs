
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common
{
    public interface IValidator : IService
    {
        Task<ValidationResultDto> ValidateAsync(BuyBookRequestDTO buyBookRequestDTO);
        Task<IEnumerable<DiagnosticResultDto>> DiagnosticAsync();

        Task<List<Book>> GetAllBooksAsync();
        Task<List<BankAccount>> GetAllAccountsAsync();
    }
}
