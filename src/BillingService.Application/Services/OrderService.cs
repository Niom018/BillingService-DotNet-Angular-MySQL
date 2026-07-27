using AutoMapper;
using BillingService.Application.DTOs;
using BillingService.Application.Exceptions;
using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;
using FluentValidation;

namespace BillingService.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<CreateOrderRequest> _createOrderValidator;
    private readonly IMapper _mapper;

    public OrderService(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IValidator<CreateOrderRequest> createOrderValidator,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _createOrderValidator = createOrderValidator;
        _mapper = mapper;
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
    {
        await _createOrderValidator.ValidateAndThrowAsync(request, ct);

        var customer = await _customerRepository.GetByIdAsync(request.CustomerId, ct)
            ?? throw NotFoundException.For<Customer>(request.CustomerId);

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await _productRepository.GetByIdsAsync(productIds, ct);
        var productMap = products.ToDictionary(p => p.Id);

        var missingIds = productIds.Except(productMap.Keys).ToList();
        if (missingIds.Count > 0)
            throw new NotFoundException($"Product(s) not found: {string.Join(", ", missingIds)}");

        var orderNumber = await GenerateUniqueOrderNumberAsync(ct);
        var order = new Order(customer, orderNumber);

        foreach (var item in request.Items)
        {
            order.AddItem(productMap[item.ProductId], item.Quantity);
        }

        await _orderRepository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<OrderResponse> ConfirmOrderAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct)
            ?? throw NotFoundException.For<Order>(orderId);

        order.Confirm();
        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<OrderResponse>(order);
    }

    public async Task<OrderResponse?> GetOrderAsync(int orderId, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, ct);
        return order is null ? null : _mapper.Map<OrderResponse>(order);
    }

    private async Task<string> GenerateUniqueOrderNumberAsync(CancellationToken ct)
    {
        string candidate;
        do
        {
            candidate = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        } while (await _orderRepository.OrderNumberExistsAsync(candidate, ct));

        return candidate;
    }
}
