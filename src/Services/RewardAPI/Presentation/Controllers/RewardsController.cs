using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.RewardAPI.Application.Commands;
using Mango.Services.RewardAPI.Application.DTOs;
using Mango.Services.RewardAPI.Application.Queries;

namespace Mango.Services.RewardAPI.Presentation.Controllers;

/// <summary>
/// Rewards controller
/// </summary>
[ApiController]
[Route("api/v1/rewards")]
[ApiVersion("1.0")]
public class RewardsController : ControllerBase
{
    private readonly ILogger<RewardsController> _logger;
    private readonly IMediator _mediator;

    public RewardsController(ILogger<RewardsController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Get user reward by user ID
    /// </summary>
    [HttpGet("user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserReward(string userId)
    {
        _logger.LogInformation("GetUserReward called for user: {UserId}", userId);
        
        var query = new GetUserRewardByUserIdQuery(userId);
        var reward = await _mediator.Send(query);
        
        if (reward == null)
            return NotFound(new { message = "User reward not found" });
        
        return Ok(reward);
    }

    /// <summary>
    /// Get all rewards
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRewards()
    {
        _logger.LogInformation("GetRewards called");
        
        var query = new GetRewardsQuery();
        var rewards = await _mediator.Send(query);
        
        return Ok(rewards);
    }

    /// <summary>
    /// Get reward by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReward(Guid id)
    {
        _logger.LogInformation("GetReward called with ID: {Id}", id);
        
        var query = new GetRewardByIdQuery(id);
        var reward = await _mediator.Send(query);
        
        if (reward == null)
            return NotFound(new { message = "Reward not found" });
        
        return Ok(reward);
    }

    /// <summary>
    /// Earn points
    /// </summary>
    [HttpPost("earn")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> EarnPoints([FromBody] EarnPointsRequest request)
    {
        _logger.LogInformation("EarnPoints called for user: {UserId}, points: {Points}", 
            request.UserId, request.Points);
        
        var command = new EarnPointsCommand(
            request.UserId,
            request.Points,
            request.Description,
            request.OrderId
        );
        
        var result = await _mediator.Send(command);
        
        return Ok(result);
    }

    /// <summary>
    /// Redeem points
    /// </summary>
    [HttpPost("redeem")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsRequest request)
    {
        _logger.LogInformation("RedeemPoints called for user: {UserId}, points: {Points}", 
            request.UserId, request.Points);
        
        var command = new RedeemPointsCommand(
            request.UserId,
            request.Points,
            request.Description,
            request.RewardId
        );
        
        var result = await _mediator.Send(command);
        
        if (result == null)
            return BadRequest(new { message = "Insufficient points or user not found" });
        
        return Ok(result);
    }

    /// <summary>
    /// Create a new reward
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateReward([FromBody] CreateRewardRequest request)
    {
        _logger.LogInformation("CreateReward called with name: {Name}", request.Name);
        
        var command = new CreateRewardCommand(
            request.Name,
            request.Description,
            request.PointsRequired,
            request.ImageUrl,
            request.MaxAvailable
        );
        
        var reward = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetReward), new { id = reward.Id }, reward);
    }

    /// <summary>
    /// Delete a reward
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReward(Guid id)
    {
        _logger.LogInformation("DeleteReward called with ID: {Id}", id);
        
        var command = new DeleteRewardCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result)
            return NotFound(new { message = "Reward not found" });
        
        return Ok(new { message = "Reward deleted successfully" });
    }

    /// <summary>
    /// Health check
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "RewardAPI" });
    }
}
