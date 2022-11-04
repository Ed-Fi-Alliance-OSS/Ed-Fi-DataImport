// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Net;

namespace DataImport.Common.Preprocessors.Cmdlets
{
    public abstract class InvokeOdsApiCmdletBase : PSCmdlet
    {
        /// <summary>
        /// Gets or sets a request path to append to the base url. For example, /ed-fi/studentEducationOrganizationAssociations?limit=1
        /// </summary>
        [Parameter(Mandatory = true, Position = 0)]
        public string RequestPath { get; set; }

        [Parameter(Mandatory = false, Position = 1)]
        public int RetryAttempts { get; set; }

        protected abstract ScriptBlock GetScriptBlock();

        protected override void BeginProcessing()
        {
            int retryAttempts = RetryAttempts > 0 ? RetryAttempts : 3;
            var currentAttempt = 0;

            while (retryAttempts > currentAttempt)
            {
                currentAttempt++;
                try
                {
                    var authenticationResult = GetOdsAuthenticationResult();
                    string url = Helpers.UrlUtility.CombineUri(authenticationResult.BaseUrl.AbsoluteUri, RequestPath);

                    var scriptBlock = GetScriptBlock();

                    Collection<PSObject> invokeResult = scriptBlock.Invoke(url, authenticationResult.AccessToken);
                    WriteObject(invokeResult, enumerateCollection: true);
                    return;
                }
                catch (CmdletInvocationException e)
                {
                    if (!(e.InnerException is WebException webException) || webException.Response == null || ((HttpWebResponse) webException.Response).StatusCode != HttpStatusCode.Unauthorized)
                    {
                        throw;
                    }

                    SessionState.PSVariable.Set(new PSVariable("ODS", null, ScopedItemOptions.Private));
                    WriteVerbose("The ODS / API returned an Unauthorized response. This can happen, for instance, when the access token has expired. Attempting to obtain a new access token and retry the request...");
                }
            }
        }

        private OdsAuthenticationResult GetOdsAuthenticationResult()
        {
            // ODS variable can be set outside of the cmdlet to support debugging PowerShell scripts in code editors such as VS Code.
            // or it can be set by the cmdlet in case of running the cmdlet from the PowerShell Preprocessor.
            var odsVariable = GetVariableValue("ODS");
            if (odsVariable == null)
            {
                return Authenticate();
            }

            if (odsVariable is OdsAuthenticationResult odsAuthenticationResult)
            {
                return odsAuthenticationResult;
            }

            if (odsVariable is PSObject psObject && psObject.BaseObject is OdsAuthenticationResult wrappedAuthenticationResult)
            {
                return wrappedAuthenticationResult;
            }

            throw new InvalidOperationException("Cannot get authentication result. Either set the 'ODS' variable explicitly or set 'OdsAuthenticator'.");
        }

        private OdsAuthenticationResult Authenticate()
        {
            var authenticator = GetVariableValue("OdsAuthenticator") as OdsAuthenticator;
            if (authenticator == null)
            {
                throw new InvalidOperationException("The 'OdsAuthenticator' variable is not set. Cannot proceed with obtaining a new access token.");
            }

            WriteVerbose("Obtaining a new access token...");
            var authenticationResult = authenticator.Authenticate();
            SessionState.PSVariable.Set(new PSVariable("ODS", authenticationResult, ScopedItemOptions.Private));

            return authenticationResult;
        }
    }
}
