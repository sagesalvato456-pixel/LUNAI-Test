using System.Text.Json;
using System.Text.Json.Serialization;
using LUNAI.Models;

namespace LUNAI.Services;

public sealed class InstalledAppRecord
{
    [JsonPropertyName("id")]          public string Id            { get; set; } = "";
    [JsonPropertyName("version")]     public string Version       { get; set; } = "";
    [JsonPropertyName("apk_path")]    public string? ApkPath      { get; set; }
    [JsonPropertyName("installed_at")]public string InstalledAt   { get; set; } = "";
}

public sealed class InstallStateStore
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LUNAI", "install_state.json");

    private readonly Dictionary<string, InstalledAppRecord> _records = new();

    public InstallStateStore()
    {
        Load();
    }

    public InstalledAppRecord? Get(AppId id)
        => _records.TryGetValue(id.ToString(), out var r) ? r : null;

    public void Set(AppId id, InstalledAppRecord record)
    {
        _records[id.ToString()] = record;
        Save();
    }

    public void Remove(AppId id)
    {
        _records.Remove(id.ToString());
        Save();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return;
            var json = File.ReadAllText(FilePath);
            var list = JsonSerializer.Deserialize<List<InstalledAppRecord>>(json);
            if (list is null) return;
            foreach (var r in list)
                _records[r.Id] = r;
        }
        catch { /* corrupt file — start fresh */ }
    }

    private void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var json = JsonSerializer.Serialize(_records.Values.ToList(),
                new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch { /* non-fatal */ }
    }
}
