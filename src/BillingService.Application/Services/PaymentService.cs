using AutoMapper;
using BillingService.Application.DTOs;
using BillingService.Application.Exceptions;
using BillingService.Application.Interfaces;
using BillingService.Domain.Entities;
using BillingService.Domain.Enums;
using FluentValidation;

namespace BillingService.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidator<RecordPaymentRequest> _validator;
    private readonly IMapper _mapper;

    public PaymentService(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,
        IValidator<RecordPaymentRequest> validator,
        IMapper mapper)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<OrderResponse> RecordPaymentAsync(int orderId, RecordPaymentRequest request, CancellationToken ct = default)
    {
        await _validator.ValidateAndThrowAsync(request, ct);

        var order = await _orderRepository.GetByIdAsync(orderId, ct)
            ?? throw NotFoundException.For<Order>(orderId);

        if (order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException("Payment can only be recorded for a confirmed order.");

        if (order.Payment is not null)
            throw new InvalidOperationException("This order already has a payment recorded.");

        var method = Enum.Parse<PaymentMethod>(request.Method, ignoreCase: true);

        Payment payment = method switch
        {
            PaymentMethod.Cash => Payment.CreateCashPayment(order, request.AmountPaid),

            PaymentMethod.Card => Payment.CreateCardPayment(
                order,
                request.AmountPaid,
                request.TransactionReference!),

            PaymentMethod.Mfs => Payment.CreateMfsPayment(
                order,
                request.AmountPaid,
                Enum.Parse<MfsProvider>(request.MfsProvider!, ignoreCase: true),
                request.TransactionReference!),

            _ => throw new ArgumentException($"Unsupported payment method: {request.Method}")
        };

        order.Payment = payment;
        order.Complete();

        await _unitOfWork.SaveChangesAsync(ct);

        return _mapper.Map<OrderResponse>(order);
    }
}
