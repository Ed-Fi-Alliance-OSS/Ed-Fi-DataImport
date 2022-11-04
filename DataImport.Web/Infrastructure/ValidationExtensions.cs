// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace DataImport.Web.Infrastructure
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptionsConditions<T, TProperty> SafeCustom<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, ILogger logger, Action<TProperty, ValidationContext<T>> action)
        {
            return ruleBuilder.Custom((command, context) =>
            {
                try
                {
                    action(command, context);
                }
                catch (Exception exception)
                {
                    context.AddFailure("A validation rule encountered an unexpected error. Check the Application Log for troubleshooting information.");
                    logger.LogError(exception, "A validation rule encountered an unexpected error.");
                }
            });
        }

        public static void AddFailures<T>(this ValidationContext<T> context, ValidationResult result)
        {
            result.Errors.Select(x => x.ErrorMessage).Each(context.AddFailure);
        }
    }
}