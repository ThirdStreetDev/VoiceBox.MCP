using Whisper.net.Ggml;
using Whisper.net;

namespace VoiceBox.MCP.Services;

public class TranscriptionService : IDisposable
{
    private const string ModelFileName = "ggml-base.en.bin";
    private WhisperFactory? _factory;
    private bool _initialized;

    public async Task InitializeAsync()
    {
        var modelPath = GetModelPath();

        if (!File.Exists(modelPath))
        {
            await DownloadModelAsync(modelPath);
        }

        _factory = WhisperFactory.FromPath(modelPath);
        _initialized = true;
    }

    public async Task<string> TranscribeAsync(byte[] wavData)
    {
        if (!_initialized || _factory == null)
            throw new InvalidOperationException("TranscriptionService not initialized. Call InitializeAsync first.");

        using var processor = _factory.CreateBuilder()
            .WithLanguage("en")
            .Build();

        using var stream = new MemoryStream(wavData);
        var results = new List<string>();

        await foreach (var result in processor.ProcessAsync(stream))
        {
            var text = result.Text.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                results.Add(text);
            }
        }

        return string.Join(" ", results);
    }

    private static string GetModelPath()
    {
        var appData = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "VoiceBox",
            "models");

        Directory.CreateDirectory(appData);
        return Path.Combine(appData, ModelFileName);
    }

    private static async Task DownloadModelAsync(string modelPath)
    {
        Console.Error.WriteLine("Downloading Whisper model (base.en)... This only happens once.");
        using HttpClient httpClient = new();
        var downloader = new WhisperGgmlDownloader(httpClient);
        using var modelStream = await downloader.GetGgmlModelAsync(GgmlType.Base);
        using var fileStream = File.Create(modelPath);
        await modelStream.CopyToAsync(fileStream);

        Console.Error.WriteLine("Model downloaded successfully.");
    }

    public void Dispose()
    {
        _factory?.Dispose();
    }
}
