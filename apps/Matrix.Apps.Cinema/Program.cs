using Matrix.Tools.Packager;
using MatrixSdk;

var playout = new PackagePlayout("rick.pak");
var imageBuffer = ImageBuffer.Create();

playout.Run(imageBuffer, 30, 1);