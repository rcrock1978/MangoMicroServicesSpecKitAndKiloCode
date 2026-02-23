using MediatR;
using AutoMapper;
using Mango.Services.RewardAPI.Application.Commands;
using Mango.Services.RewardAPI.Application.DTOs;
using Mango.Services.RewardAPI.Application.Queries;
using Mango.Services.RewardAPI.Domain;
using Mango.Services.RewardAPI.Domain.Interfaces;

namespace Mango.Services.RewardAPI.Application.Handlers;

/// <summary>
/// Earn Points Handler
/// </summary>
public class EarnPointsHandler : IRequestHandler<EarnPointsCommand, UserRewardDto>
{
    private readonly IUserRewardRepository _repository;
    private readonly IMapper _mapper;

    public EarnPointsHandler(IUserRewardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserRewardDto> Handle(EarnPointsCommand request, CancellationToken cancellationToken)
    {
        var userReward = await _repository.GetByUserIdWithTransactionsAsync(request.UserId);
        
        if (userReward == null)
        {
            // Create new user reward
            userReward = new UserReward
            {
                UserId = request.UserId,
                TotalPoints = request.Points,
                AvailablePoints = request.Points,
                LifetimePoints = request.Points,
                CreatedAt = DateTime.UtcNow
            };
            
            userReward.Transactions.Add(new RewardTransaction
            {
                UserId = request.UserId,
                Type = TransactionType.Earned,
                Points = request.Points,
                Description = request.Description,
                OrderId = request.OrderId
            });
            
            await _repository.AddAsync(userReward);
        }
        else
        {
            // Add points to existing reward
            userReward.TotalPoints += request.Points;
            userReward.AvailablePoints += request.Points;
            userReward.LifetimePoints += request.Points;
            userReward.UpdatedAt = DateTime.UtcNow;
            
            userReward.Transactions.Add(new RewardTransaction
            {
                UserRewardId = userReward.Id,
                UserId = request.UserId,
                Type = TransactionType.Earned,
                Points = request.Points,
                Description = request.Description,
                OrderId = request.OrderId
            });
            
            await _repository.UpdateAsync(userReward);
        }
        
        return _mapper.Map<UserRewardDto>(userReward);
    }
}

/// <summary>
/// Redeem Points Handler
/// </summary>
public class RedeemPointsHandler : IRequestHandler<RedeemPointsCommand, UserRewardDto?>
{
    private readonly IUserRewardRepository _repository;
    private readonly IMapper _mapper;

    public RedeemPointsHandler(IUserRewardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserRewardDto?> Handle(RedeemPointsCommand request, CancellationToken cancellationToken)
    {
        var userReward = await _repository.GetByUserIdWithTransactionsAsync(request.UserId);
        
        if (userReward == null || userReward.AvailablePoints < request.Points)
            return null;
        
        // Deduct points
        userReward.AvailablePoints -= request.Points;
        userReward.UpdatedAt = DateTime.UtcNow;
        
        userReward.Transactions.Add(new RewardTransaction
        {
            UserRewardId = userReward.Id,
            UserId = request.UserId,
            Type = TransactionType.Redeemed,
            Points = -request.Points,
            Description = request.Description
        });
        
        await _repository.UpdateAsync(userReward);
        
        return _mapper.Map<UserRewardDto>(userReward);
    }
}

/// <summary>
/// Create Reward Handler
/// </summary>
public class CreateRewardHandler : IRequestHandler<CreateRewardCommand, RewardDto>
{
    private readonly IRewardRepository _repository;
    private readonly IMapper _mapper;

    public CreateRewardHandler(IRewardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RewardDto> Handle(CreateRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = new Reward
        {
            Name = request.Name,
            Description = request.Description,
            PointsRequired = request.PointsRequired,
            ImageUrl = request.ImageUrl,
            MaxAvailable = request.MaxAvailable,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(reward);
        return _mapper.Map<RewardDto>(created);
    }
}

/// <summary>
/// Delete Reward Handler
/// </summary>
public class DeleteRewardHandler : IRequestHandler<DeleteRewardCommand, bool>
{
    private readonly IRewardRepository _repository;

    public DeleteRewardHandler(IRewardRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = await _repository.GetByIdAsync(request.Id);
        if (reward == null)
            return false;

        await _repository.DeleteAsync(request.Id);
        return true;
    }
}

/// <summary>
/// Get User Reward By User ID Handler
/// </summary>
public class GetUserRewardByUserIdHandler : IRequestHandler<GetUserRewardByUserIdQuery, UserRewardDto?>
{
    private readonly IUserRewardRepository _repository;
    private readonly IMapper _mapper;

    public GetUserRewardByUserIdHandler(IUserRewardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserRewardDto?> Handle(GetUserRewardByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userReward = await _repository.GetByUserIdWithTransactionsAsync(request.UserId);
        if (userReward == null)
            return null;
        
        return _mapper.Map<UserRewardDto>(userReward);
    }
}

/// <summary>
/// Get Rewards Handler
/// </summary>
public class GetRewardsHandler : IRequestHandler<GetRewardsQuery, List<RewardDto>>
{
    private readonly IRewardRepository _repository;
    private readonly IMapper _mapper;

    public GetRewardsHandler(IRewardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<RewardDto>> Handle(GetRewardsQuery request, CancellationToken cancellationToken)
    {
        var rewards = await _repository.GetAllAsync();
        return _mapper.Map<List<RewardDto>>(rewards);
    }
}

/// <summary>
/// Get Reward By ID Handler
/// </summary>
public class GetRewardByIdHandler : IRequestHandler<GetRewardByIdQuery, RewardDto?>
{
    private readonly IRewardRepository _repository;
    private readonly IMapper _mapper;

    public GetRewardByIdHandler(IRewardRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<RewardDto?> Handle(GetRewardByIdQuery request, CancellationToken cancellationToken)
    {
        var reward = await _repository.GetByIdAsync(request.Id);
        if (reward == null)
            return null;
        
        return _mapper.Map<RewardDto>(reward);
    }
}
