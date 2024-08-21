// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.Helpers;
using DataImport.Common.Preprocessors;

namespace DataImport.Server.TransformLoad
{
    public class AppSettings : IFileSettings, IPowerShellPreprocessSettings, IEncryptionKeySettings
    {
        public bool AllowTestCertificates { get; set; }
        public string EncryptionKey { get; set; }
        public string FileMode { get; set; }
        public string ShareName { get; set; }
        public bool UsePowerShellWithNoRestrictions { get; set; }
        public string MinimumLevelIngestionLog { get; set; }
        public bool IgnoresCertificateErrors { get; set; } = false;
        public bool UseBasicAuthentication { get; set; } = false;
    }

    public class ConcurrencySettings
    {
        public bool LimitConcurrentApiPosts { get; set; }
        public int MaxConcurrentApiPosts { get; set; }
    }
}
