using Microsoft.AspNetCore.Mvc;
using SmartCv.Models;

namespace SmartCv.Controllers;

public static class FactoryResultExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, FactoryResult<T> result, int successStatusCode = StatusCodes.Status200OK) =>
        result.Status switch
        {
            FactoryStatus.Success => controller.StatusCode(successStatusCode, result.Value),
            FactoryStatus.NotFound => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status404NotFound),
            _ => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest),
        };

    public static IActionResult ToActionResult(this ControllerBase controller, FactoryResult result) =>
        result.Status switch
        {
            FactoryStatus.Success => controller.NoContent(),
            FactoryStatus.NotFound => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status404NotFound),
            _ => controller.Problem(detail: result.Error, statusCode: StatusCodes.Status400BadRequest),
        };
}
