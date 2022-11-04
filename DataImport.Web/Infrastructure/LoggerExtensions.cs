// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;

namespace DataImport.Web.Infrastructure
{
    internal static class LoggerExtensions
    {
        public static void Added<T>(this ILogger logger, T entity, Func<T, string> getName) where T : Entity =>
            InfoCrud(logger, entity, getName);

        public static void Modified<T>(this ILogger logger, T entity, Func<T, string> getName)
            where T : Entity =>
            InfoCrud(logger, entity, getName);

        public static void Deleted<T>(this ILogger logger, T entity, Func<T, string> getName)
            where T : Entity =>
            InfoCrud(logger, entity, getName);

        public static void Archived<T>(this ILogger logger, T entity, Func<T, string> getName)
            where T : Entity =>
            InfoCrud(logger, entity, getName);

        private static void InfoCrud<T>(this ILogger logger, T entity, Func<T, string> getName,
            [CallerMemberName] string action = null)
            where T : Entity
        {
            var entityType = entity.GetType().Name;
            var entityName = getName(entity);

            var message = $"{action} {entityType}";
            logger.LogInformation(message + " {name} (Id {id}).", entityName, entity.Id);
        }
    }
}
