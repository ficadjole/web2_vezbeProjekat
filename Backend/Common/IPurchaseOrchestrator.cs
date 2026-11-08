using Microsoft.ServiceFabric.Services.Remoting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public interface IPurchaseOrchestrator : IService
    {
        Task<Guid> StartPurchaseAsync(Guid purchaseId, string userId, string bookId, int quantity);

        Task<bool> AddCancelRequest(Guid purchaseId);

        Task<PurchaseResponseDTO> GetPurchaseAsync(Guid purchaseId);
    }
}
