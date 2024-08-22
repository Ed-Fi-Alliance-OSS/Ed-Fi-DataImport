// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Common.Preprocessors.Cmdlets;
using Microsoft.PowerShell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Management.Automation.Runspaces;
using System.Text;
using PowerShell = System.Management.Automation.PowerShell;

namespace DataImport.Common.Preprocessors
{
    /// <summary>
    /// * Note for future migration to .NET Core. *
    /// PowerShell preprocessor requires PowerShell Host assemblies to actually run PowerShell scripts.
    /// Currently System.Management.Automation.dll is taken from GAC. It is part of Windows and could be for PowerShell 5.1 or lower depending on version of Windows.
    /// The assembly reference was manually added to the proj file. When migrating to .NET Core it is recommended to use either System.Management.Automation or PowerShell.SDK NuGet package.
    /// </summary>
    public class PowerShellPreprocessorService : IPowerShellPreprocessorService
    {
        private readonly PowerShellPreprocessorOptions _psPreprocessorOptions;
        private readonly IAuthRequestWrapper _authRequestWrapper;
        private readonly IPowerShellPreprocessSettings _powerShellPreprocessSettings;

        public PowerShellPreprocessorService(IPowerShellPreprocessSettings powerShellPreprocessSettings, PowerShellPreprocessorOptions options, IAuthRequestWrapper authRequestWrapper)
        {
            _psPreprocessorOptions = options ?? throw new ArgumentNullException(nameof(options));
            _authRequestWrapper = authRequestWrapper;
            _powerShellPreprocessSettings = powerShellPreprocessSettings;
        }

        public Stream ProcessStreamWithScript(string scriptContent, Stream input, ProcessOptions options = null)
        {
            if (string.IsNullOrEmpty(scriptContent))
            {
                return input;
            }

            var outputLines = RunInPowerShell((runspace, host) =>
            {
                host.AddScript(scriptContent);

                Collection<PSObject> result;
                try
                {
                    result = host.Invoke(ReadAllLines(input));
                }
                catch (Exception x)
                {
                    throw new PowerShellProcessException(x);
                }

                if (host.Streams.Error.Any() || host.InvocationStateInfo.Reason != null)
                {
                    throw new PowerShellProcessException(host.Streams.Error.ToList(), host.InvocationStateInfo.Reason);
                }

                return result;
            }, options);

            var output = string.Join(Environment.NewLine, outputLines.Select(s => s?.ToString()));
            return new MemoryStream(Encoding.ASCII.GetBytes(output));
        }

        public void ValidateScript(string scriptContent)
        {
            if (scriptContent == null)
            {
                return;
            }

            Parser.ParseInput(scriptContent, out var tokens, out var errors);
            if (errors != null && errors.Length > 0)
            {
                throw new PowerShellValidateException(errors);
            }
        }

        public string GenerateFile(string script, ProcessOptions processOptions)
        {
            if (string.IsNullOrEmpty(script)) throw new ArgumentNullException(nameof(script));

            return RunInPowerShell((runspace, host) =>
            {
                host.AddScript(script);

                Collection<PSObject> result;
                try
                {
                    result = host.Invoke();
                }
                catch (Exception x)
                {
                    throw new PowerShellProcessException(x);
                }

                if (host.Streams.Error.Any() || host.InvocationStateInfo.Reason != null)
                {
                    throw new PowerShellProcessException(host.Streams.Error.ToList(), host.InvocationStateInfo.Reason);
                }

                return (string) result.Single().ImmediateBaseObject;
            }, processOptions);
        }

        public (PowerShell powerShell, Runspace runspace) CreatePowerShellEnvironment(ProcessOptions processOptions)
        {
            ValidateOptions(processOptions);

            var runspace = CreateRunspace(processOptions);

            try
            {
                var host = PowerShell.Create();

                try
                {
                    host.Runspace = runspace;

                    if (processOptions != null)
                    {
                        host.Streams.Error.DataAdding += processOptions.OnProcessStreamDataAdding;
                        host.Streams.Warning.DataAdding += processOptions.OnProcessStreamDataAdding;
                        host.Streams.Information.DataAdding += processOptions.OnProcessStreamDataAdding;
                        host.Streams.Debug.DataAdding += processOptions.OnProcessStreamDataAdding;
                        host.Streams.Verbose.DataAdding += processOptions.OnProcessStreamDataAdding;
                    }

                    return (host, runspace);
                }
                catch
                {
                    host.Dispose();
                    throw;
                }
            }
            catch
            {
                runspace.Dispose();
                throw;
            }
        }

        private T RunInPowerShell<T>(Func<Runspace, PowerShell, T> action, ProcessOptions options = null)
        {
            var psEnvironment = CreatePowerShellEnvironment(options);

            using (psEnvironment.runspace)
            {
                using (psEnvironment.powerShell)
                {
                    var result = action(psEnvironment.runspace, psEnvironment.powerShell);

                    psEnvironment.runspace.Close();

                    return result;
                }
            }
        }

