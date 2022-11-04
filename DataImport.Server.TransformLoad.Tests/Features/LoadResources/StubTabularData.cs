// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using DataImport.Server.TransformLoad.Features.LoadResources;

namespace DataImport.Server.TransformLoad.Tests.Features.LoadResources
{
    public class StubTabularData : ITabularData
    {
        private readonly Dictionary<string, string>[] _rows;

        public StubTabularData(params Dictionary<string, string>[] rows)
        {
            _rows = rows;
        }

        public IEnumerable<Dictionary<string, string>> GetRows() => _rows;

        public void Dispose() => Disposed = true;

        public bool Disposed { get; set; }
    }
}