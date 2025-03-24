using Media.API.Service.Interfaces;

namespace Media.API.File.DeletePhysicalFile;

public record DeletePhysicalFileCommand(FileModel Model) : ICommand<Unit>;

internal class DeletePhysicalFileCommandHandler(IStorageService storageService)
    : ICommandHandler<DeletePhysicalFileCommand, Unit>
{
    public async Task<Unit> Handle(DeletePhysicalFileCommand command, CancellationToken cancellationToken)
    {
        await storageService.DeleteFile(command.Model.StorageLocation);
        return Unit.Value;
    }
}
