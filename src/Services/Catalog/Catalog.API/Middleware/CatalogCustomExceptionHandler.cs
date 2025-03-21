using Microsoft.AspNetCore.Diagnostics;

namespace Catalog.API.Middleware;

public class CatalogCustomExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        
        if (httpContext.Response.HasStarted)
        {
            Console.WriteLine("Response has already started, cannot modify");
            return false; // Không thể xử lý nếu response đã bắt đầu
        }
        httpContext.Response.ContentType = "application/json";
        switch (exception)
        {
            case DuplicateCategoryException duplicateEx:
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Duplicate category",
                    code = "DUPLICATE_CATEGORY",
                    message = duplicateEx.Message
                }, cancellationToken);
                return true;

            case CategoryInUseException inUseEx:
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Category in use",
                    code = "CATEGORY_IN_USE",
                    message = inUseEx.Message
                }, cancellationToken);
                return true;

            default:
                return false;
        }
    }
}
