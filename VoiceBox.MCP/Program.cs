using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using VoiceBox.MCP.Resources;
using VoiceBox.MCP.Services;

var builder = Host.CreateEmptyApplicationBuilder(settings: null);

builder.Services.AddSingleton<AudioService>();
builder.Services.AddSingleton<TranscriptionService>();
builder.Services.AddMcpServer(options =>
{
    options.ServerInfo = new Implementation { Name = "VoiceBox", Version = "1.0.0" };
    options.ServerInstructions = ServerInstructions.Load();
})
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

var transcription = app.Services.GetRequiredService<TranscriptionService>();
await transcription.InitializeAsync();

await app.RunAsync();