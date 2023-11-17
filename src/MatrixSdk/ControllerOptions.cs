namespace MatrixSdk;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class ControllerOptions
{
    public static ControllerOptions LoadOptions()
    {
        // Walk up directories until we find a file named "appsettings.json"
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (directory is not null)
        {
            var optionsFile = new FileInfo(Path.Combine(directory.FullName, "appsettings.json"));

            if (optionsFile.Exists)
            {
                return JsonSerializer.Deserialize(
                    json: File.ReadAllText(optionsFile.FullName),
                    jsonTypeInfo: ControllerOptionsJsonSerializerContext.Default.ControllerOptions)!;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("Could not find options.json");
    }

    [JsonRequired]
    [JsonPropertyName(nameof(Width))]
    public required int Width { get; init; }

    [JsonRequired]
    [JsonPropertyName(nameof(Height))]
    public required int Height { get; init; }

    [JsonRequired]
    [JsonPropertyName(nameof(Host))]
    public required string Host { get; init; }

    [JsonRequired]
    [JsonPropertyName(nameof(Port))]
    public required int Port { get; init; }

    [JsonPropertyName(nameof(MirroringOption))]
    [JsonConverter(typeof(JsonStringEnumConverter<ImageMirroringOption>))]
    public ImageMirroringOption MirroringOption { get; init; }
}

[JsonSerializable(typeof(ControllerOptions))]
internal sealed partial class ControllerOptionsJsonSerializerContext : JsonSerializerContext
{
}