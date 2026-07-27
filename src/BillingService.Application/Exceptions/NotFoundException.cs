namespace BillingService.Application.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }

    public static NotFoundException For<T>(object key) =>
        new($"{typeof(T).Name} with id '{key}' was not found.");
}
