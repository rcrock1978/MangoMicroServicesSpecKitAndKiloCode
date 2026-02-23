using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.CouponAPI.Application.Commands;
using Mango.Services.CouponAPI.Application.DTOs;
using Mango.Services.CouponAPI.Application.Queries;
using Mango.Services.CouponAPI.Domain;

namespace Mango.Services.CouponAPI.Presentation.Controllers;

/// <summary>
/// Coupons controller
/// </summary>
[ApiController]
[Route("api/v1/coupons")]
[ApiVersion("1.0")]
public class CouponsController : ControllerBase
{
    private readonly ILogger<CouponsController> _logger;
    private readonly IMediator _mediator;

    public CouponsController(ILogger<CouponsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Get all coupons
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCoupons()
    {
        _logger.LogInformation("GetCoupons called");
        
        var query = new GetCouponsQuery();
        var coupons = await _mediator.Send(query);
        
        return Ok(coupons);
    }

    /// <summary>
    /// Get coupon by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCoupon(Guid id)
    {
        _logger.LogInformation("GetCoupon called with ID: {Id}", id);
        
        var query = new GetCouponByIdQuery(id);
        var coupon = await _mediator.Send(query);
        
        if (coupon == null)
            return NotFound(new { message = "Coupon not found" });
        
        return Ok(coupon);
    }

    /// <summary>
    /// Get coupon by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCouponByCode(string code)
    {
        _logger.LogInformation("GetCouponByCode called with code: {Code}", code);
        
        var query = new GetCouponByCodeQuery(code);
        var coupon = await _mediator.Send(query);
        
        if (coupon == null)
            return NotFound(new { message = "Coupon not found" });
        
        return Ok(coupon);
    }

    /// <summary>
    /// Validate coupon
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateCoupon([FromBody] string code, [FromQuery] decimal orderAmount)
    {
        _logger.LogInformation("ValidateCoupon called with code: {Code}, orderAmount: {OrderAmount}", code, orderAmount);
        
        var command = new ValidateCouponCommand(code, orderAmount);
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new coupon
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponRequest request)
    {
        _logger.LogInformation("CreateCoupon called with code: {Code}", request.Code);
        
        var command = new CreateCouponCommand(
            request.Code,
            request.Description,
            request.CouponType,
            request.DiscountAmount,
            request.DiscountPercentage,
            request.MinOrderAmount,
            request.MaxUsageCount,
            request.ValidFrom,
            request.ValidUntil
        );
        
        var coupon = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetCoupon), new { id = coupon.Id }, coupon);
    }

    /// <summary>
    /// Update an existing coupon
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCoupon(Guid id, [FromBody] UpdateCouponRequest request)
    {
        _logger.LogInformation("UpdateCoupon called with ID: {Id}", id);
        
        if (id != request.Id)
            return BadRequest(new { message = "ID mismatch" });
        
        var command = new UpdateCouponCommand(
            request.Id,
            request.Code,
            request.Description,
            request.CouponType,
            request.DiscountAmount,
            request.DiscountPercentage,
            request.MinOrderAmount,
            request.MaxUsageCount,
            request.ValidFrom,
            request.ValidUntil,
            request.IsActive
        );
        
        var coupon = await _mediator.Send(command);
        
        if (coupon == null)
            return NotFound(new { message = "Coupon not found" });
        
        return Ok(coupon);
    }

    /// <summary>
    /// Delete a coupon
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCoupon(Guid id)
    {
        _logger.LogInformation("DeleteCoupon called with ID: {Id}", id);
        
        var command = new DeleteCouponCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound(new { message = "Coupon not found" });
        
        return Ok(new { message = "Coupon deleted successfully" });
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "CouponAPI" });
    }
}
