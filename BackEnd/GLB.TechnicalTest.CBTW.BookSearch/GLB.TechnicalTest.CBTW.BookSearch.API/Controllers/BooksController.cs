using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Books.Queries;
using Microsoft.AspNetCore.Mvc;

namespace GLB.TechnicalTest.CBTW.BookSearch.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController(ISender sender) : ControllerBase
    {
        [HttpGet("search")]
        public async Task<SearchBookResult> Search(
            [FromQuery] string searchText, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            return await sender.Send(new SearchBookQuery(searchText, page, pageSize), cancellationToken);
        }
    }
}
