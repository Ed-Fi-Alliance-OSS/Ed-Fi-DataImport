// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;

namespace DataImport.Web.Tests
{
    public class StubHttpPostedFileBase : FormFile
    {
        public StubHttpPostedFileBase(string fileName, string content)
            : base(new MemoryStream(Encoding.UTF8.GetBytes(content)), 0, content.Length * sizeof(char), fileName, fileName )
        {
        }
    }
}