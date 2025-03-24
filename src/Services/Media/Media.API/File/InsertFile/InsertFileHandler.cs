using Media.API.FileType.GetFileTypeById;
using Media.API.Service.Interfaces;

namespace Media.API.File.InsertFile;
public record InsertFileCommand(FileInsertModel Model) : ICommand<Unit>;

internal class InsertFileCommandHandler : ICommandHandler<InsertFileCommand, Unit>
{
    private readonly IDocumentSession _session;
    private readonly IFileUploaderService _fileUploaderService;
    private readonly ISender _sender;
    private readonly ILogger<InsertFileCommandHandler> _logger;

    public InsertFileCommandHandler(
        IDocumentSession session,
        IFileUploaderService fileUploaderService,
        ISender sender,
        ILogger<InsertFileCommandHandler> logger)
    {
        _session = session;
        _fileUploaderService = fileUploaderService;
        _sender = sender;
        _logger = logger;
    }

    public async Task<Unit> Handle(InsertFileCommand command, CancellationToken cancellationToken)
    {
        if (command.Model == null || command.Model.File == null || command.Model.File.Length == 0)
            throw new ArgumentException("No file provided for upload.");

        Model.FileType fileType;
        try
        {
            var query = new GetFileTypeByIdQuery(command.Model.FileTypeId);
            fileType = await _sender.Send(query, cancellationToken);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError(ex, "FileType not found for identifier.");
            throw new ArgumentException("Invalid FileType.", ex);
        }

        FileInsertModel uploadedFileInfo;
        try
        {
            uploadedFileInfo = await _fileUploaderService.FileUploadAsync(
                command.Model.File,
                command.Model.ProductId.GetValueOrDefault(),
                fileType.Identifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file to Cloudflare R2.");
            throw new InvalidOperationException("File upload failed.", ex);
        }

        var file = new Model.File
        {
            Id = Guid.NewGuid(),
            FileName = uploadedFileInfo.FileName ?? command.Model.FileName ?? command.Model.File.FileName,
            Extension = uploadedFileInfo.Extension ?? command.Model.Extension ?? Path.GetExtension(command.Model.File.FileName),
            StorageLocation = uploadedFileInfo.StorageLocation ?? command.Model.StorageLocation,
            DisplayName = command.Model.DisplayName ?? command.Model.FileName ?? command.Model.File.FileName,
            FileTypeId = command.Model.FileTypeId,
            UserId = command.Model.UserId,
            ProductId = command.Model.ProductId,
            ThumbnailStorageLocation = uploadedFileInfo.ThumbnailStorageLocation ?? command.Model.ThumbnailStorageLocation,
            SmallStorageLocation = uploadedFileInfo.SmallStorageLocation ?? command.Model.SmallStorageLocation,
            MediumStorageLocation = uploadedFileInfo.MediumStorageLocation ?? command.Model.MediumStorageLocation,
            LargeStorageLocation = uploadedFileInfo.LargeStorageLocation ?? command.Model.LargeStorageLocation,
            ImageOrder = command.Model.ImageOrder,
            FileSize = uploadedFileInfo.FileSize > 0 ? uploadedFileInfo.FileSize : command.Model.File.Length,
            IsActive = command.Model.IsActive || true,
            Created = DateTime.UtcNow
        };

        try
        {
            _session.Store(file);
            await _session.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file metadata to database. Rolling back upload.");

            try
            {
                await _fileUploaderService.DeleteFileAsync(file.StorageLocation);
            }
            catch (Exception rollbackEx)
            {
                _logger.LogError(rollbackEx, "Failed to rollback file upload on Cloudflare R2.");
            }

            throw new InvalidOperationException("Failed to save file metadata.", ex);
        }

        _logger.LogInformation("File {FileId} uploaded and saved successfully.", file.Id);
        return Unit.Value;
    }
}
