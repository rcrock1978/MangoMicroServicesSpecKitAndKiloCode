using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.ProductAPI.Application.Commands;
using Mango.Services.ProductAPI.Application.DTOs;
using Mango.Services.ProductAPI.Application.Queries;

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
    private readonly IMediator _mediator;

    public ProductsController(ILogger<ProductsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Get all products with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? categoryName = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("GetProducts called with searchTerm: {SearchTerm}, category: {Category}, page: {Page}", 
            searchTerm, categoryName, page);
        
        var query = new GetProductsQuery(searchTerm, categoryName, page, pageSize);
        var result = await _mediator.Send(query);
        
        return Ok(result);
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        _logger.LogInformation("GetProduct called with ID: {Id}", id);
        
        var query = new GetProductByIdQuery(id);
        var product = await _mediator.Send(query);
        
        if (product == null)
            return NotFound(new { message = "Product not found" });
        
        return Ok(product);
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        _logger.LogInformation("CreateProduct called with name: {Name}", request.Name);
        
        var command = new CreateProductCommand(
            request.Name,
            request.Description,
            request.Price,
            request.CategoryName,
            request.ImageUrl,
            request.StockQuantity
        );
        
        var product = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
    {
        _logger.LogInformation("UpdateProduct called with ID: {Id}", id);
        
        if (id != request.Id)
            return BadRequest(new { message = "ID mismatch" });
        
        var command = new UpdateProductCommand(
            request.Id,
            request.Name,
            request.Description,
            request.Price,
            request.CategoryName,
            request.ImageUrl,
            request.StockQuantity,
            request.IsActive
        );
        
        var product = await _mediator.Send(command);
        
        if (product == null)
            return NotFound(new { message = "Product not found" });
        
        return Ok(product);
    }

    /// <summary>
    /// Delete a product
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        _logger.LogInformation("DeleteProduct called with ID: {Id}", id);
        
        var command = new DeleteProductCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound(new { message = "Product not found" });
        
        return Ok(new { message = "Product deleted successfully" });
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
