// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public sealed class CsvTabularData : ITabularData
    {
        private readonly CsvReader _reader;

        public CsvTabularData(string filePath)
        {
            // The caller is responsible for disposing of this instance.
            // When disposed, CsvReader will dispose the StreamReader.

            _reader = new CsvReader(new StreamReader(filePath), CultureInfo.InvariantCulture);
        }

        public CsvTabularData(Stream stream)
        {
            _reader = new CsvReader(new StreamReader(stream), CultureInfo.InvariantCulture);
        }

        public IEnumerable<Dictionary<string, string>> GetRows()
        {
            _reader.Read();
            _reader.ReadHeader();

            var headers = _reader.HeaderRecord;

            while (_reader.Read())
                yield return headers.ToDictionary(singleHeader => singleHeader, singleHeader => _reader[(string)singleHeader]);
        }

        public void Dispose()
            => _reader.Dispose();
    }
}