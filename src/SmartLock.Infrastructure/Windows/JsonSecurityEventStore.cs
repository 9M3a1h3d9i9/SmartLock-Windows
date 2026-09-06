using System.Text.Json;
using SmartLock.Core.Models;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

public sealed class JsonSecurityEventStore : ISecurityEventStore
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public JsonSecurityEventStore(string? filePath = null)
    {
        _filePath = filePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SmartLock",
            "security-events.json");
    }

    public async Task AppendAsync(SecurityEvent securityEvent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(securityEvent);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var events = await LoadUnsafeAsync(cancellationToken);
            events.Add(securityEvent);
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            var tempPath = _filePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(events, JsonOptions), cancellationToken);
            File.Move(tempPath, _filePath, true);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<IReadOnlyList<SecurityEvent>> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            return await LoadUnsafeAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<SecurityEvent>> LoadUnsafeAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_filePath, cancellationToken);
        return JsonSerializer.Deserialize<List<SecurityEvent>>(json, JsonOptions) ?? [];
    }
}
