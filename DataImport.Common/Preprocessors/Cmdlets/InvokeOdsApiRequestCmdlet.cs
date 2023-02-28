// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Management.Automation;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    [Cmdlet("Invoke", "OdsApiRequest")]
    public class InvokeOdsApiRequestCmdlet : InvokeOdsApiCmdletBase
    {
        protected override ScriptBlock GetScriptBlock()
        {
            return ScriptBlock.Create(@"
                return Invoke-WebRequest -Uri $args[0] -Headers @{""Authorization"" = ""bearer $($args[1])""} -UseBasicParsing
            ");
        }
    }
}
