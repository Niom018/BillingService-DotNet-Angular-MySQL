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
public class CustomersController : ControllerBase
{
    private readonly ICustomerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateCustomerRequest> _validator;
    private readonly IMapper _mapper;

    public CustomersController(
        ICustomerRepository repository,
        IUnitOfWork unitOfWork,
        IValidator<CreateCustomerRequest> validator,
        IMapper mapper)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _mapper = mapper;
    }

    [HttpPost]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Manager}")]
    public async Task<ActionResult<CustomerResponse>> Create(CreateCustomerRequest request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var customer = new Customer
        {
            Name = request.Name,
            Phone = request.Phone,
            Email = request.Email,
            Address = request.Address
        };

        await _repository.AddAsync(customer, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var response = _mapper.Map<CustomerResponse>(customer);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, response);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CustomerResponse>> GetById(int id, CancellationToken ct)
    {
        var customer = await _repository.GetByIdAsync(id, ct);
        return customer is null ? NotFound() : Ok(_mapper.Map<CustomerResponse>(customer));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerResponse>>> GetAll(CancellationToken ct)
    {
        var customers = await _repository.GetAllAsync(ct);
        return Ok(_mapper.Map<List<CustomerResponse>>(customers));
    }
}
