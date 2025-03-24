using Media.API.Enum;

namespace Media.API.Common;

public static class Constants
{
    public static class FileSettings
    {
        public static readonly List<EFileTypeIdentifier> ListImageType = new()
            {
                EFileTypeIdentifier.ImageProduct,
                EFileTypeIdentifier.ImageUser
            };

        public const string KeyReplaceYear = "{year}";
        public const string KeyReplaceMonth = "{month}";
        public const string KeyReplaceId = "{id}";
        public const string KeyReplaceUserInput = "{userinput}";
        public const string KeyReplaceTimestamp = "{timestamp}";

        public const string FileIsEmptyKey = "file_is_empty_key";
        public const string FileTypeIsNotConfiguredKey = "file_type_is_not_configured_key";

        public const string RootPathFileStorage = "./data";
        public const string SubFolderPublic = "public";
        public const string SubFolderPrivate = "private";

        public const string FileExtensionNotValid = "{0}_must_be_{1}";
        public const string FileSizeNotValid = "{0}_must_be_less_than_{1}MB";
    }
}
