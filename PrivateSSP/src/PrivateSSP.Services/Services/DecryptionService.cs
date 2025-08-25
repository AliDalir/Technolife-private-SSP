using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PrivateSSP.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace PrivateSSP.Services.Services;

/// <summary>
/// Service for decrypting hashed PII data from WebEngage
/// </summary>
public class DecryptionService : IDecryptionService
{
    private readonly ILogger<DecryptionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public DecryptionService(ILogger<DecryptionService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Get encryption key and IV from configuration
        var keyString = _configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not configured");
        var ivString = _configuration["Encryption:IV"] ?? throw new InvalidOperationException("Encryption IV not configured");
        
        _key = Convert.FromBase64String(keyString);
        _iv = Convert.FromBase64String(ivString);
    }

    public async Task<string> DecryptPhoneNumberAsync(string hashedPhone)
    {
        try
        {
            _logger.LogInformation("Decrypting phone number hash: {HashPrefix}...", hashedPhone[..8]);
            
            var decrypted = await DecryptValueAsync(hashedPhone);
            
            _logger.LogInformation("Successfully decrypted phone number hash");
            return decrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt phone number hash: {Hash}", hashedPhone);
            throw;
        }
    }

    public async Task<string> DecryptEmailAsync(string hashedEmail)
    {
        try
        {
            _logger.LogInformation("Decrypting email hash: {HashPrefix}...", hashedEmail[..8]);
            
            var decrypted = await DecryptValueAsync(hashedEmail);
            
            _logger.LogInformation("Successfully decrypted email hash");
            return decrypted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to decrypt email hash: {Hash}", hashedEmail);
            throw;
        }
    }

    public async Task<bool> ValidateHashedValueAsync(string hashedValue)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hashedValue))
                return false;

            // Basic validation - check if it's a valid base64 string
            if (hashedValue.Length % 4 != 0)
                return false;

            try
            {
                Convert.FromBase64String(hashedValue);
                return true;
            }
            catch
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate hashed value: {Value}", hashedValue);
            return false;
        }
    }

    private async Task<string> DecryptValueAsync(string encryptedValue)
    {
        return await Task.Run(() =>
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            var encryptedBytes = Convert.FromBase64String(encryptedValue);
            var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            
            return Encoding.UTF8.GetString(decryptedBytes);
        });
    }
}
