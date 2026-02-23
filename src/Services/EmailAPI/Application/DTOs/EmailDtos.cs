using Mango.Services.EmailAPI.Domain;

namespace Mango.Services.EmailAPI.Application.DTOs;

/// <summary>
/// Email Template DTO
/// </summary>
public class EmailTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailType EmailType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Email Log DTO
/// </summary>
public class EmailLogDto
{
    public Guid Id { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string? CcEmail { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailType EmailType { get; set; }
    public EmailStatus Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTOs
/// </summary>
public class SendEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string? CcEmail { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailType EmailType { get; set; }
}

public class SendTemplatedEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string? CcEmail { get; set; }
    public EmailType EmailType { get; set; }
    public Dictionary<string, string> Placeholders { get; set; } = new();
}

public class CreateEmailTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailType EmailType { get; set; }
}
