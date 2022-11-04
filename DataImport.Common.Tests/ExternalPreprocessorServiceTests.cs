// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DataImport.Common.Preprocessors;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Common.Tests
{
    [TestFixture]
    public class ExternalPreprocessorServiceTests
    {
        private ExternalPreprocessorService _service;
        private string _pythonExe;

        [SetUp]
        public void Setup()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .AddEnvironmentVariables()
                .Build();

            _pythonExe = configuration.GetValue<string>("PythonExecutableLocation");

            if (!File.Exists(_pythonExe))
                throw new FileNotFoundException("Python executable not found. Run 'build.cmd SetUp' to download.");

            var logger = A.Fake<ILogger<ExternalPreprocessorService>>();
            var options = new ExternalPreprocessorOptions
            {
                Enabled = true,
                TimeOutInMilliseconds = 5000,
            };
            _service = new ExternalPreprocessorService(logger, Options.Create(options));
        }

        [Test]
        public void ProcessShouldReturnScriptOutputContentInReturnedStream()
        {
            var script = ResolvePythonScriptPath("PrintHello.py");
            var input = new MemoryStream();
            var output = _service.ProcessStreamWithExternalProcess(_pythonExe, script, input);
            var outputContent = new StreamReader(output).ReadToEnd();

            outputContent.ShouldBe("hello");
        }

        [Test]
        public void ShouldReturnProcessedInputStream()
        {
            var script = ResolvePythonScriptPath("PrintInput.py");
            var input = new MemoryStream(Encoding.ASCII.GetBytes(string.Join(Environment.NewLine, "abc", "DEF", "ghi")));
            var output = _service.ProcessStreamWithExternalProcess(_pythonExe, script, input);
            var outputContent = new StreamReader(output).ReadToEnd().Trim().Split(Environment.NewLine);

            outputContent[0].ShouldBe("abc");
            outputContent[1].ShouldBe("DEF");
            outputContent[2].ShouldBe("ghi");
        }

        [Test]
        public void ShouldReturnOutputFileName()
        {
            var script = ResolvePythonScriptPath("PrintHello.py");
            var output = _service.GenerateFile(_pythonExe, script);

            output.ShouldBe("hello");
        }

        [Test]
        public void ShouldThrowWhenNonZeroExitCodeReturned()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("Skipping: Test is only valid on Windows");
                return;
            }

            var script = ResolvePythonScriptPath("ExitWith1.py");
            var input = new MemoryStream();

            var exception = Should.Throw<ExternalPreprocessorException>(() => _service.ProcessStreamWithExternalProcess(_pythonExe, script, input));
            exception.Message.ShouldContain("python");
            exception.Message.ShouldContain("non-zero exit code", Case.Insensitive);
            exception.Message.ShouldContain("1"); // exit code from script
        }

        [Test]
        public void ShouldThrowWhenErrorsAreOutput()
        {
            var script = ResolvePythonScriptPath("WritesErrors.py");
            var input = new MemoryStream();

            var exception = Should.Throw<ExternalPreprocessorException>(() => _service.ProcessStreamWithExternalProcess(_pythonExe, script, input));
            exception.Message.ShouldContain("python");
            var innerMessages = exception.InnerExceptions.Select(e => e.Message).ToList();
            innerMessages.ShouldContain("Start with an error");
            innerMessages.ShouldContain("Here's an in-between error");
            innerMessages.ShouldContain("This is the last error printed");
        }

        [Test]
        public void ShouldSkipExecutionWhenDisabled()
        {
            var printHello = ResolvePythonScriptPath("PrintHello.py");
            var input = new MemoryStream(Encoding.ASCII.GetBytes(string.Join(Environment.NewLine, "abc", "DEF", "ghi")));

            var output = GetDisabledService().ProcessStreamWithExternalProcess(_pythonExe, printHello, input);

            var outputContent = new StreamReader(output).ReadToEnd().Trim().Split(Environment.NewLine);
            outputContent[0].ShouldBe("abc");
            outputContent[1].ShouldBe("DEF");
            outputContent[2].ShouldBe("ghi");
        }

        [Test]
        public void ShouldThrowWhenGeneratingAndDisabled()
        {
            var printHello = ResolvePythonScriptPath("PrintHello.py");

            Should.Throw<ExternalPreprocessorException>(() =>
                GetDisabledService().GenerateFile(_pythonExe, printHello));
        }

        private ExternalPreprocessorService GetDisabledService()
        {
            var logger = A.Fake<ILogger<ExternalPreprocessorService>>();
            var options = new ExternalPreprocessorOptions
            {
                Enabled = false,
                TimeOutInMilliseconds = 5000,
            };

            return _service = new ExternalPreprocessorService(logger, Options.Create(options));
        }

        private string ResolvePythonScriptPath(string scriptName)
        {
            var path = Path.Combine("PythonPreprocessors", scriptName);
            if (!File.Exists(path))
                throw new FileNotFoundException("Script not found. Make sure the name is correct and the file is set to Copy to Output Directory Always.");

            return path;
        }
    }
}
