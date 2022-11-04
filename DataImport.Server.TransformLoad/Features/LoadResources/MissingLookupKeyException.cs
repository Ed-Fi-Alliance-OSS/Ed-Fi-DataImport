// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public class MissingLookupKeyException : Exception
    {
        public MissingLookupKeyException(string columnName, string lookupName)
            : base($"Column \"{columnName}\" contains a value which is not defined by Lookup \"{lookupName}\".")
        {
        }
    }
}