using System.Diagnostics.CodeAnalysis;
using ErrorOr;
using FluentValidation;
using MediatR;

namespace GeminiWarehouse.Application.Common.Messaging;

[ExcludeFromCodeCoverageAttribute]
// pipeline behavior which will surround the register command
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationBehavior(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validator is null)
        {
            return await next();
        }

        // codes here executed BEFORE the handler
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        // if failed, convert result to ErrorOr<AuthenticationResult>
        if (validationResult.IsValid)
        {
            return await next();
        }

        var errors = validationResult.Errors
            .ConvertAll(validationFailure => Error.Validation(
                validationFailure.PropertyName,
                validationFailure.ErrorMessage));

        // codes here executed AFTER the handler
        return (dynamic)errors;
    }
}
