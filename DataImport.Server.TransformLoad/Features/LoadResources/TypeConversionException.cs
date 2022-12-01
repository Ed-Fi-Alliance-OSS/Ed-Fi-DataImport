// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Text;
using DataImport.Models;

namespace DataImport.Server.TransformLoad.Features.LoadResources
{
    public class TypeConversionException : Exception
    {
        public TypeConversionException(DataMapper node, string expectedType, bool typeIsUnsupported = false)
        : base(BuildMessage(node, expectedType, typeIsUnsupported))
        {
        }

        private static string BuildMessage(DataMapper node, string expectedType, bool typeIsUnsupported = false)
        {
            var message = new StringBuilder();

            message.Append(!string.IsNullOrWhiteSpace(node.SourceColumn)
                ? $"Column \"{node.SourceColumn}\" contains a value for property \"{node.Name}\" which cannot be "
                : $"Static value for property \"{node.Name}\" cannot be ");

            message.Append($"converted to{(typeIsUnsupported ? " unsupported " : " ")}type \"{expectedType}\".");

            return message.ToString();
        }
    }
}
