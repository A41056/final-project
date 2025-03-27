namespace Media.API.FileType.GetFileTypes;

public class GetAllFileTypesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/filetypes",
            async (ISender sender) =>
            {
                var query = new GetAllFileTypesQuery();
                var result = await sender.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetAllFileTypes")
            .Produces<List<Model.FileType>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Get All File Types")
            .WithDescription("Retrieves all file types available in the system.")
            .RequireAuthorization();
    }
}
