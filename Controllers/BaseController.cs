using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ONEERP.Platform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected async Task<List<string>> ValidateAsync<T>(IValidator<T>? validator, T model)
    {
        if (validator is null)
            return new List<string>();

        var result = await validator.ValidateAsync(model);
        return result.Errors.Select(e => e.ErrorMessage).ToList();
    }
}
