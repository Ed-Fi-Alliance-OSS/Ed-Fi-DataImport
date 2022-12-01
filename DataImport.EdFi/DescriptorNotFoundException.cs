// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace DataImport.EdFi
{
    public class DescriptorNotFoundException : Exception
    {
        public DescriptorNotFoundException(Exception innerException)
            : base("An exception was thrown while attempting to access the descriptors in the ODS API. " +
                   "See the inner exception for details.", innerException)
        {
        }
    }
}
