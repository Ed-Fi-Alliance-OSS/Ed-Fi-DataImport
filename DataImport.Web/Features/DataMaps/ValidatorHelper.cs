// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using FluentValidation;
using System;
using System.Linq;

namespace DataImport.Web.Features.DataMaps
{
    public class ValidatorHelper
    {
        public static bool BePopulatedIfRequiredByPreprocessor<T>(int? preprocessorId, string attribute, ValidationContext<T> context, DataImportDbContext dbContext)
        {
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));

            if (!preprocessorId.HasValue)
            {
                return true;
            }

            var preprocessor = dbContext.Scripts.Single(x => x.Id == preprocessorId.Value);
            if (preprocessor.HasAttribute && string.IsNullOrWhiteSpace(attribute))
            {
                context.MessageFormatter.AppendArgument("PreprocessorName", preprocessor.Name);
                return false;
            }

            return true;
        }
    }
}
