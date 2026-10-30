using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebStatelessService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticsController : ControllerBase
    {
        [HttpGet("/partitions")]
        public IActionResult Get()
        {
            var proxy = ServiceProxy.Create<IValidator>(new Uri("fabric:/ProjekatVezbeWeb/ValidatorStatelessService"));

            var result = proxy.DiagnosticAsync().GetAwaiter().GetResult();

            return Ok(new { result });


        }
    }
}
