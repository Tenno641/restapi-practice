using Connecty.Contracts.Responses;
using FluentValidation;

namespace Connecty.Api.Middlewares;

public class ValidationMappingMiddleware
{
    private readonly RequestDelegate _next;
    
    public ValidationMappingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException e)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            
            ValidationFailureResponse failureResponse = new ValidationFailureResponse
            {
                Errors = e.Errors.Select(error => new ValidationResponse
                {
                    Property = error.PropertyName,
                    Message = error.ErrorMessage
                })
            };
            
            await context.Response.WriteAsJsonAsync(failureResponse);
        }
    }
}