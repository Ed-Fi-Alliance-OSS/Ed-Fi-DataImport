// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DataImport.Common.Preprocessors
{
    [Serializable]
    public class ExternalPreprocessorException : AggregateException
    {
        public string ProcessName { get; }
        public IEnumerable<string> Errors { get; }

        public ExternalPreprocessorException(string processName, IReadOnlyCollection<string> errors, int exitCode)
            : base($"Process {processName} ended with non-zero exit code: {exitCode}", CombineExceptions(errors, null))
        {
            Errors = errors;
            ProcessName = processName;
        }

        public ExternalPreprocessorException(string processName, IReadOnlyCollection<string> errors)
            : base($"Process {processName} execution failed.", CombineExceptions(errors, null))
        {
            Errors = errors;
            ProcessName = processName;
        }

        public ExternalPreprocessorException(string processName, Exception terminatingException)
            : base($"Process {processName} execution failed.", terminatingException)
        {
            Errors = Enumerable.Empty<string>();
            ProcessName = processName;
        }

        public ExternalPreprocessorException(string processName, IReadOnlyCollection<string> errors, Exception terminatingException)
            : base($"Process {processName} execution failed.", CombineExceptions(errors, terminatingException))
        {
            Errors = errors;
            ProcessName = processName;
        }

        private static IEnumerable<Exception> CombineExceptions(IReadOnlyCollection<string> errors, Exception terminatingException)
        {
            var listErrors = new List<Exception>(errors.Select(e => new Exception(e)));
            if (terminatingException != null)
            {
                listErrors.Add(new Exception("The process terminated unexpectedly.", terminatingException));
            }

            return listErrors;
        }


        public ExternalPreprocessorException(string message)
            : base(message) { }

        public ExternalPreprocessorException(string message, params Exception[] innerExceptions)
            : base(message, innerExceptions) { }

#pragma warning disable S1133 // Deprecated code should be removed
        [Obsolete("Obsolete for NET 8", DiagnosticId = "SYSLIB0051")]
        protected ExternalPreprocessorException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        [Obsolete("Obsolete for NET 8", DiagnosticId = "SYSLIB0051")]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(ProcessName), ProcessName);
            info.AddValue(nameof(Errors), Errors);
        }
#pragma warning disable S1133 // Deprecated code should be removed
    }
}
