
using Microsoft.ServiceFabric.Services.Remoting;

namespace Common
{
    public interface IValidator : IService
    {
        Task<ValidationResultDto> ValidateAsync(BuyBookRequestDTO buyBookRequestDTO, Guid idempotencyKey);

        Task<Result<PurchaseResponseDTO>> GetPurchase(Guid idempotencyKey);
        Task<ValidationResultDto> PurchaseCencalltion(Guid idempotencyKey);
        Task<IEnumerable<DiagnosticResultDto>> DiagnosticAsync();

        Task<List<Book>> GetAllBooksAsync();
    
        Task<List<BankAccount>> GetAllAccountsAsync();
    

    }
}
