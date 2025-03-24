using System.ComponentModel;

namespace Media.API.Enum;

public enum EFileTypeIdentifier
{
    [Description("image product")]
    ImageProduct = 1,

    [Description("avatar user")]
    ImageUser = 2,

    [Description("product video")]
    VideoProduct = 3,
}
