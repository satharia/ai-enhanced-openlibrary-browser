using GLB.TechnicalTest.CBTW.BookSearch.Application.Common.Messaging;
using GLB.TechnicalTest.CBTW.BookSearch.Application.Specifics.Authors.Queries;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace GLB.TechnicalTest.CBTW.BookSearch.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController(ISender sender) : ControllerBase
    {
        // authorId is an Open Library author id, eg. OL26320A
        [HttpGet(@"{authorId:regex(^OL\d+A$)}/works")]
        public async Task<ActionResult<AuthorWorksResult>> GetWorks(
            string authorId, [FromQuery, Range(1, 1000)] int page = 1,
            [FromQuery, Range(1, 100)] int pageSize = 20, CancellationToken cancellationToken = default)
        {
            var result = await sender.Send(new GetAuthorWorksQuery(authorId, page, pageSize), cancellationToken);
            if (result is null)
                return NotFound();

            return result;
        }
    }
}
