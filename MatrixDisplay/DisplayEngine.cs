namespace MatrixDisplay;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

public sealed class DisplayEngine
{
    private readonly DisplayOptions _options;

    public DisplayEngine(IOptions<DisplayOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options.Value;
    }

    public void Run(IDisplayController controller, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(controller);

        var pixelBuffer = new PixelBufferAccessor(
            memoryMappedFileName: PixelBuffer.DefaultMemoryMappedFileName,
            width: _options.Width,
            height: _options.Height);

        var spinWait = new SpinWait();

        while (!cancellationToken.IsCancellationRequested)
        {
            while (!pixelBuffer.TryGetNextFrame())
            {
                cancellationToken.ThrowIfCancellationRequested();
                spinWait.SpinOnce();
            }

            controller.Update(pixelBuffer.Colors);
        }
    }
}
