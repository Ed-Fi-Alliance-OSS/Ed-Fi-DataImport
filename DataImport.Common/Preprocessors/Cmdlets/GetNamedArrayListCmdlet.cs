// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections;
using System.Management.Automation;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    [Cmdlet(VerbsCommon.Get, "NamedArrayList")]
    [OutputType(typeof(ArrayList))]
    public class GetNamedArrayListCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        protected override void BeginProcessing()
        {
            WriteObject(SessionState.PSVariable.GetValue(Name));
        }
    }
}
