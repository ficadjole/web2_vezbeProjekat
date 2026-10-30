using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebStatelessService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Accounts : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] BankAccount account)
        {
            // Validate the account object
            if (account == null || string.IsNullOrEmpty(account.Owner) || account.Balance < 0)
            {
                return BadRequest("Invalid account data.");
            }
            var serviceUri = new Uri("fabric:/ProjekatVezbeWeb/BankStatefulService");
            var proxy = ServiceProxy.Create<IBankService>(serviceUri, new ServicePartitionKey(PartitionKeyHelper.ForAccount(account.Id)));
            var result = await proxy.AddAccount(account);
            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to add the account.");
            }
            return Ok();
        }
    }
}
