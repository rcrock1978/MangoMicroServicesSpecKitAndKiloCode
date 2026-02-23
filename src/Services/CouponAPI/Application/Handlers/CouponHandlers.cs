using MediatR;
using AutoMapper;
using Mango.Services.CouponAPI.Application.Commands;
using Mango.Services.CouponAPI.Application.DTOs;
using Mango.Services.CouponAPI.Application.Queries;
using Mango.Services.CouponAPI.Domain;
using Mango.Services.CouponAPI.Domain.Interfaces;

namespace Mango.Services.CouponAPI.Application.Handlers;

/// <summary>
/// Create Coupon Handler
/// </summary>
public class CreateCouponHandler : IRequestHandler<CreateCouponCommand, CouponDto>
{
    private readonly ICouponRepository _repository;
    private readonly IMapper _mapper;

    public CreateCouponHandler(ICouponRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CouponDto> Handle(CreateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = new Coupon
        {
            Code = request.Code.ToUpperInvariant(),
            Description = request.Description,
            CouponType = request.CouponType,
            DiscountAmount = request.DiscountAmount,
            DiscountPercentage = request.DiscountPercentage,
            MinOrderAmount = request.MinOrderAmount,
            MaxUsageCount = request.MaxUsageCount,
            ValidFrom = request.ValidFrom,
            ValidUntil = request.ValidUntil,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(coupon);
        return _mapper.Map<CouponDto>(created);
    }
}

/// <summary>
/// Update Coupon Handler
/// </summary>
public class UpdateCouponHandler : IRequestHandler<UpdateCouponCommand, CouponDto?>
{
    private readonly ICouponRepository _repository;
    private readonly IMapper _mapper;

    public UpdateCouponHandler(ICouponRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CouponDto?> Handle(UpdateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _repository.GetByIdAsync(request.Id);
        if (coupon == null)
            return null;

        coupon.Code = request.Code.ToUpperInvariant();
        coupon.Description = request.Description;
        coupon.CouponType = request.CouponType;
        coupon.DiscountAmount = request.DiscountAmount;
        coupon.DiscountPercentage = request.DiscountPercentage;
        coupon.MinOrderAmount = request.MinOrderAmount;
        coupon.MaxUsageCount = request.MaxUsageCount;
        coupon.ValidFrom = request.ValidFrom;
        coupon.ValidUntil = request.ValidUntil;
        coupon.IsActive = request.IsActive;
        coupon.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(coupon);
        return _mapper.Map<CouponDto>(coupon);
    }
}

/// <summary>
/// Delete Coupon Handler
/// </summary>
public class DeleteCouponHandler : IRequestHandler<DeleteCouponCommand, bool>
{
    private readonly ICouponRepository _repository;

    public DeleteCouponHandler(ICouponRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _repository.GetByIdAsync(request.Id);
        if (coupon == null)
            return false;

        await _repository.DeleteAsync(request.Id);
        return true;
    }
}

/// <summary>
/// Validate Coupon Handler
/// </summary>
public class ValidateCouponHandler : IRequestHandler<ValidateCouponCommand, CouponValidationResultDto>
{
    private readonly ICouponRepository _repository;

    public ValidateCouponHandler(ICouponRepository repository)
    {
        _repository = repository;
    }

    public async Task<CouponValidationResultDto> Handle(ValidateCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _repository.GetByCodeAsync(request.Code.ToUpperInvariant());
        
        if (coupon == null)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "Coupon not found",
                Code = request.Code
            };
        }

        if (!coupon.IsActive)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "Coupon is not active",
                Code = request.Code
            };
        }

        var now = DateTime.UtcNow;
        if (now < coupon.ValidFrom || now > coupon.ValidUntil)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "Coupon is not valid at this time",
                Code = request.Code
            };
        }

        if (coupon.UsedCount >= coupon.MaxUsageCount && coupon.MaxUsageCount > 0)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = "Coupon usage limit exceeded",
                Code = request.Code
            };
        }

        if (request.OrderAmount < coupon.MinOrderAmount)
        {
            return new CouponValidationResultDto
            {
                IsValid = false,
                ErrorMessage = $"Minimum order amount of {coupon.MinOrderAmount} required",
                Code = request.Code
            };
        }

        decimal discount = coupon.CouponType == CouponType.Percentage
            ? (request.OrderAmount * coupon.DiscountPercentage / 100)
            : coupon.DiscountAmount;

        return new CouponValidationResultDto
        {
            IsValid = true,
            DiscountAmount = discount,
            Code = request.Code
        };
    }
}

/// <summary>
/// Get Coupon By ID Handler
/// </summary>
public class GetCouponByIdHandler : IRequestHandler<GetCouponByIdQuery, CouponDto?>
{
    private readonly ICouponRepository _repository;
    private readonly IMapper _mapper;

    public GetCouponByIdHandler(ICouponRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CouponDto?> Handle(GetCouponByIdQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _repository.GetByIdAsync(request.Id);
        if (coupon == null)
            return null;
        
        return _mapper.Map<CouponDto>(coupon);
    }
}

/// <summary>
/// Get Coupon By Code Handler
/// </summary>
public class GetCouponByCodeHandler : IRequestHandler<GetCouponByCodeQuery, CouponDto?>
{
    private readonly ICouponRepository _repository;
    private readonly IMapper _mapper;

    public GetCouponByCodeHandler(ICouponRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<CouponDto?> Handle(GetCouponByCodeQuery request, CancellationToken cancellationToken)
    {
        var coupon = await _repository.GetByCodeAsync(request.Code.ToUpperInvariant());
        if (coupon == null)
            return null;
        
        return _mapper.Map<CouponDto>(coupon);
    }
}

/// <summary>
/// Get Coupons Handler
/// </summary>
public class GetCouponsHandler : IRequestHandler<GetCouponsQuery, List<CouponDto>>
{
    private readonly ICouponRepository _repository;
    private readonly IMapper _mapper;

    public GetCouponsHandler(ICouponRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<CouponDto>> Handle(GetCouponsQuery request, CancellationToken cancellationToken)
    {
        var coupons = await _repository.GetAllAsync();
        return _mapper.Map<List<CouponDto>>(coupons);
    }
}
