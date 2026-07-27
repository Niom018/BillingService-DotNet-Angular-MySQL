using BillingService.Application.DTOs;
using FluentValidation;

namespace BillingService.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .GreaterThan(0);

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("An order must have at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        });
    }
}
