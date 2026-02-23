using AutoMapper;
using Mango.Services.RewardAPI.Domain;
using Mango.Services.RewardAPI.Application.DTOs;

namespace Mango.Services.RewardAPI.Infrastructure.Mappings;

/// <summary>
/// AutoMapper Profile for Reward API
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // UserReward mappings
        CreateMap<UserReward, UserRewardDto>().ReverseMap();
        
        // RewardTransaction mappings
        CreateMap<RewardTransaction, RewardTransactionDto>().ReverseMap();
        
        // Reward mappings
        CreateMap<Reward, RewardDto>().ReverseMap();
    }
}
