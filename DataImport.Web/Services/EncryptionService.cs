// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.Extensions.Logging;
using System;
using static DataImport.Common.Encryption;

namespace DataImport.Web.Services
{
    public interface IEncryptionService
    {
        bool TryEncrypt(string plainText, string encryptionKey, out string encryptedText);
        bool TryDecrypt(string encryptedText, string encryptionKey, out string decryptedText);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly ILogger _logger;

        public EncryptionService(ILogger<EncryptionService> logger)
        {
            _logger = logger;
        }

        public bool TryEncrypt(string plainText, string encryptionKey, out string encryptedText)
        {
            encryptedText = string.Empty;
            if (string.IsNullOrEmpty(encryptionKey))
            {
                _logger.LogError("Encryption key can not be empty");
                return false;
            }

            try
            {
                encryptedText = Encrypt(plainText, encryptionKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Provided encryption key is not valid.");
            }

            return false;
        }

        public bool TryDecrypt(string encryptedText, string encryptionKey, out string decryptedText)
        {
            decryptedText = string.Empty;
            if (string.IsNullOrEmpty(encryptionKey))
            {
                _logger.LogError("Encryption key can not be empty");
                return false;
            }

            try
            {
                decryptedText = Decrypt(encryptedText, encryptionKey);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Provided encryption key is not valid.");
            }

            return false;
        }
    }
}
