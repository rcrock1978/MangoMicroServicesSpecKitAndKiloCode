using MediatR;
using Mango.Services.EmailAPI.Application.DTOs;
using Mango.Services.EmailAPI.Domain;

namespace Mango.Services.EmailAPI.Application.Commands;

/// <summary>
/// Send Email Command
/// </summary>
public record SendEmailCommand(
    string ToEmail,
    string? CcEmail,
    string Subject,
    string Body,
    EmailType EmailType
) : IRequest<EmailLogDto>;

/// <summary>
/// Send Templated Email Command
/// </summary>
public record SendTemplatedEmailCommand(
    string ToEmail,
    string? CcEmail,
    EmailType EmailType,
    Dictionary<string, string> Placeholders
) : IRequest<EmailLogDto>;

/// <summary>
/// Create Email Template Command
/// </summary>
public record CreateEmailTemplateCommand(
    string Name,
    string Subject,
    string Body,
    EmailType EmailType
) : IRequest<EmailTemplateDto>;

/// <summary>
/// Delete Email Template Command
/// </summary>
public record DeleteEmailTemplateCommand(Guid Id) : IRequest<bool>;
