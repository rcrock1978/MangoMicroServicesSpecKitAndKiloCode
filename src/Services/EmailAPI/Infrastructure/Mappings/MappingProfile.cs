using AutoMapper;
using Mango.Services.EmailAPI.Domain;
using Mango.Services.EmailAPI.Application.DTOs;

namespace Mango.Services.EmailAPI.Infrastructure.Mappings;

/// <summary>
/// AutoMapper Profile for Email API
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // EmailTemplate mappings
        CreateMap<EmailTemplate, EmailTemplateDto>().ReverseMap();

        // EmailLog mappings
        CreateMap<EmailLog, EmailLogDto>().ReverseMap();
    }
}
