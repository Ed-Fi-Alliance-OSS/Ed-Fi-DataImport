// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Security.Cryptography;
using System.Text;

namespace DataImport.Common
{
    /// <summary>
    /// AES-256-CBC authenticated encryption using the same wire format as the former
    /// Rijndael256 3.2.0 <c>RijndaelEtM</c> class, enabling seamless decryption of values
    /// already stored in the database.
    /// </summary>
    /// <remarks>
    /// Wire format: <c>Base64(iv[16] + aes_cbc_ciphertext + pbkdf2_mac[64])</c>
    ///
    /// Key derivation (matches Rijndael256 AeKeyRing + Rijndael.GenerateKey exactly):
    ///   1. H = SHA-512(UTF-8(password)) rendered as uppercase hex (128 chars)
    ///   2. CipherKey = H[0..63]  (64 upper-hex chars)
    ///   3. MacKey    = UTF-8(H[64..127])  (64 ASCII bytes)
    ///   4. step1_salt = SHA-512(UTF-8(CipherKey + "64")) as uppercase hex
    ///   5. step2_salt = PBKDF2-HMAC-SHA1(CipherKey_bytes, step1_salt_bytes, 10000, 64)
    ///   6. aesKey    = PBKDF2-HMAC-SHA1(CipherKey_bytes, step2_salt, 10000, 32)
    ///
    /// MAC = PBKDF2-HMAC-SHA1(iv + ciphertext, MacKey, 10000, 64)
    /// </remarks>
    public static class Encryption
    {
        private const int IvBytes = 16;
        private const int MacBytes = 64;
        private const int AesKeyBytes = 32;
        private const int Iterations = 10000;

        /// <summary>
        /// Encrypts plaintext using AES-256-CBC with an Encrypt-then-MAC authentication tag.
        /// </summary>
        public static string Encrypt(string plainText, string encryptionKey)
        {
            DeriveKeys(encryptionKey, out var aesKey, out var macKey);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            aes.GenerateIV();
            var iv = aes.IV;

            var encryptedBody = aes.EncryptCbc(Encoding.UTF8.GetBytes(plainText), iv);

            // ciphertext = IV + encrypted body
            var ciphertext = new byte[IvBytes + encryptedBody.Length];
            Buffer.BlockCopy(iv, 0, ciphertext, 0, IvBytes);
            Buffer.BlockCopy(encryptedBody, 0, ciphertext, IvBytes, encryptedBody.Length);

            // MAC over the full ciphertext (iv + body)
            var mac = Rfc2898DeriveBytes.Pbkdf2(ciphertext, macKey, Iterations, HashAlgorithmName.SHA1, MacBytes);

            // Output = ciphertext + mac
            var output = new byte[ciphertext.Length + MacBytes];
            Buffer.BlockCopy(ciphertext, 0, output, 0, ciphertext.Length);
            Buffer.BlockCopy(mac, 0, output, ciphertext.Length, MacBytes);

            return Convert.ToBase64String(output);
        }

        /// <summary>
        /// Decrypts EtM ciphertext produced by this class or by Rijndael256 3.2.0
        /// <c>RijndaelEtM.Encrypt(plainText, key, KeySize.Aes256)</c>.
        /// </summary>
        public static string Decrypt(string cipherText, string encryptionKey)
        {
            DeriveKeys(encryptionKey, out var aesKey, out var macKey);

            var input = Convert.FromBase64String(cipherText);

            // Split into ciphertext (iv + body) and mac
            var ciphertext = new byte[input.Length - MacBytes];
            var mac = new byte[MacBytes];
            Buffer.BlockCopy(input, 0, ciphertext, 0, ciphertext.Length);
            Buffer.BlockCopy(input, ciphertext.Length, mac, 0, MacBytes);

            // Verify MAC before decrypting
            var expectedMac = Rfc2898DeriveBytes.Pbkdf2(ciphertext, macKey, Iterations, HashAlgorithmName.SHA1, MacBytes);
            if (!CryptographicOperations.FixedTimeEquals(mac, expectedMac))
                throw new CryptographicException("Authentication failed!");

            // Extract IV and encrypted body from ciphertext
            var iv = new byte[IvBytes];
            var encryptedBody = new byte[ciphertext.Length - IvBytes];
            Buffer.BlockCopy(ciphertext, 0, iv, 0, IvBytes);
            Buffer.BlockCopy(ciphertext, IvBytes, encryptedBody, 0, encryptedBody.Length);

            using var aes = Aes.Create();
            aes.Key = aesKey;
            var plainBytes = aes.DecryptCbc(encryptedBody, iv);
            return Encoding.UTF8.GetString(plainBytes);
        }

        /// <summary>
        /// Replicates the key derivation of Rijndael256's AeKeyRing.Generate + Rijndael.GenerateKey.
        /// </summary>
        private static void DeriveKeys(string password, out byte[] aesKey, out byte[] macKey)
        {
            // AeKeyRing.Generate: SHA-512 the password → uppercase hex → split into two 64-char halves
            var sha512Bytes = SHA512.HashData(Encoding.UTF8.GetBytes(password));
            var h = BitConverter.ToString(sha512Bytes).Replace("-", "");  // 128-char uppercase hex

            var cipherKey = h.Substring(0, 64);
            var macKeyStr = h.Substring(64, 64);
            macKey = Encoding.UTF8.GetBytes(macKeyStr);  // 64 bytes

            // Rijndael.GenerateKey(cipherKey, Aes256):
            //   step1_salt = Hash.Sha512(cipherKey + cipherKey.Length)
            //   step2_salt = Pbkdf2(cipherKey, step1_salt_utf8, 10000, 64)
            //   aesKey     = Pbkdf2(cipherKey, step2_salt, 10000, 32)
            var step1SaltStr = BitConverter
                .ToString(SHA512.HashData(Encoding.UTF8.GetBytes(cipherKey + cipherKey.Length.ToString())))
                .Replace("-", "");  // 128-char uppercase hex

            var cipherKeyBytes = Encoding.UTF8.GetBytes(cipherKey);
            var step2Salt = Rfc2898DeriveBytes.Pbkdf2(
                cipherKeyBytes,
                Encoding.UTF8.GetBytes(step1SaltStr),
                Iterations, HashAlgorithmName.SHA1, 64);

            aesKey = Rfc2898DeriveBytes.Pbkdf2(
                cipherKeyBytes, step2Salt,
                Iterations, HashAlgorithmName.SHA1, AesKeyBytes);
        }
    }
}
