using System.Net.Http.Json;
using System.Text.Json;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

public sealed class TelegramAlertService : ITelegramAlertService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _botToken;
    private readonly string _chatId;

    public TelegramAlertService(string? botToken = null, string? chatId = null, HttpClient? httpClient = null)
    {
        _botToken = botToken ?? Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? string.Empty;
        _chatId = chatId ?? Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID") ?? string.Empty;
        _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
    }

    public async Task<bool> SendIncidentAsync(
        string message,
        string? photoPath = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_botToken) || string.IsNullOrWhiteSpace(_chatId) || string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        try
        {
            var baseUrl = $"https://api.telegram.org/bot{_botToken}";

            if (!string.IsNullOrWhiteSpace(photoPath) && File.Exists(photoPath))
            {
                await using var stream = File.OpenRead(photoPath);
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(_chatId), "chat_id");
                content.Add(new StringContent(message), "caption");
                content.Add(new StreamContent(stream), "photo", Path.GetFileName(photoPath));

                using var response = await _httpClient.PostAsync($"{baseUrl}/sendPhoto", content, cancellationToken);
                return response.IsSuccessStatusCode;
            }

            var payload = new { chat_id = _chatId, text = message };
            using var textResponse = await _httpClient.PostAsJsonAsync($"{baseUrl}/sendMessage", payload, cancellationToken);
            return textResponse.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException)
        {
            return false;
        }
    }

    public void Dispose() => _httpClient.Dispose();
}
