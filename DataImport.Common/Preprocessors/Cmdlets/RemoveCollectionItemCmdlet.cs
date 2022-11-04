// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections;
using System.Management.Automation;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    [Cmdlet(VerbsCommon.Remove, "CollectionItem")]
    public class RemoveCollectionItemCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public string ArrayListName { get; set; }

        [Parameter(Mandatory = true)]
        public object Item { get; set; }

        protected override void BeginProcessing()
        {
            ArrayList arrayList = (ArrayList) SessionState.PSVariable.GetValue(ArrayListName, new ArrayList());
            arrayList.Remove(Item);
        }
    }
}
