// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Runtime.CompilerServices;

public static class RuntimeAssertion
{
    public static void Assert(bool condition, [CallerFilePath] string file = null, [CallerLineNumber] int line = 0, [CallerMemberName] string member = null)
    {
        if (!condition)
            throw new Exception($"Assertion Failed in {member} @ {file}:line {line}");
    }
}
