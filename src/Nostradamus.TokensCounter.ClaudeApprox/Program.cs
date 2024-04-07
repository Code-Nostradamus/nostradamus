const int ApproxSymbolsPerToken = 5;

if (args.Length != 1)
{
    Console.WriteLine("Usage: SymbolCounter <directory_path>");
    return;
}

string directoryPath = args[0];

if (!Directory.Exists(directoryPath))
{
    Console.WriteLine($"The directory '{directoryPath}' does not exist.");
    return;
}

try
{
    long totalSymbols = await CountSymbolsInCsFilesAsync(directoryPath);
    Console.WriteLine($"Total symbols in '.cs' files: {totalSymbols:N0}");
    Console.WriteLine($"Approximate tokens: {totalSymbols / ApproxSymbolsPerToken:N0}");
}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}

return;

static async Task<long> CountSymbolsInCsFilesAsync(string directoryPath)
{
    long totalSymbols = 0;
    var csFilePaths = Directory.EnumerateFiles(directoryPath, "*.cs", SearchOption.AllDirectories);

    await Parallel.ForEachAsync(csFilePaths, async (filePath, cancellationToken) =>
    {
        long fileSymbols = (await File.ReadAllTextAsync(filePath, cancellationToken)).Length;
        Interlocked.Add(ref totalSymbols, fileSymbols);
    });

    return totalSymbols;
}
