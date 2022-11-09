namespace MatrixDisplay;

using System.Text.Json.Serialization;

[JsonSerializable(typeof(DisplayOptions))]
internal sealed partial class AppSerializerContext : JsonSerializerContext
{
}
