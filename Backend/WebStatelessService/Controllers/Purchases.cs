using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebStatelessService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Purchases : ControllerBase
    {

        [HttpGet("{idempotencyKey}")]
        public async Task<IActionResult> GetPurchases(Guid idempotencyKey)
        {
            var proxy = ServiceProxy.Create<IValidator>(new Uri("fabric:/ProjekatVezbeWeb/ValidatorStatelessService"));

            var result = await proxy.GetPurchase(idempotencyKey);

            if (result.IsFailure)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Purchase not found." });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, new { Message = "Purchase found.", Data = result.Value });
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreatePurchase([FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,[FromBody] BuyBookRequestDTO buyBookRequestDTO)
        {
            var proxy = ServiceProxy.Create<IValidator>(new Uri("fabric:/ProjekatVezbeWeb/ValidatorStatelessService"));
       
            var result = await proxy.ValidateAsync(buyBookRequestDTO, idempotencyKey);

            if (!result.IsValid)
            {
                return BadRequest(new { Message = result.Message });
            }
            else
            {

                return StatusCode(StatusCodes.Status202Accepted, new { Message = "Purchase request accepted for processing." });
            }
        }

        [HttpDelete("/{idempotencyKey}")]
        public async Task<IActionResult> DeletePurchase(Guid idempotencyKey)
        {
            var proxy = ServiceProxy.Create<IValidator>(new Uri("fabric:/ProjekatVezbeWeb/ValidatorStatelessService"));


            var result = await proxy.PurchaseCencalltion(idempotencyKey);

            if (!result.IsValid) { 
            
                return StatusCode(StatusCodes.Status409Conflict, new { Message = "Past pivot" });

            }

            return StatusCode(StatusCodes.Status202Accepted, new { Message = "Purchase cancellation request accepted for processing." });
        }
    }
}
