using MediatR;
using AutoMapper;
using Mango.Services.EmailAPI.Application.Commands;
using Mango.Services.EmailAPI.Application.Queries;
using Mango.Services.EmailAPI.Application.DTOs;
using Mango.Services.EmailAPI.Domain;
using Mango.Services.EmailAPI.Domain.Interfaces;

namespace Mango.Services.EmailAPI.Application.Handlers;

/// <summary>
/// Email Command Handlers
/// </summary>
public class SendEmailHandler : IRequestHandler<SendEmailCommand, EmailLogDto>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public SendEmailHandler(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<EmailLogDto> Handle(SendEmailCommand request, CancellationToken cancellationToken)
    {
        var emailLog = new EmailLog
        {
            Id = Guid.NewGuid(),
            ToEmail = request.ToEmail,
            CcEmail = request.CcEmail,
            Subject = request.Subject,
            Body = request.Body,
            EmailType = request.EmailType,
            Status = EmailStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            // In a real application, you would send the email here via SMTP or a third-party service
            // For now, we simulate sending
            await Task.Delay(100, cancellationToken); // Simulate network delay
            
            emailLog.Status = EmailStatus.Sent;
            emailLog.SentAt = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            emailLog.Status = EmailStatus.Failed;
            emailLog.ErrorMessage = ex.Message;
        }

        var created = await _emailRepository.CreateEmailLogAsync(emailLog);
        return _mapper.Map<EmailLogDto>(created);
    }
}

public class SendTemplatedEmailHandler : IRequestHandler<SendTemplatedEmailCommand, EmailLogDto>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public SendTemplatedEmailHandler(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<EmailLogDto> Handle(SendTemplatedEmailCommand request, CancellationToken cancellationToken)
    {
        // Get the template
        var template = await _emailRepository.GetEmailTemplateByTypeAsync(request.EmailType);
        if (template == null)
        {
            throw new InvalidOperationException($"Email template for type {request.EmailType} not found");
        }

        // Replace placeholders
        var subject = template.Subject;
        var body = template.Body;

        foreach (var placeholder in request.Placeholders)
        {
            subject = subject.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
            body = body.Replace($"{{{{{placeholder.Key}}}}}", placeholder.Value);
        }

        // Create and send the email
        var command = new SendEmailCommand(
            request.ToEmail,
            request.CcEmail,
            subject,
            body,
            request.EmailType
        );

        var handler = new SendEmailHandler(_emailRepository, _mapper);
        return await handler.Handle(command, cancellationToken);
    }
}

public class CreateEmailTemplateHandler : IRequestHandler<CreateEmailTemplateCommand, EmailTemplateDto>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public CreateEmailTemplateHandler(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<EmailTemplateDto> Handle(CreateEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new EmailTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Subject = request.Subject,
            Body = request.Body,
            EmailType = request.EmailType,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _emailRepository.CreateEmailTemplateAsync(template);
        return _mapper.Map<EmailTemplateDto>(created);
    }
}

public class DeleteEmailTemplateHandler : IRequestHandler<DeleteEmailTemplateCommand, bool>
{
    private readonly IEmailRepository _emailRepository;

    public DeleteEmailTemplateHandler(IEmailRepository emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<bool> Handle(DeleteEmailTemplateCommand request, CancellationToken cancellationToken)
    {
        return await _emailRepository.DeleteEmailTemplateAsync(request.Id);
    }
}

/// <summary>
/// Email Query Handlers
/// </summary>
public class GetEmailTemplateByIdHandler : IRequestHandler<GetEmailTemplateByIdQuery, EmailTemplateDto?>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public GetEmailTemplateByIdHandler(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByIdQuery request, CancellationToken cancellationToken)
    {
        var template = await _emailRepository.GetEmailTemplateByIdAsync(request.Id);
        return template == null ? null : _mapper.Map<EmailTemplateDto>(template);
    }
}

public class GetEmailTemplateByTypeHandler : IRequestHandler<GetEmailTemplateByTypeQuery, EmailTemplateDto?>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public GetEmailTemplateByTypeHandler(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<EmailTemplateDto?> Handle(GetEmailTemplateByTypeQuery request, CancellationToken cancellationToken)
    {
        var template = await _emailRepository.GetEmailTemplateByTypeAsync(request.EmailType);
        return template == null ? null : _mapper.Map<EmailTemplateDto>(template);
    }
}

public class GetAllEmailTemplatesHandler : IRequestHandler<GetAllEmailTemplatesQuery, IEnumerable<EmailTemplateDto>>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public GetAllEmailTemplatesHandler(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EmailTemplateDto>> Handle(GetAllEmailTemplatesQuery request, CancellationToken cancellationToken)
    {
        var templates = await _emailRepository.GetAllEmailTemplatesAsync();
        return _mapper.Map<IEnumerable<EmailTemplateDto>>(templates);
    }
}

public class GetEmailLogByIdHandler : IRequestHandler<GetEmailLogByIdQuery, EmailLogDto?>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public GetEmailLogByIdHandler(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<EmailLogDto?> Handle(GetEmailLogByIdQuery request, CancellationToken cancellationToken)
    {
        var log = await _emailRepository.GetEmailLogByIdAsync(request.Id);
        return log == null ? null : _mapper.Map<EmailLogDto>(log);
    }
}

public class GetEmailLogsHandler : IRequestHandler<GetEmailLogsQuery, IEnumerable<EmailLogDto>>
{
    private readonly IEmailRepository _emailRepository;
    private readonly IMapper _mapper;

    public GetEmailLogsHandler(IEmailRepository emailRepository, IMapper mapper)
    {
        _emailRepository = emailRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EmailLogDto>> Handle(GetEmailLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await _emailRepository.GetEmailLogsAsync(
            request.PageNumber,
            request.PageSize,
            request.EmailType,
            request.Status
        );
        return _mapper.Map<IEnumerable<EmailLogDto>>(logs);
    }
}

public class GetEmailLogsCountHandler : IRequestHandler<GetEmailLogsCountQuery, int>
{
    private readonly IEmailRepository _emailRepository;

    public GetEmailLogsCountHandler(IEmailRepository emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<int> Handle(GetEmailLogsCountQuery request, CancellationToken cancellationToken)
    {
        return await _emailRepository.GetEmailLogsCountAsync(request.EmailType, request.Status);
    }
}
