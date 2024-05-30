using Matrix.Tools.Packager;
using MatrixSdk;

var playout = new PackagePlayout("bad-apple.pak");
var imageBuffer = ImageBuffer.Create();

while (playout.Next(imageBuffer))
{
     imageBuffer.Invert();
     imageBuffer.Threshold(10);
    imageBuffer.Commit();
}