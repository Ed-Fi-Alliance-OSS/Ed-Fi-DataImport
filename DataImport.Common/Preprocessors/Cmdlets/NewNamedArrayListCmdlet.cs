// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections;
using System.Management.Automation;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    [Cmdlet(VerbsCommon.New, "NamedArrayList")]
    public class NewNamedArrayListCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = false)]
        public ICollection Collection { get; set; }

        [Parameter(Mandatory = false)]
        public int Capacity { get; set; }

        [Parameter(Mandatory = true, Position = 0)]
        public string Name { get; set; }

        protected override void BeginProcessing()
        {
            ArrayList arrayList;

            if (Collection != null)
            {
                arrayList = new ArrayList(Collection);
                if (Capacity > Collection.Count)
                {
                    arrayList.Capacity = Capacity;
                }
            }
            else if (Capacity > 0)
            {
                arrayList = new ArrayList(Capacity);
            }
            else
            {
                arrayList = new ArrayList();
            }

            SessionState.PSVariable.Set(Name, arrayList);
            WriteObject(arrayList);
        }
    }
}
