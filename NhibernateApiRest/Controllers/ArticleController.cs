using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NhibernateApiRest.Core.IConfiguration;
using System;
using System.Threading.Tasks;

namespace NhibernateApiRest.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ArticleController : ControllerBase
	{
        private readonly ILogger<ArticleController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ArticleController(
            ILogger<ArticleController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var articles = await _unitOfWork.Articles.All();
            return Ok(articles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetItem(int id)
        {
            try
            {
                var item = await _unitOfWork.Articles.GetById(id);
                if (item == null)
                {
                    return NotFound();
                }

                return Ok(item);
            }
            catch(Exception)
			{
                return StatusCode(500);
            }
        }

    }
}
