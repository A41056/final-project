using Media.API.Service.Interfaces;

namespace Media.API.File.DeleteFile;
public record DeleteFileCommand(Guid FileId) : ICommand<Unit>;

internal class DeleteFileCommandHandler(IDocumentSession session, IStorageService storageService)
    : ICommandHandler<DeleteFileCommand, Unit>
{
    public async Task<Unit> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
    {
        var file = await session.LoadAsync<Model.File>(command.FileId);
        if (file == null)
            throw new KeyNotFoundException($"File with ID {command.FileId} not found.");

        session.Delete(file);
        await session.SaveChangesAsync(cancellationToken);
        await storageService.DeleteFile(file.StorageLocation);

        return Unit.Value;
    }
}
