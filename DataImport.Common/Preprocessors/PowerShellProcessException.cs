// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Runtime.Serialization;

namespace DataImport.Common.Preprocessors
{
    [Serializable]
    public class PowerShellProcessException : AggregateException
    {
        public IEnumerable<ErrorRecord> Errors { get; }

        public PowerShellProcessException(Exception terminatingException)
            : base("Script execution failed.", terminatingException)
        {
            Errors = Enumerable.Empty<ErrorRecord>();
        }

        public PowerShellProcessException(IReadOnlyCollection<ErrorRecord> errors, Exception terminatingException)
            : base("Script execution failed.", CombineExceptions(errors, terminatingException))
        {
            Errors = errors;
        }

        private static IEnumerable<Exception> CombineExceptions(IReadOnlyCollection<ErrorRecord> errors, Exception terminatingException)
        {
            var listErrors = new List<Exception>(errors.Select(e => e.Exception));
            if (terminatingException != null)
            {
                listErrors.Add(new Exception("The PowerShell host has terminated unexpectedly.", terminatingException));
            }

            return listErrors;
        }

        public PowerShellProcessException(string message)
            : base(message) { }

        public PowerShellProcessException(string message, params Exception[] innerExceptions)
            : base(message, innerExceptions) { }
#pragma warning disable S1133 // Deprecated code should be removed
        [Obsolete("Obsolete for NET 8", DiagnosticId = "SYSLIB0051")]
        protected PowerShellProcessException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        [Obsolete("Obsolete for NET 8", DiagnosticId = "SYSLIB0051")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Errors), Errors);
        }
#pragma warning disable S1133 // Deprecated code should be removed
    }
}
