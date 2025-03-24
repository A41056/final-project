namespace Media.API.File.UpdateFile;
public record UpdateFileCommand(FileInsertModel Model) : ICommand<Unit>;

internal class UpdateFileCommandHandler(IDocumentSession session)
    : ICommandHandler<UpdateFileCommand, Unit>
{
    public async Task<Unit> Handle(UpdateFileCommand command, CancellationToken cancellationToken)
    {
        var file = await session.LoadAsync<Model.File>(command.Model.Id);
        if (file == null)
            throw new KeyNotFoundException($"File with ID {command.Model.Id} not found.");

        file.FileName = command.Model.FileName ?? file.FileName;
        file.Extension = command.Model.Extension ?? file.Extension;
        file.StorageLocation = command.Model.StorageLocation ?? file.StorageLocation;
        file.FileTypeId = command.Model.FileTypeId;
        file.UserId = command.Model.UserId ?? file.UserId;
        file.ProductId = command.Model.ProductId ?? file.ProductId;
        file.FileSize = command.Model.FileSize;

        session.Update(file);
        await session.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
