using MediatR;
using Mango.Services.EmailAPI.Application.DTOs;
using Mango.Services.EmailAPI.Domain;

namespace Mango.Services.EmailAPI.Application.Queries;

/// <summary>
/// Get Email Template by ID Query
/// </summary>
public record GetEmailTemplateByIdQuery(Guid Id) : IRequest<EmailTemplateDto?>;

/// <summary>
/// Get Email Template by Type Query
/// </summary>
public record GetEmailTemplateByTypeQuery(EmailType EmailType) : IRequest<EmailTemplateDto?>;

/// <summary>
/// Get All Email Templates Query
/// </summary>
public record GetAllEmailTemplatesQuery() : IRequest<IEnumerable<EmailTemplateDto>>;

/// <summary>
/// Get Email Log by ID Query
/// </summary>
public record GetEmailLogByIdQuery(Guid Id) : IRequest<EmailLogDto?>;

/// <summary>
/// Get Email Logs Query (with pagination)
/// </summary>
public record GetEmailLogsQuery(
    int PageNumber = 1,
    int PageSize = 10,
    EmailType? EmailType = null,
    EmailStatus? Status = null
) : IRequest<IEnumerable<EmailLogDto>>;

/// <summary>
/// Get Email Logs Count Query
/// </summary>
public record GetEmailLogsCountQuery(
    EmailType? EmailType = null,
    EmailStatus? Status = null
) : IRequest<int>;
