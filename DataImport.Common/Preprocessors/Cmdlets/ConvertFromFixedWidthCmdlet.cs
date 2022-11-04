// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections;
using System.Management.Automation;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    [Cmdlet("ConvertFrom", "FixedWidth")]
    public class ConvertFromFixedWidthCmdlet : Cmdlet
    {
        [Parameter(Mandatory = true)]
        public string FixedWidthString { get; set; }

        [Parameter(Mandatory = true)]
        public ICollection FieldMap { get; set; }

        [Parameter(Position = 2)]
        public SwitchParameter NoTrim { get; set; } = false;

        protected override void BeginProcessing()
        {
            if (string.IsNullOrEmpty(FixedWidthString))
            {
                WriteObject(new string[] { });
                return;
            }

            var normalizedMap = NormalizeMap(FieldMap, FixedWidthString.Length);

            var output = new string[FieldMap.Count];

            for (var index = 0; index < normalizedMap.Length; index++)
            {
                var fieldMapEntry = normalizedMap[index];

                output[index] = FixedWidthString.Substring(fieldMapEntry[0], fieldMapEntry[1]);
                if (!NoTrim)
                {
                    output[index] = output[index].Trim();
                }
            }

            WriteObject(output);
        }

        protected static int[][] NormalizeMap(ICollection fieldMap, int stringLength)
        {
            int[][] normalizedMapping = new int[fieldMap.Count][];

            int i = 0;

            foreach (var fieldMapEntry in fieldMap)
            {
                normalizedMapping[i] = new int[2];
                if (fieldMapEntry is int startPosition)
                {
                    normalizedMapping[i][0] = startPosition;
                    if (i > 0 && normalizedMapping[i - 1][1] == 0)
                    {
                        normalizedMapping[i - 1][1] = startPosition - normalizedMapping[i - 1][0];
                    }
                }
                else if (fieldMapEntry is ICollection startEndMap)
                {
                    if (startEndMap.Count != 2)
                    {
                        throw new InvalidOperationException($"Invalid boundaries for map at index {i}");
                    }

                    normalizedMapping[i] = new int[2];
                    int mapIndex = 0;
                    foreach (var map in startEndMap)
                    {
                        normalizedMapping[i][mapIndex] = (int) map;
                        mapIndex++;
                    }
                }
                else
                {
                    throw new NotSupportedException("Field map item must be either integer or an array of two numbers");
                }

                i++;
            }

            if (normalizedMapping[i - 1][1] == 0)
            {
                normalizedMapping[i - 1][1] = stringLength - normalizedMapping[i - 1][0];
            }

            return normalizedMapping;
        }
    }
}
