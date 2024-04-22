// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace DataImport.Common.Preprocessors
{
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
    }
}
