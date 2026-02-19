using VoiceBox.Installer;

const string AppName = "VoiceBox";
const string ServerName = "voicebox";
const string ExeName = "VoiceBox.MCP.exe";

Console.WriteLine($"{AppName} MCP Installer");
Console.WriteLine(new string('=', 40));
Console.WriteLine();

var uninstall = args.Any(a => a.Equals("--uninstall", StringComparison.OrdinalIgnoreCase) ||
                               a.Equals("-u", StringComparison.OrdinalIgnoreCase));

if (uninstall)
{
    RunUninstall();
}
else
{
    RunInstall();
}

void RunInstall()
{
    Console.Write("Checking for Claude Desktop... ");
    if (!ClaudeDesktopConfig.IsClaudeDesktopInstalled())
    {
        Console.WriteLine("NOT FOUND");
        Console.WriteLine();
        Console.WriteLine("Claude Desktop does not appear to be installed.");
        Console.WriteLine("Please install Claude Desktop first from: https://claude.ai/download");
        ExitWithError();
        return;
    }
    Console.WriteLine("OK");

    var sourcePath = FindSourcePath(args);
    if (sourcePath == null)
    {
        Console.WriteLine();
        Console.WriteLine("Could not find VoiceBox.MCP files.");
        Console.WriteLine("Please run the installer from the same directory as VoiceBox.MCP.exe");
        Console.WriteLine("or specify the path: VoiceBox.Installer.exe --source <path>");
        ExitWithError();
        return;
    }
    Console.WriteLine($"Source path: {sourcePath}");

    var installPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName);
    Console.WriteLine($"Install path: {installPath}");

    Console.Write("Copying files... ");
    try
    {
        CopyFiles(sourcePath, installPath);
        Console.WriteLine("OK");
    }
    catch (Exception ex)
    {
        Console.WriteLine("FAILED");
        Console.WriteLine($"Error: {ex.Message}");
        ExitWithError();
        return;
    }

    Console.Write("Backing up Claude config... ");
    var backupPath = ClaudeDesktopConfig.BackupConfig();
    if (backupPath != null)
    {
        Console.WriteLine($"OK ({Path.GetFileName(backupPath)})");
    }
    else
    {
        Console.WriteLine("SKIPPED (no existing config)");
    }

    Console.Write("Updating Claude Desktop config... ");
    try
    {
        var config = ClaudeDesktopConfig.ReadConfig();
        var exePath = Path.Combine(installPath, ExeName);

        if (ClaudeDesktopConfig.HasServer(config, ServerName))
        {
            Console.WriteLine("UPDATED (existing entry)");
        }
        else
        {
            Console.WriteLine("OK");
        }

        ClaudeDesktopConfig.AddServer(config, ServerName, exePath);
        ClaudeDesktopConfig.WriteConfig(config);
    }
    catch (Exception ex)
    {
        Console.WriteLine("FAILED");
        Console.WriteLine($"Error: {ex.Message}");
        ExitWithError();
        return;
    }

    Console.WriteLine();
    Console.WriteLine($"{AppName} installed successfully!");
    Console.WriteLine();
    Console.WriteLine("Next steps:");
    Console.WriteLine("  1. Restart Claude Desktop");
    Console.WriteLine("  2. Start a new conversation");
    Console.WriteLine("  3. Say \"listen\" and start talking!");
    Console.WriteLine();

    WaitForKey();
}

void RunUninstall()
{
    Console.WriteLine("Uninstalling...");
    Console.WriteLine();

    Console.Write("Updating Claude Desktop config... ");
    try
    {
        var config = ClaudeDesktopConfig.ReadConfig();
        if (ClaudeDesktopConfig.RemoveServer(config, ServerName))
        {
            ClaudeDesktopConfig.WriteConfig(config);
            Console.WriteLine("OK");
        }
        else
        {
            Console.WriteLine("SKIPPED (not configured)");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("FAILED");
        Console.WriteLine($"Error: {ex.Message}");
    }

    var installPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppName);

    Console.WriteLine();
    Console.WriteLine($"Application files are located at: {installPath}");
    Console.WriteLine("You may manually delete the folder to remove all files.");
    Console.WriteLine();

    Console.WriteLine($"{AppName} uninstalled successfully!");
    Console.WriteLine("Please restart Claude Desktop.");
    Console.WriteLine();

    WaitForKey();
}

string? FindSourcePath(string[] args)
{
    for (int i = 0; i < args.Length - 1; i++)
    {
        if (args[i].Equals("--source", StringComparison.OrdinalIgnoreCase) ||
            args[i].Equals("-s", StringComparison.OrdinalIgnoreCase))
        {
            var path = args[i + 1];
            if (Directory.Exists(path) && File.Exists(Path.Combine(path, ExeName)))
            {
                return path;
            }
        }
    }

    var currentDir = Directory.GetCurrentDirectory();
    if (File.Exists(Path.Combine(currentDir, ExeName)))
    {
        return currentDir;
    }

    var installerDir = Path.GetDirectoryName(Environment.ProcessPath);
    if (installerDir != null && File.Exists(Path.Combine(installerDir, ExeName)))
    {
        return installerDir;
    }

    return null;
}

void CopyFiles(string sourcePath, string destPath)
{
    Directory.CreateDirectory(destPath);

    foreach (var file in Directory.GetFiles(sourcePath))
    {
        var fileName = Path.GetFileName(file);
        var destFile = Path.Combine(destPath, fileName);
        File.Copy(file, destFile, overwrite: true);
    }

    foreach (var dir in Directory.GetDirectories(sourcePath))
    {
        var dirName = Path.GetFileName(dir);
        CopyFiles(dir, Path.Combine(destPath, dirName));
    }
}

void WaitForKey()
{
    Console.WriteLine("Press any key to exit...");
    Console.ReadKey(intercept: true);
}

void ExitWithError()
{
    Console.WriteLine();
    WaitForKey();
    Environment.Exit(1);
}
