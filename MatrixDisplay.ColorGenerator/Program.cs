using System.Drawing;

foreach (var knownColor in Enum.GetValues<KnownColor>().OrderBy(x => x.ToString()))
{
    var color = Color.FromKnownColor(knownColor);

    Console.WriteLine($"public static Color {knownColor} => new({color.R}, {color.G}, {color.B});");
}