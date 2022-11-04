// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace DataImport.Common.Preprocessors
{
    public interface IPowerShellPreprocessorService
    {
        Stream ProcessStreamWithScript(string scriptContent, Stream input, ProcessOptions options = null);

        void ValidateScript(string scriptContent);

        string GenerateFile(string script, ProcessOptions processOptions);

        (PowerShell powerShell, Runspace runspace) CreatePowerShellEnvironment(ProcessOptions processOptions);
    }
}
