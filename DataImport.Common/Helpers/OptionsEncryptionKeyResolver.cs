// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Common.Helpers
{
    public class OptionsEncryptionKeyResolver : IEncryptionKeyResolver
    {
        private readonly string _encryptionKey;

        public OptionsEncryptionKeyResolver(IEncryptionKeySettings encryptionKeySettings)
        {
            _encryptionKey = encryptionKeySettings.EncryptionKey;
        }

        public string GetEncryptionKey()
        {
            return _encryptionKey;
        }
    }
}
