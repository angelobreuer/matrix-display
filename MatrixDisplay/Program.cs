using System.Net;
using System.Text.Json;
using System.Windows.Forms;
using MatrixDisplay;
using MatrixDisplay.Board;
using MatrixDisplay.Emulator;
using Microsoft.Extensions.Options;

var optionsContent = File.ReadAllText("appsettings.json");
var options = JsonSerializer.Deserialize(optionsContent, AppSerializerContext.Default.DisplayOptions)!;

var engine = new DisplayEngine(Options.Create(options));

if (options.Endpoint is null)
{
    // Emulator
    var form = new MainForm(options.Width, options.Height);
    new Thread(() => engine.Run(form)).Start();
    Application.Run(form);
}
else
{
    engine.Run(new BoardClient(IPEndPoint.Parse(options.Endpoint), options.Width, options.Height));
}