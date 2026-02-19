using System.Text.Json.Nodes;
using System.Text.Json;

namespace VoiceBox.Installer;

public class ClaudeDesktopConfig
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string GetConfigPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Claude",
                "claude_desktop_config.json");
        }
        else if (OperatingSystem.IsMacOS())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library",
                "Application Support",
                "Claude",
                "claude_desktop_config.json");
        }
        else if (OperatingSystem.IsLinux())
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                ".config",
                "Claude",
                "claude_desktop_config.json");
        }

        throw new PlatformNotSupportedException("Unsupported operating system");
    }

    public static bool IsClaudeDesktopInstalled()
    {
        var configDir = Path.GetDirectoryName(GetConfigPath());
        return configDir != null && Directory.Exists(configDir);
    }

    public static JsonObject ReadConfig()
    {
        var configPath = GetConfigPath();

        if (File.Exists(configPath))
        {
            var json = File.ReadAllText(configPath);
            return JsonNode.Parse(json)?.AsObject() ?? CreateEmptyConfig();
        }

        return CreateEmptyConfig();
    }

    public static string? BackupConfig()
    {
        var configPath = GetConfigPath();

        if (!File.Exists(configPath))
            return null;

        var backupPath = $"{configPath}.backup.{DateTime.Now:yyyyMMdd_HHmmss}";
        File.Copy(configPath, backupPath);
        return backupPath;
    }

    public static void AddServer(JsonObject config, string serverName, string command, string[]? args = null, Dictionary<string, string>? env = null)
    {
        if (!config.ContainsKey("mcpServers"))
        {
            config["mcpServers"] = new JsonObject();
        }

        var mcpServers = config["mcpServers"]!.AsObject();

        var serverConfig = new JsonObject
        {
            ["command"] = command
        };

        if (args != null && args.Length > 0)
        {
            var argsArray = new JsonArray();
            foreach (var arg in args)
            {
                argsArray.Add(arg);
            }
            serverConfig["args"] = argsArray;
        }

        if (env != null && env.Count > 0)
        {
            var envObject = new JsonObject();
            foreach (var kvp in env)
            {
                envObject[kvp.Key] = kvp.Value;
            }
            serverConfig["env"] = envObject;
        }

        mcpServers[serverName] = serverConfig;
    }

    public static bool RemoveServer(JsonObject config, string serverName)
    {
        if (!config.ContainsKey("mcpServers"))
            return false;

        var mcpServers = config["mcpServers"]!.AsObject();
        return mcpServers.Remove(serverName);
    }

    public static bool HasServer(JsonObject config, string serverName)
    {
        if (!config.ContainsKey("mcpServers"))
            return false;

        var mcpServers = config["mcpServers"]!.AsObject();
        return mcpServers.ContainsKey(serverName);
    }

    public static void WriteConfig(JsonObject config)
    {
        var configPath = GetConfigPath();
        var configDir = Path.GetDirectoryName(configPath);

        if (configDir != null && !Directory.Exists(configDir))
        {
            Directory.CreateDirectory(configDir);
        }

        var json = config.ToJsonString(JsonOptions);
        File.WriteAllText(configPath, json);
    }

    private static JsonObject CreateEmptyConfig()
    {
        return new JsonObject
        {
            ["mcpServers"] = new JsonObject()
        };
    }
}
