// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Web.Helpers
{
    public struct Constants
    {
        public const string AgentDecryptionError =
            "An error occurred while trying to decrypt the password. This may be caused by an invalid encryption key. Please contact your administrator for configuring the correct encryption key.";
        public const string AgentEncryptionError =
            "An error occurred while trying to encrypt the password. This may be caused by an invalid encryption key. Please contact your administrator for configuring the correct encryption key.";
        public const string ConfigDecryptionError = "An error occurred while trying to decrypt the key and secret. This may be caused by an invalid encryption key. Please contact your administrator for configuring the correct encryption key.";
        public const string ConfigEncryptionError = "An error occurred while trying to encrypt the key and secret. This may be caused by an invalid encryption key. Please contact your administrator for configuring the correct encryption key.";
    }
}