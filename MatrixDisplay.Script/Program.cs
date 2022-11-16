using System.Diagnostics;

var process = default(Process?);
var appsDirectory = Directory.CreateDirectory("apps");

string WaitForCopyFile(string file)
{
    string? text;

    while (true)
    {
        try
        {
            text = File.ReadAllText(file);
            break;
        }
        catch (IOException)
        {
            Thread.Sleep(100);
        }
    }

    var tempFile = Path.GetTempFileName();
    File.WriteAllText(tempFile, text);
    return tempFile;
}

var task = default(Task?);

async Task RunAsync(string file)
{
    var processStartInfo = new ProcessStartInfo("MatrixDisplay.Script.Runner");
    processStartInfo.ArgumentList.Add(file);

    var newProcess = Process.Start(processStartInfo)!;
    var previousProcess = Interlocked.Exchange(ref process, newProcess);

    previousProcess?.Kill(entireProcessTree: true);

    if (task is not null)
    {
        try
        {
            await task.ConfigureAwait(false);
        }
        catch (Exception)
        {
        }
    }

    await newProcess.WaitForExitAsync();
}

using var fileSystemWatcher = new FileSystemWatcher(appsDirectory.FullName)
{
    EnableRaisingEvents = true,
    IncludeSubdirectories = true,
    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.Security,
};

fileSystemWatcher.Changed += (s, e) => Console.WriteLine("e");

while (true)
{
    Console.WriteLine($"Script Executor - Waiting for change ('{appsDirectory.FullName}')...");

    var change = fileSystemWatcher.WaitForChanged(WatcherChangeTypes.Created | WatcherChangeTypes.Changed | WatcherChangeTypes.Renamed);
    Console.WriteLine($"Script Executor - Changed: {change.Name}.");

    var file = WaitForCopyFile(change.Name!);
    task = RunAsync(file);
}