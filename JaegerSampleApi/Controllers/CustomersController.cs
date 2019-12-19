using CustomersApi.DataStore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Linq;

namespace CustomersApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerDbContext _dbContext;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(CustomerDbContext dbContext, ILogger<CustomersController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok(_dbContext.Customers.ToList());
        }

        [HttpGet("{id:int}")]
        public IActionResult Index(int id)
        {
            var customer = _dbContext.Customers.FirstOrDefault(x => x.CustomerId == id);

            if (customer == null)
                return NotFound();

            _logger.LogInformation("Returning data for customer {CustomerId}", id);

            return Ok(customer);
        }
    }
}
