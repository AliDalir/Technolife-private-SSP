using PrivateSSP.Core.Models;

namespace PrivateSSP.Core.Interfaces;

/// <summary>
/// Service for decrypting hashed PII data from WebEngage
/// </summary>
public interface IDecryptionService
{
    /// <summary>
    /// Decrypts a hashed phone number
    /// </summary>
    /// <param name="hashedPhone">The hashed phone number to decrypt</param>
    /// <returns>The decrypted phone number</returns>
    Task<string> DecryptPhoneNumberAsync(string hashedPhone);

    /// <summary>
    /// Decrypts a hashed email address
    /// </summary>
    /// <param name="hashedEmail">The hashed email to decrypt</param>
    /// <returns>The decrypted email address</returns>
    Task<string> DecryptEmailAsync(string hashedEmail);

    /// <summary>
    /// Validates if a hashed value is valid
    /// </summary>
    /// <param name="hashedValue">The hashed value to validate</param>
    /// <returns>True if the hashed value is valid</returns>
    Task<bool> ValidateHashedValueAsync(string hashedValue);
}