        private Runspace CreateRunspace(ProcessOptions options = null)
        {
            InitialSessionState initialSessionState;
            if (options?.UsePowerShellWithNoRestrictions == true)
            {
                initialSessionState = InitialSessionState.CreateDefault();
                initialSessionState.LanguageMode = PSLanguageMode.FullLanguage;
            }
            else
            {
                initialSessionState = InitialSessionState.Create();
                initialSessionState.LanguageMode = PSLanguageMode.ConstrainedLanguage;
                WhitelistDefaultCommands(initialSessionState);
            }

            initialSessionState.ThrowOnRunspaceOpenError = true;

            if (_psPreprocessorOptions.Modules != null && _psPreprocessorOptions.Modules.Count > 0)
            {
                initialSessionState.ExecutionPolicy = ExecutionPolicy.Bypass;
                initialSessionState.ImportPSModule(_psPreprocessorOptions.Modules.ToArray());
            }

            if (options?.RequiresOdsConnection == true)
            {
                // Adding commands to session state must happen before creating a runspace
                initialSessionState.Commands.Add(new SessionStateCmdletEntry("Invoke-OdsApiRequest", typeof(InvokeOdsApiRequestCmdlet), null));
                initialSessionState.Commands.Add(new SessionStateCmdletEntry("Invoke-OdsApiRestMethod", typeof(InvokeOdsApiRestMethodCmdlet), null));
            }

            AddCustomCommands(initialSessionState);

            var runspace = RunspaceFactory.CreateRunspace(initialSessionState);

            try
            {
                runspace.Open();

                AddBuiltInVariables(runspace, options);
            }
            catch
            {
                runspace.Dispose();
                throw;
            }

            return runspace;
        }

        private static void ValidateOptions(ProcessOptions options)
        {
            if (options?.RequiresOdsConnection == true)
            {
                if (options.OdsConnectionSettings == null)
                    throw new ArgumentException(
                        "The RequiresOdsConnection option was set, but the OdsConnectionSettings option was not provided.",
                        nameof(options));

                if (options.OdsConnectionSettings.ApiVersion == null)
                {
                    throw new ArgumentException(
                        "ApiVersion must be set for the OdsConnectionSettings",
                        nameof(options));
                }
            }
        }

        private void AddBuiltInVariables(Runspace runspace, ProcessOptions options)
        {
            if (options?.RequiresOdsConnection == true)
            {
                // Adding variables to runspace is only possible when runspace is open.
                var odsApiTokenRetriever = new OdsApiTokenRetriever(_authRequestWrapper, options.OdsConnectionSettings, _powerShellPreprocessSettings.EncryptionKey);
                var odsAuthenticator = new OdsAuthenticator(odsApiTokenRetriever, options.OdsConnectionSettings);
                runspace.SessionStateProxy.SetVariable("OdsAuthenticator", odsAuthenticator);
                runspace.SessionStateProxy.SetVariable("ODS", odsAuthenticator.Authenticate());
            }

            if (!string.IsNullOrWhiteSpace(options?.CacheIdentifier))
            {
                runspace.SessionStateProxy.SetVariable(AgentCacheItemCmdletBase.CacheIdentifierVariableName, options.CacheIdentifier);
            }

            var customVariables = new
            {
                MapAttribute = options?.MapAttribute,
                Filename = options?.FileName,
                PreviewFlag = options?.IsDataMapPreview,
                ApiVersion = options?.OdsConnectionSettings?.ApiVersion?.Version
            };
            runspace.SessionStateProxy.PSVariable.Set(new PSVariable("DataImport", customVariables, ScopedItemOptions.Private | ScopedItemOptions.ReadOnly));
        }

        private void AddCustomCommands(InitialSessionState initialSessionState)
        {
            initialSessionState.Commands.Add(new SessionStateCmdletEntry("New-NamedArrayList", typeof(NewNamedArrayListCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-NamedArrayList", typeof(GetNamedArrayListCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry("Add-CollectionItem", typeof(AddCollectionItemCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry("Remove-CollectionItem", typeof(RemoveCollectionItemCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry("ConvertFrom-FixedWidth", typeof(ConvertFromFixedWidthCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry("New-AgentCacheItem", typeof(NewAgentCacheItemCmdlet), null));
            initialSessionState.Commands.Add(new SessionStateCmdletEntry("Get-AgentCacheItem", typeof(GetAgentCacheItemCmdlet), null));
        }

        private void WhitelistDefaultCommands(InitialSessionState initialSessionState)
        {
            if (_psPreprocessorOptions.AllowedCommands == null || _psPreprocessorOptions.AllowedCommands.Count == 0)
            {
                return;
            }

            var defaultSessionState = InitialSessionState.CreateDefault();

            foreach (var commandName in _psPreprocessorOptions.AllowedCommands)
            {
                var command = defaultSessionState.Commands[commandName];

                initialSessionState.Commands.Add(command);
            }
        }

        private static IEnumerable<string> ReadAllLines(Stream input)
        {
            using (var reader = new StreamReader(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
    }
}
