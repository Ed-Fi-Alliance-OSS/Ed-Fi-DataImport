// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Management.Automation.Language;

namespace DataImport.Common.Preprocessors
{
    public class PowerShellValidateException : Exception
    {
        public IEnumerable<ParseError> ParseErrors { get; }

        public PowerShellValidateException(ParseError[] parseErrors)
            : base("Validation failed for PowerShell script.")
        {
            ParseErrors = parseErrors;
        }

        public PowerShellValidateException(string message)
            : base(message) { }
    }
}
