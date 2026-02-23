using Microsoft.EntityFrameworkCore;
using Mango.Services.EmailAPI.Domain;
using Mango.Services.EmailAPI.Domain.Interfaces;
using Mango.Services.EmailAPI.Infrastructure.Data;

namespace Mango.Services.EmailAPI.Infrastructure.Repositories;

/// <summary>
/// Email Repository Implementation
/// </summary>
public class EmailRepository : IEmailRepository
{
    private readonly EmailDbContext _context;

    public EmailRepository(EmailDbContext context)
    {
        _context = context;
    }

    #region EmailTemplate Operations

    public async Task<EmailTemplate?> GetEmailTemplateByIdAsync(Guid id)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<EmailTemplate?> GetEmailTemplateByTypeAsync(EmailType emailType)
    {
        return await _context.EmailTemplates
            .FirstOrDefaultAsync(t => t.EmailType == emailType && t.IsActive);
    }

    public async Task<IEnumerable<EmailTemplate>> GetAllEmailTemplatesAsync()
    {
        return await _context.EmailTemplates
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<EmailTemplate> CreateEmailTemplateAsync(EmailTemplate template)
    {
        _context.EmailTemplates.Add(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<EmailTemplate> UpdateEmailTemplateAsync(EmailTemplate template)
    {
        template.UpdatedAt = DateTime.UtcNow;
        _context.EmailTemplates.Update(template);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<bool> DeleteEmailTemplateAsync(Guid id)
    {
        var template = await _context.EmailTemplates.FindAsync(id);
        if (template == null)
            return false;

        _context.EmailTemplates.Remove(template);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region EmailLog Operations

    public async Task<EmailLog?> GetEmailLogByIdAsync(Guid id)
    {
        return await _context.EmailLogs
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<EmailLog>> GetEmailLogsAsync(
        int pageNumber,
        int pageSize,
        EmailType? emailType = null,
        EmailStatus? status = null)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (emailType.HasValue)
        {
            query = query.Where(l => l.EmailType == emailType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetEmailLogsCountAsync(
        EmailType? emailType = null,
        EmailStatus? status = null)
    {
        var query = _context.EmailLogs.AsQueryable();

        if (emailType.HasValue)
        {
            query = query.Where(l => l.EmailType == emailType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        return await query.CountAsync();
    }

    public async Task<EmailLog> CreateEmailLogAsync(EmailLog emailLog)
    {
        _context.EmailLogs.Add(emailLog);
        await _context.SaveChangesAsync();
        return emailLog;
    }

    public async Task<EmailLog> UpdateEmailLogAsync(EmailLog emailLog)
    {
        _context.EmailLogs.Update(emailLog);
        await _context.SaveChangesAsync();
        return emailLog;
    }

    public async Task<bool> DeleteEmailLogAsync(Guid id)
    {
        var log = await _context.EmailLogs.FindAsync(id);
        if (log == null)
            return false;

        _context.EmailLogs.Remove(log);
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion
}
