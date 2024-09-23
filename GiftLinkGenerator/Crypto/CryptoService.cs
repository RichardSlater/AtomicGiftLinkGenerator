// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace GiftLinkGenerator.Crypto;

public class CryptoService(IOptions<CryptoOptions> options) : ICryptoService {
    private readonly UTF8Encoding _encoder = new(encoderShouldEmitUTF8Identifier: false);
    private readonly CryptoOptions _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    private readonly HashAlgorithmName _hashAlgorithmName = new(options.Value.HashAlgorithm);

    public string Encrypt(string plaintext, string password) {
        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));
        
        var plaintextBytes = _encoder.GetBytes(plaintext);
        var saltBytes = _encoder.GetBytes(_options.Salt ?? throw new InvalidOperationException("Salt can not be null, check configuration."));

        var key = new Rfc2898DeriveBytes(password, saltBytes, _options.Iterations, _hashAlgorithmName);
        var encryptionAlgorithm = Aes.Create();
        encryptionAlgorithm.Key = key.GetBytes(16);

        using var buffer = new MemoryStream();
        buffer.Write(encryptionAlgorithm.IV);
        buffer.Write(_encoder.GetBytes("!")); // 0x33

        using var cryptoStream =
            new CryptoStream(buffer, encryptionAlgorithm.CreateEncryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(plaintextBytes, 0, plaintextBytes.Length);
        cryptoStream.FlushFinalBlock();
        cryptoStream.Close();

        var ciphertext = buffer.ToArray();
        return Convert.ToBase64String(ciphertext);
    }

    public string Decrypt(string ciphertext, string password) {
        var ciphertextBytes = Convert.FromBase64String(ciphertext);

        if (ciphertextBytes[16] != 33) {
            throw new ArgumentException("Ciphertext does not match the expected format (iv[16]!data[...]");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(password, nameof(password));

        var saltBytes = _encoder.GetBytes(_options.Salt ?? throw new InvalidOperationException("Salt can not be null, check configuration."));
        var key = new Rfc2898DeriveBytes(password, saltBytes, _options.Iterations, _hashAlgorithmName);
        var encryptionAlgorithm = Aes.Create();
        encryptionAlgorithm.Key = key.GetBytes(16);
        encryptionAlgorithm.IV = ciphertextBytes[0..16];

        using var buffer = new MemoryStream();
        using var cryptoStream =
            new CryptoStream(buffer, encryptionAlgorithm.CreateDecryptor(), CryptoStreamMode.Write);
        cryptoStream.Write(ciphertextBytes, 17, ciphertextBytes.Length - 17);
        cryptoStream.Flush();
        cryptoStream.Close();

        return _encoder.GetString(buffer.ToArray());
    }
}