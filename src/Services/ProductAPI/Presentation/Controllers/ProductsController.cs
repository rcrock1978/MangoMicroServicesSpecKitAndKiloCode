using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Presentation.Controllers;

/// <summary>
/// Products controller
/// </summary>
[ApiController]
[Route("api/v1/products")]
[ApiVersion("1.0")]
public class ProductsController : ControllerBase
{
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ILogger<ProductsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetProducts()
    {
        _logger.LogInformation("GetProducts called");
        return Ok(new { message = "Products endpoint - implementation pending", products = new[] { } });
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetProduct(Guid id)
    {
        _logger.LogInformation("GetProduct called with ID: {Id}", id);
        return Ok(new { id, name = "Sample Product", price = 99.99m });
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "ProductAPI" });
    }
}
