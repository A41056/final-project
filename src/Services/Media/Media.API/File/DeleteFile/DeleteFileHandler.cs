using Media.API.Service.Interfaces;

namespace Media.API.File.DeleteFile;
public record DeleteFileCommand(string FileName) : ICommand<DeleteFileResult>;
public record DeleteFileResult(bool IsSuccess);

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().WithMessage("File name is required");
    }
}
internal class DeleteFileHandler : ICommandHandler<DeleteFileCommand, DeleteFileResult>
{
    private readonly IDocumentSession _session;
    private readonly IStorageService _storageService;

    public DeleteFileHandler(IDocumentSession session, IStorageService storageService)
    {
        _session = session;
        _storageService = storageService;
    }

    public async Task<DeleteFileResult> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
    {
        var file = _session.Query<Model.File>()
            .FirstOrDefault(f => f.StorageLocation.EndsWith(command.FileName));

        if (file == null)
        {
            throw new FileNotFoundException(command.FileName);
        }

        await _storageService.DeleteFile(file.StorageLocation);

        _session.Delete(file);
        await _session.SaveChangesAsync(cancellationToken);

        return new DeleteFileResult(true);
    }
}