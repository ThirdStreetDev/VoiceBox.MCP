using ModelContextProtocol.Server;
using System.ComponentModel;
using VoiceBox.MCP.Services;

namespace VoiceBox.MCP.Tools;

[McpServerToolType]
public class VoiceBoxTools
{
    [McpServerTool(Name = "listen")]
    [Description("Capture speech from the user's microphone via local Whisper transcription. " +
        "After receiving the result: (1) Echo the transcription back as 'Heard: \"...\"' so the user can catch errors, " +
        "(2) (2) Respond concisely by following instructions or answering the prompt. Keep responses brief during voice interactions.")]
    public static async Task<string> Listen(AudioService audioService, TranscriptionService transcriptionService)
    {
        var audio = await audioService.RecordUntilSilenceAsync();
        var text = await transcriptionService.TranscribeAsync(audio);
        return text;
    }
}
