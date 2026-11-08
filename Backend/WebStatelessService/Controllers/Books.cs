using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace WebStatelessService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Books : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Book book)
        {
            // Validate the book object
            if (book == null || string.IsNullOrEmpty(book.Title) || string.IsNullOrEmpty(book.Author) || book.Quantity < 0)
            {
                return BadRequest("Invalid book data.");
            }

            var serviceUri = new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService");

            var proxy = ServiceProxy.Create<ILibraryService>(serviceUri, new ServicePartitionKey(PartitionKeyHelper.ForBook(book.Id)));

            var result = await proxy.AddBook(book);

            if (!result)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to add the book.");
            }

            return Ok();
        }

        //[HttpPost("{bookId}/reservations")]
        //public async Task<IActionResult> Reservation(string bookId,[FromBody] ReservationDTO reservation)
        //{
        //    if(reservation == null || reservation.Quantity <= 0)
        //    {
        //        return BadRequest("Invalid reservation data.");
        //    }

        //    var serviceUri = new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService");

        //    var proxy = ServiceProxy.Create<ILibraryService>(serviceUri, new ServicePartitionKey(PartitionKeyHelper.ForBook(bookId)));

        //    var newGuid = Guid.NewGuid();

        //    var reservationResult = await proxy.ReserveAsync(newGuid, bookId, reservation.Quantity);

        //    if (reservationResult == 0)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to reserve the book.");
        //    }

        //    return Ok(reservationResult);

        //}

        [HttpGet("{bookId}/consistency")]
        public async Task<IActionResult> ConsistencyCheck(string bookId)
        {
            var serviceUri = new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService");
            var proxy = ServiceProxy.Create<ILibraryService>(serviceUri, new ServicePartitionKey(PartitionKeyHelper.ForBook(bookId)));

            var consistencyResult = await proxy.ConsistencyCheck(bookId);

            if(consistencyResult.IsConsistent)
            {
                return Ok(consistencyResult);
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, consistencyResult);
            }
        }

        [HttpGet("{bookId}")]
        public async Task<IActionResult> Get(string bookId)
        {
            var serviceUri = new Uri("fabric:/ProjekatVezbeWeb/LibraryStatefulService");
            var proxy = ServiceProxy.Create<ILibraryService>(serviceUri, new ServicePartitionKey(PartitionKeyHelper.ForBook(bookId)));
            var book = await proxy.GetBook(bookId);
            if (book == null)
            {
                return NotFound();
            }
            return Ok(book);
        }
    }
}
