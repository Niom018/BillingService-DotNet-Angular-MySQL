using BillingService.Application.DTOs;
using FluentValidation;

namespace BillingService.Application.Validators;

public class RecordPaymentRequestValidator : AbstractValidator<RecordPaymentRequest>
{
    private static readonly string[] ValidMethods = { "Cash", "Card", "Mfs" };
    private static readonly string[] ValidMfsProviders = { "Bkash", "Nagad", "Rocket", "Upay" };

    public RecordPaymentRequestValidator()
    {
        RuleFor(x => x.Method)
            .NotEmpty()
            .Must(m => ValidMethods.Contains(m, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Method must be one of: {string.Join(", ", ValidMethods)}");

        RuleFor(x => x.AmountPaid)
            .GreaterThan(0);

        When(x => string.Equals(x.Method, "Mfs", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.MfsProvider)
                .Must(p => p != null && ValidMfsProviders.Contains(p, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"MfsProvider is required and must be one of: {string.Join(", ", ValidMfsProviders)}");

            RuleFor(x => x.TransactionReference)
                .NotEmpty()
                .WithMessage("Transaction reference is required for MFS payments.");
        });

        When(x => string.Equals(x.Method, "Card", StringComparison.OrdinalIgnoreCase), () =>
        {
            RuleFor(x => x.TransactionReference)
                .NotEmpty()
                .WithMessage("Transaction reference is required for card payments.");
        });
    }
}
