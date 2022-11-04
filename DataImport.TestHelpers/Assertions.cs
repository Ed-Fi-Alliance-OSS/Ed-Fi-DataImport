// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DataImport.TestHelpers
{
    using static TestHelpers;

    public static class Assertions
    {
        public static void ShouldMatch<T>(this IEnumerable<T> actual, params T[] expected)
            => actual.ToArray().ShouldMatch(expected);

        public static void ShouldMatch<T>(this T actual, T expected)
        {
            if (Json(expected) != Json(actual))
                throw new MatchException(expected, actual);
        }

        public static void ShouldMatch(this JToken actual, string expected)
        {
            var expectedToken = JToken.Parse(expected);

            if (expectedToken.ToString() != actual.ToString())
                throw new MatchException(expectedToken, actual);
        }
    }
}
