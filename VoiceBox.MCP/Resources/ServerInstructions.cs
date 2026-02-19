namespace VoiceBox.MCP.Resources;

public static class ServerInstructions
{
    public static string Load()
    {
        var assembly = typeof(ServerInstructions).Assembly;
        using var stream = assembly.GetManifestResourceStream("VoiceBox.MCP.Resources.Instructions.md");
        if (stream is null)
            throw new InvalidOperationException("Instructions.md not found as embedded resource");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
