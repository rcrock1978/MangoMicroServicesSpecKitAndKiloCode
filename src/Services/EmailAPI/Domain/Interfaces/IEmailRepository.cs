using Mango.Services.EmailAPI.Domain;

namespace Mango.Services.EmailAPI.Domain.Interfaces;

/// <summary>
/// Email Template Repository Interface
/// </summary>
public interface IEmailTemplateRepository
{
    Task<EmailTemplate?> GetByIdAsync(Guid id);
    Task<EmailTemplate?> GetByTypeAsync(EmailType emailType);
    Task<IEnumerable<EmailTemplate>> GetAllAsync();
    Task<EmailTemplate> AddAsync(EmailTemplate entity);
    Task UpdateAsync(EmailTemplate entity);
    Task DeleteAsync(Guid id);
}

/// <summary>
/// Email Log Repository Interface
/// </summary>
public interface IEmailLogRepository
{
    Task<EmailLog?> GetByIdAsync(Guid id);
    Task<IEnumerable<EmailLog>> GetByEmailAsync(string email);
    Task<IEnumerable<EmailLog>> GetAllAsync(int page, int pageSize);
    Task<EmailLog> AddAsync(EmailLog entity);
    Task UpdateAsync(EmailLog entity);
}
