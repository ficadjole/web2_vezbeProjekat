using Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebStatelessService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        [HttpPost]
        public IActionResult Post([FromBody] BuyBookRequestDTO buyBookRequestDTO )
        {
            // Here you can add logic to process the incoming value
            // For demonstration, we will just return the received value

            var proxy = ServiceProxy.Create<IValidator>(new Uri("fabric:/ProjekatVezbeWeb/ValidatorStatelessService"));

            try
            {

                var validationResult = proxy.ValidateAsync(buyBookRequestDTO).GetAwaiter().GetResult();
                if (!validationResult.IsValid)
                {
                    return BadRequest(new { Message = "Validation failed", Errors = validationResult.Message });
                }

            }
            catch (Exception ex)
            {
                // Handle any exceptions that may occur during service communication
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error communicating with ValidatorService: {ex.Message}");
            }

            return Ok(new { ReceivedValue = buyBookRequestDTO });

        }
    }
}
