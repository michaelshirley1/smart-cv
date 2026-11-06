using Microsoft.AspNetCore.Mvc;
using SmartCv.Models;

namespace SmartCv.Controllers;

public static class FactoryResultExtensions
{
    public static IActionResult ToActionResult<T>(this FactoryResult<T> result, int successStatusCode = StatusCodes.Status200OK) =>
        result.Status switch
        {
            FactoryStatus.Success => new ObjectResult(result.Value) { StatusCode = successStatusCode },
            FactoryStatus.NotFound => new NotFoundObjectResult(new ErrorResponse(result.Error)),
            _ => new BadRequestObjectResult(new ErrorResponse(result.Error)),
        };

    public static IActionResult ToActionResult(this FactoryResult result) =>
        result.Status switch
        {
            FactoryStatus.Success => new NoContentResult(),
            FactoryStatus.NotFound => new NotFoundObjectResult(new ErrorResponse(result.Error)),
            _ => new BadRequestObjectResult(new ErrorResponse(result.Error)),
        };
}
