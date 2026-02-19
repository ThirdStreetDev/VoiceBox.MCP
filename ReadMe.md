# 🎙️ VoiceBox

**Talk to Claude instead of typing.** VoiceBox is an MCP server that captures audio from your microphone and transcribes it locally using [Whisper](https://github.com/ggerganov/whisper.cpp) — nothing leaves your machine.

<!-- TODO: Add a GIF demo here -->
<!-- ![VoiceBox Demo](docs/demo.gif) -->

## How It Works

1. You type `/listen` in Claude Desktop
2. VoiceBox captures audio from your microphone
3. Speech is transcribed locally using Whisper (base.en model)
4. The transcribed text is returned to Claude as your message

The Whisper model downloads automatically on first run (~142 MB). All processing happens on your machine — no audio is sent to any external service.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A working microphone
- [Claude Desktop](https://claude.ai/download)

## Installation

### Option 1: Installer (recommended)

Download the latest release and run the installer:

```bash
VoiceBox.Installer.exe
```

The installer will:
- Detect your Claude Desktop installation
- Copy VoiceBox to your local app data folder
- Automatically update your `claude_desktop_config.json`
- Back up your existing config before making changes

Final Step: Restart Claude Desktop after installing.

To uninstall:

```bash
VoiceBox.Installer.exe --uninstall
```

### Option 2: Manual setup

Clone and build:

```bash
git clone https://github.com/thirdstreetdev/voicebox-mcp.git
cd voicebox-mcp
dotnet build
```

Manually Add VoiceBox to your Claude Desktop config (`claude_desktop_config.json`):

```json
{
  "mcpServers": {
    "voicebox": {
      "command": "dotnet",
      "args": ["run", "--project", "/path/to/VoiceBox.MCP"]
    }
  }
}
```

Restart Claude Desktop after saving.

## Usage

| Command | What it does |
|---------|-------------|
| `/listen` | Capture and transcribe speech |

You can also type things like **"let me talk"**, **"voice mode"**, or **"use my mic"** and Claude will know to start listening.

### Tips

- Speak naturally — VoiceBox detects when you stop talking (1.5s of silence)
- Max recording length is 30 seconds per capture
- If transcription seems off, try speaking closer to your mic

## Project Structure

```
VoiceBox/
├── README.md
├── LICENSE
├── VoiceBox.MCP/
│   ├── Program.cs
│   ├── Tools/
│   │   └── VoiceBoxTools.cs
│   ├── Services/
│   │   ├── AudioService.cs
│   │   └── TranscriptionService.cs
│   ├── Resources/
│   │   └── Instructions.md
│   └── VoiceBox.MCP.csproj
├── VoiceBox.Installer/
│   ├── Program.cs
│   └── ClaudeDesktopConfig.cs
└── VoiceBox.sln
```

## Tech Stack

- **[.NET 10](https://dotnet.microsoft.com/)** — Runtime
- **[Whisper.net](https://github.com/sandrohanea/whisper.net)** — Local speech-to-text via whisper.cpp bindings
- **[NAudio](https://github.com/naudio/NAudio)** — Microphone capture
- **[Model Context Protocol (MCP)](https://modelcontextprotocol.io/)** — Tool interface for Claude

## Known Limitations

- **Built-in microphones** may produce inconsistent results or blank audio depending on hardware, noise cancellation settings, and environment. For best results, use a dedicated external microphone (USB or headset).
- **Silence detection** can occasionally trigger early in noisy environments or if the mic gain is too low.

## Roadmap

- [ ] Test and document external microphone compatibility
- [ ] Speak tool — text-to-speech output for Claude Desktop responses
- [ ] Configurable Whisper model size (small, medium, large)
- [ ] Multi-language support
- [ ] Continuous listening mode
- [ ] Audio file transcription tool
- [ ] Configurable silence detection threshold

## Privacy

VoiceBox runs entirely on your machine. Audio is captured, transcribed locally via Whisper, and the text is passed to your MCP client. No audio data is transmitted to any external service.

## License

[MIT](LICENSE)

---

Built with the [Model Context Protocol](https://modelcontextprotocol.io/) · Works with [Claude Desktop](https://claude.ai/download)