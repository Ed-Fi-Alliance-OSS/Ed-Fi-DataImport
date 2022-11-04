// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DataImport.Common.Preprocessors
{
    public interface IExternalPreprocessorService
    {
        Stream ProcessStreamWithExternalProcess(string commandPath, string arguments, Stream input);
        string GenerateFile(string commandPath, string arguments);
    }

    public class ExternalPreprocessorService : IExternalPreprocessorService
    {
        private readonly ILogger<ExternalPreprocessorService> _logger;
        private readonly ExternalPreprocessorOptions _options;

        public ExternalPreprocessorService(ILogger<ExternalPreprocessorService> logger, IOptions<ExternalPreprocessorOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Stream ProcessStreamWithExternalProcess(string commandPath, string arguments, Stream input)
        {
            if (!_options.Enabled)
            {
                _logger.LogWarning("Skipping execution: External Preprocessors are disabled");
                return input;
            }

            var output = ExecuteAndCollectOutput(commandPath, arguments, process =>
            {
                foreach (var line in ReadAllLines(input))
                {
                    process.StandardInput.WriteLine(line);
                }
                process.StandardInput.WriteLine("DATAIMPORT_END_OF_FILE");
            });

            return new MemoryStream(Encoding.ASCII.GetBytes(output));
        }

        public string GenerateFile(string commandPath, string arguments)
        {
            if (!_options.Enabled)
            {
                _logger.LogWarning("Skipping execution: External Preprocessors are disabled");
                throw new ExternalPreprocessorException(commandPath, new Exception("External preprocessing is disabled"));
            }

            if (string.IsNullOrEmpty(commandPath)) throw new ArgumentNullException(nameof(commandPath));
            var output = ExecuteAndCollectOutput(commandPath, arguments, _ => { });
            return output.Trim();
        }

        private string ExecuteAndCollectOutput(string commandPath, string arguments, Action<Process> interact)
        {
            var outputLines = new Collection<string>();
            var errors = new Collection<string>();
            var startInfo = new ProcessStartInfo
            {
                FileName = commandPath,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            void WriteToCollection(DataReceivedEventArgs eventArgs, Collection<string> collection)
            {
                if (!string.IsNullOrEmpty(eventArgs.Data))
                    collection.Add(eventArgs.Data.Trim());
            }

            try
            {
                using var process = Process.Start(startInfo);

                var processName = process.ProcessName;
                _logger.LogInformation("External preprocess {ProcessName} started", processName);

                process.OutputDataReceived += (_, e) => { WriteToCollection(e, outputLines); };
                process.BeginOutputReadLine();
                process.ErrorDataReceived += (_, e) => { WriteToCollection(e, errors); };
                process.BeginErrorReadLine();

                interact(process);

                process.WaitForExit(_options.TimeOutInMilliseconds);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) //process properties after exit are only available on Windows
                {
                    _logger.LogInformation("External preprocess {ProcessName} exited in {Time}ms", processName, process.TotalProcessorTime.Milliseconds);
                    if (process.ExitCode != 0)
                        throw new ExternalPreprocessorException(processName, errors, process.ExitCode);
                }
                else
                {
                    _logger.LogInformation("External preprocess {ProcessName} has exited", processName);
                }

                if (errors.Count > 0)
                    throw new ExternalPreprocessorException(processName, errors);

                return string.Join(Environment.NewLine, outputLines);
            }
            catch (ExternalPreprocessorException) { throw; }
            catch (InvalidOperationException e) { throw new ExternalPreprocessorException(commandPath, e); }
            catch (Win32Exception e) { throw new ExternalPreprocessorException(commandPath, e); }
            catch (Exception e) { throw new ExternalPreprocessorException(commandPath, e); }
        }

        private static IEnumerable<string> ReadAllLines(Stream input)
        {
            using var reader = new StreamReader(input);

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                yield return line;
            }
        }
    }
}
