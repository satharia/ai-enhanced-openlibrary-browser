using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GLB.TechnicalTest.CBTW.BookSearch.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(ISender sender) : ControllerBase
    {
        [HttpGet("search")]
        public async Task<SearchBookResult> Search(
            [FromQuery] string searchText, [FromQuery, Range(1, 100)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            return await sender.Send(new SearchBookQuery(searchText, page, pageSize), cancellationToken);
        }

        // workId is an Open Library work id, eg. OL27448W
        [HttpGet(@"{workId:regex(^OL\d+W$)}/editions")]
        public async Task<ActionResult<BookEditionsResult>> GetEditions(
            string workId, [FromQuery, Range(1, 1000)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(new GetBookEditionsQuery(workId, page, pageSize), cancellationToken);
            if (result is null)
                return NotFound();

            return result;
        }
    }
}
