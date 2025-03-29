namespace Media.API.File.DeleteFile;

public class DeleteFileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/files/{fileName}",
            async (string fileName, ISender sender) =>
            {
                var command = new DeleteFileCommand(fileName);
                var result = await sender.Send(command);
                return Results.Ok(result);
            })
            .WithName("DeleteFile")
            .Produces<DeleteFileResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Delete File")
            .WithDescription("Delete a file by its name");
    }
}