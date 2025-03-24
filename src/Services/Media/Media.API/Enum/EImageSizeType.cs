using System.ComponentModel;

namespace Media.API.Enum;

public enum EImageSizeType
{
    [Description("2048 x 2048")] Large,
    [Description("1024 x 1024")] Medium,
    [Description("512 x 512")] Small,
    [Description("266 x 177")] Thumbnail,
    [Description("Original")] Original
}
