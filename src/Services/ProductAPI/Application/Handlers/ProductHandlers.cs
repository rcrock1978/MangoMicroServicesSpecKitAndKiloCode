using MediatR;
using AutoMapper;
using Mango.Services.ProductAPI.Application.Commands;
using Mango.Services.ProductAPI.Application.DTOs;
using Mango.Services.ProductAPI.Application.Queries;
using Mango.Services.ProductAPI.Domain;
using Mango.Services.ProductAPI.Domain.Interfaces;

namespace Mango.Services.ProductAPI.Application.Handlers;

/// <summary>
/// Create Product Handler
/// </summary>
public class CreateProductHandler : IRequestHandler<CreateProductCommand, ProductDto>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public CreateProductHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            CategoryName = request.CategoryName,
            ImageUrl = request.ImageUrl,
            StockQuantity = request.StockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _repository.AddAsync(product);
        return _mapper.Map<ProductDto>(created);
    }
}

/// <summary>
/// Update Product Handler
/// </summary>
public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, ProductDto?>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public UpdateProductHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        if (product == null)
            return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Price = request.Price;
        product.CategoryName = request.CategoryName;
        product.ImageUrl = request.ImageUrl;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(product);
        return _mapper.Map<ProductDto>(product);
    }
}

/// <summary>
/// Delete Product Handler
/// </summary>
public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _repository;

    public DeleteProductHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        if (product == null)
            return false;

        await _repository.DeleteAsync(request.Id);
        return true;
    }
}

/// <summary>
/// Get Product By ID Handler
/// </summary>
public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.Id);
        if (product == null)
            return null;
        return _mapper.Map<ProductDto>(product);
    }
}

/// <summary>
/// Get Products Handler
/// </summary>
public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repository;
    private readonly IMapper _mapper;

    public GetProductsHandler(IProductRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.FindAsync(p => p.IsActive);

        // Apply filters
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = _repository.FindAsync(p => 
                p.IsActive && (p.Name.Contains(request.SearchTerm) || p.Description.Contains(request.SearchTerm)));
        }

        if (!string.IsNullOrEmpty(request.CategoryName))
        {
            query = _repository.FindAsync(p => 
                p.IsActive && p.CategoryName == request.CategoryName);
        }

        var products = await query;
        var totalCount = products.Count();

        var pagedProducts = products
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return new PagedResult<ProductDto>
        {
            Items = _mapper.Map<List<ProductDto>>(pagedProducts),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Update Stock Handler
/// </summary>
public class UpdateStockHandler : IRequestHandler<UpdateStockCommand, bool>
{
    private readonly IProductRepository _repository;

    public UpdateStockHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(UpdateStockCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIdAsync(request.ProductId);
        if (product == null)
            return false;

        product.StockQuantity += request.QuantityChange;
        if (product.StockQuantity < 0)
            product.StockQuantity = 0;

        await _repository.UpdateAsync(product);
        return true;
    }
}
