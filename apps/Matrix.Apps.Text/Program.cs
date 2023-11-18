using MatrixSdk;
using SkiaSharp;

var buffer = ImageBuffer.Create();
using var bitmap = new SKBitmap(buffer.Width, buffer.Height);

using var canvas = new SKCanvas(bitmap);
using var typeface = SKTypeface.FromFamilyName("Arial");
using var font = new SKFont(typeface);
using var paint = new SKPaint(font) { Color = SKColors.Red, };
canvas.DrawText("H", default, paint);

buffer.DrawImage(bitmap);
buffer.Commit();