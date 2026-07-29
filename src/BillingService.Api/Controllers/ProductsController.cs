using AutoMapper;
using BillingService.Application.DTOs;
using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;
using BillingService.Infrastructure.Identity;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateProductRequest> _validator;
    private readonly IMapper _mapper;

    public ProductsController(
        IProductRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateProductRequest> validator,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var product = new Product
        {
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            UnitPrice = request.UnitPrice,
            StockQuantity = request.StockQuantity
        };

        await _repository.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = _mapper.Map<ProductResponse>(product);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct)
    {
        var product = await _repository.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(_mapper.Map<ProductResponse>(product));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(CancellationToken ct)
    {
        var products = await _repository.GetAllAsync(ct);
        return Ok(_mapper.Map<List<ProductResponse>>(products));
    }
}
