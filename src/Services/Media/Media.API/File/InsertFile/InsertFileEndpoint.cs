namespace Media.API.File.InsertFile;

public class InsertFileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/files", async (HttpRequest req, ISender sender) =>
        {
            var file = req.Form.Files.GetFile("file");
            if (file == null || file.Length == 0)
                return Results.BadRequest("No file provided.");

            var model = new FileInsertModel
            {
                File = file,
                FileName = req.Form["fileName"].FirstOrDefault(),
                Extension = Path.GetExtension(file.FileName),
                FileTypeId = Guid.Parse(req.Form["fileTypeId"].FirstOrDefault() ?? throw new ArgumentException("FileTypeId is required")),
                UserId = req.Form["userId"].FirstOrDefault() != null ? Guid.Parse(req.Form["userId"]) : null,
                ProductId = req.Form["productId"].FirstOrDefault() != null ? Guid.Parse(req.Form["productId"]) : null,
                DisplayName = req.Form["displayName"].FirstOrDefault(),
                ImageOrder = int.TryParse(req.Form["imageOrder"].FirstOrDefault(), out var order) ? order : 0,
                IsActive = bool.TryParse(req.Form["isActive"].FirstOrDefault(), out var isActive) ? isActive : true
            };

            var command = new InsertFileCommand(model);
            await sender.Send(command);

            return Results.Created($"/files/{model.Id}", new
            {
                fileId = model.Id,
                storageLocation = model.StorageLocation
            });
        })
        .WithName("InsertFile")
        .RequireAuthorization()
        .Produces(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Upload and insert a new file")
        .WithDescription("Uploads a file to Cloudflare R2 and saves its metadata.");
    }
}