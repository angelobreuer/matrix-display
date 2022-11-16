using MatrixDisplay;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

var references = new MetadataReference[]
{
    MetadataReference.CreateFromFile(
        path: typeof(PixelBuffer).Assembly.Location,
        properties: default,
        documentation: null),

    MetadataReference.CreateFromFile(
        path: typeof(Enumerable).Assembly.Location,
        properties: default,
        documentation: null),
};

var options = ScriptOptions.Default
    .AddReferences(references)
    .AddImports(new[] { "MatrixDisplay", "System", "System.Collections.Generic" });

var text = File.ReadAllText(args[0]);

try
{
    CSharpScript
        .RunAsync(code: text, options: options, globals: null, globalsType: null)
        .GetAwaiter()
        .GetResult();
}
catch (Exception value)
{
    Console.WriteLine(value);
}