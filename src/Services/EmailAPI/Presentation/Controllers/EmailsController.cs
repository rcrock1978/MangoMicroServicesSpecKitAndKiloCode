using Microsoft.AspNetCore.Mvc;
using MediatR;
using Mango.Services.EmailAPI.Application.Commands;
using Mango.Services.EmailAPI.Application.Queries;
using Mango.Services.EmailAPI.Application.DTOs;

namespace Mango.Services.EmailAPI.Presentation.Controllers;

/// <summary>
/// Email API Controller
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[ApiVersion("1.0")]
public class EmailsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmailsController> _logger;

    public EmailsController(IMediator mediator, ILogger<EmailsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region Send Email

    /// <summary>
    /// Send a plain email
    /// </summary>
    [HttpPost("send")]
    [ProducesResponseType(typeof(EmailLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmailLogDto>> SendEmail([FromBody] SendEmailRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new SendEmailCommand(
            request.ToEmail,
            request.CcEmail,
            request.Subject,
            request.Body,
            request.EmailType
        );

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Send a templated email
    /// </summary>
    [HttpPost("send-templated")]
    [ProducesResponseType(typeof(EmailLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailLogDto>> SendTemplatedEmail([FromBody] SendTemplatedEmailRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var command = new SendTemplatedEmailCommand(
                request.ToEmail,
                request.CcEmail,
                request.EmailType,
                request.Placeholders
            );

            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    #endregion

    #region Email Templates

    /// <summary>
    /// Get all email templates
    /// </summary>
    [HttpGet("templates")]
    [ProducesResponseType(typeof(IEnumerable<EmailTemplateDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmailTemplateDto>>> GetAllTemplates()
    {
        var query = new GetAllEmailTemplatesQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get email template by ID
    /// </summary>
    [HttpGet("templates/{id:guid}")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailTemplateDto>> GetTemplateById(Guid id)
    {
        var query = new GetEmailTemplateByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Template with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get email template by type
    /// </summary>
    [HttpGet("templates/type/{emailType:int}")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailTemplateDto>> GetTemplateByType(int emailType)
    {
        var query = new GetEmailTemplateByTypeQuery((EmailType)emailType);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Template with type {emailType} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new email template
    /// </summary>
    [HttpPost("templates")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmailTemplateDto>> CreateTemplate([FromBody] CreateEmailTemplateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var command = new CreateEmailTemplateCommand(
            request.Name,
            request.Subject,
            request.Body,
            request.EmailType
        );

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTemplateById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Delete an email template
    /// </summary>
    [HttpDelete("templates/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTemplate(Guid id)
    {
        var command = new DeleteEmailTemplateCommand(id);
        var result = await _mediator.Send(command);

        if (!result)
        {
            return NotFound(new { message = $"Template with ID {id} not found" });
        }

        return NoContent();
    }

    #endregion

    #region Email Logs

    /// <summary>
    /// Get email logs with pagination
    /// </summary>
    [HttpGet("logs")]
    [ProducesResponseType(typeof(IEnumerable<EmailLogDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmailLogDto>>> GetLogs(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? emailType = null,
        [FromQuery] int? status = null)
    {
        var query = new GetEmailLogsQuery(
            pageNumber,
            pageSize,
            emailType.HasValue ? (EmailType)emailType.Value : null,
            status.HasValue ? (EmailStatus)status.Value : null
        );

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get email log by ID
    /// </summary>
    [HttpGet("logs/{id:guid}")]
    [ProducesResponseType(typeof(EmailLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmailLogDto>> GetLogById(Guid id)
    {
        var query = new GetEmailLogByIdQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
        {
            return NotFound(new { message = $"Email log with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get email logs count
    /// </summary>
    [HttpGet("logs/count")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetLogsCount(
        [FromQuery] int? emailType = null,
        [FromQuery] int? status = null)
    {
        var query = new GetEmailLogsCountQuery(
            emailType.HasValue ? (EmailType)emailType.Value : null,
            status.HasValue ? (EmailStatus)status.Value : null
        );

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    #endregion

    #region Health Check

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", service = "EmailAPI", timestamp = DateTime.UtcNow });
    }

    #endregion
}
