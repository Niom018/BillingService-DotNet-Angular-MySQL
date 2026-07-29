using AutoMapper;
using BillingService.Application.DTOs;
using BillingService.Domain.Entities;

namespace BillingService.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Customer, CustomerResponse>();
        CreateMap<Product, ProductResponse>();

        CreateMap<OrderItem, OrderItemResponse>()
            .ForCtorParam(nameof(OrderItemResponse.ProductName),
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

        CreateMap<Order, OrderResponse>()
            .ForCtorParam(nameof(OrderResponse.CustomerName),
                opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty))
            .ForCtorParam(nameof(OrderResponse.Status),
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForCtorParam(nameof(OrderResponse.PaymentMethod),
                opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Method.ToString() : null))
            .ForCtorParam(nameof(OrderResponse.PaymentStatus),
                opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Status.ToString() : null));
    }
}
