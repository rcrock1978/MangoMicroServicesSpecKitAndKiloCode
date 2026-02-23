namespace Mango.Services.EmailAPI.Domain;

/// <summary>
/// Email Template entity
/// </summary>
public class EmailTemplate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailType EmailType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Email Log entity
/// </summary>
public class EmailLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ToEmail { get; set; } = string.Empty;
    public string? CcEmail { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public EmailType EmailType { get; set; }
    public EmailStatus Status { get; set; } = EmailStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Email Type enumeration
/// </summary>
public enum EmailType
{
    Welcome = 0,
    OrderConfirmation = 1,
    OrderShipped = 2,
    OrderDelivered = 3,
    PasswordReset = 4,
    AccountVerification = 5,
    Promotional = 6,
    General = 7
}

/// <summary>
/// Email Status enumeration
/// </summary>
public enum EmailStatus
{
    Pending = 0,
    Sent = 1,
    Failed = 2,
    Retry = 3
}
