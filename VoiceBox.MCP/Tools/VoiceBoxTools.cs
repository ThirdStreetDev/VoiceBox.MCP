using ModelContextProtocol.Server;
using System.ComponentModel;
using VoiceBox.MCP.Services;

namespace VoiceBox.MCP.Tools;

[McpServerToolType]
public class VoiceBoxTools
{
    [McpServerTool(Name = "listen")]
    [Description("Start listening to the microphone and return transcribed text.")]
    public static async Task<string> Listen(AudioService audioService, TranscriptionService transcriptionService)
    {
        var audio = await audioService.RecordUntilSilenceAsync();
        var text = await transcriptionService.TranscribeAsync(audio);
        return text;
    }
}
