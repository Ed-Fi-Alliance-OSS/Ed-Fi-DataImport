// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;

namespace DataImport.Common.ExtensionMethods
{
    public static class LinqExtensions
    {
        public static void Each<T>(this IEnumerable<T> items, Action<T> action)
        {
            if (items == null)
                return;

            foreach (var item in items)
                action(item);
        }

        public static void Each<T>(this IEnumerable<T> items, Action<T, int> action)
        {
            if (items == null)
                return;

            int index = 0;
            foreach (var item in items)
            {
                action(item, index);
                index++;
            }
        }
    }
}
