using System.Security.Cryptography;
using SmartLock.Core.Services;

namespace SmartLock.Infrastructure.Windows;

public sealed class TotpAdminOtpService : IAdminOtpService
{
    private readonly byte[] _secret;

    public TotpAdminOtpService(string? base32Secret = null)
    {
        var configured = base32Secret ?? Environment.GetEnvironmentVariable("ADMIN_OTP_SECRET") ?? string.Empty;
        _secret = DecodeBase32(configured);
    }

    public bool Validate(string code)
    {
        if (_secret.Length == 0 || string.IsNullOrWhiteSpace(code) || code.Length != 6 || !code.All(char.IsDigit))
        {
            return false;
        }

        var currentStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        for (var offset = -1; offset <= 1; offset++)
        {
            if (CryptographicOperations.FixedTimeEquals(
                    ComputeCode(currentStep + offset),
                    System.Text.Encoding.ASCII.GetBytes(code)))
            {
                return true;
            }
        }

        return false;
    }

    private byte[] ComputeCode(long counter)
    {
        Span<byte> data = stackalloc byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteInt64BigEndian(data, counter);
        using var hmac = new HMACSHA1(_secret);
        var hash = hmac.ComputeHash(data.ToArray());
        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                   | (hash[offset + 1] << 16)
                   | (hash[offset + 2] << 8)
                   | hash[offset + 3];
        var otp = binary % 1_000_000;
        return System.Text.Encoding.ASCII.GetBytes(otp.ToString("D6"));
    }

    private static byte[] DecodeBase32(string value)
    {
        var normalized = value.Trim().Replace(" ", string.Empty).TrimEnd('=').ToUpperInvariant();
        if (normalized.Length == 0)
        {
            return [];
        }

        var output = new List<byte>();
        var buffer = 0;
        var bits = 0;
        foreach (var character in normalized)
        {
            var digit = character switch
            {
                >= 'A' and <= 'Z' => character - 'A',
                >= '2' and <= '7' => character - '2' + 26,
                _ => -1
            };

            if (digit < 0)
            {
                return [];
            }

            buffer = (buffer << 5) | digit;
            bits += 5;
            if (bits >= 8)
            {
                bits -= 8;
                output.Add((byte)((buffer >> bits) & 0xFF));
            }
        }

        return output.ToArray();
    }
}
