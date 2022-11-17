using MatrixDisplay;

var fileName = args.Length is 0 ? "picture.bmp" : args[0];
var buffer = PixelBuffer.Instance;

buffer.Picture(fileName);