// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Helpers;
using DataImport.Common.Preprocessors;
using DataImport.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using File = System.IO.File;


namespace DataImport.Common.Tests
{
    [TestFixture, Category("PowerShellTests")]
    public class PowerShellPreprocessorServiceTests
    {
        private class PowerShellPreprocessSettings : IPowerShellPreprocessSettings
        {
            public string EncryptionKey { get; set; }
            public bool UsePowerShellWithNoRestrictions { get; set; }
        }

        private PowerShellPreprocessorService _service;
        private PowerShellPreprocessorOptions _powerShellPreprocessorOptions;

        [SetUp]
        public void Setup()
        {
            _powerShellPreprocessorOptions = new PowerShellPreprocessorOptions();

            var oAuthRequestWrapper = A.Fake<IOAuthRequestWrapper>();
            A.CallTo(() => oAuthRequestWrapper.GetAccessCode(null, null)).WithAnyArguments().Returns("fake token");
            A.CallTo(() => oAuthRequestWrapper.GetBearerToken(null, null)).WithAnyArguments().Returns("fake token");
            A.CallTo(() => oAuthRequestWrapper.GetBearerToken(null, null, null)).WithAnyArguments().Returns("fake token");

            var powerShellPreprocessSettings = new PowerShellPreprocessSettings { EncryptionKey = Guid.NewGuid().ToString() };
            _service = new PowerShellPreprocessorService(powerShellPreprocessSettings, _powerShellPreprocessorOptions, oAuthRequestWrapper);
        }

        [Test]
        public void ProcessReturnsScriptOutputContentInReturnedStream()
        {
            var scriptContent = "Write-Output 'hello'";
            var input = new MemoryStream();
            var output = _service.ProcessStreamWithScript(scriptContent, input);
            var outputContent = new StreamReader(output).ReadToEnd();

            outputContent.ShouldBe("hello");
        }

        [TestCase("", Description = "ProcessReturnsInputStreamWhenScriptIsNull")]
        [TestCase(null, Description = "ProcessReturnsInputStreamWhenScriptIsEmpty")]
        public void ShouldReturnInputStreamWhenScriptIsNullOrEmpty(string scriptContent)
        {
            var input = new MemoryStream(Encoding.ASCII.GetBytes(string.Join(Environment.NewLine, "abc", "DEF", "ghi")));

            var output = _service.ProcessStreamWithScript(scriptContent, input);

            output.ShouldBe(input);

            Assert.AreSame(input, output);
        }

        [Test]
        public void ShouldTransformInputStreamIntoOutputStream()
        {
            var scriptContent = @"
                Process {
                    Write-Output $_.ToUpper()
                    Write-Output $_.ToLower()
                }
            ";

            var input = new MemoryStream(Encoding.ASCII.GetBytes(string.Join(Environment.NewLine, "abc", "DEF", "ghi")));
            var output = _service.ProcessStreamWithScript(scriptContent, input);
            var outputContent = new StreamReader(output).ReadToEnd();

            outputContent.ShouldBe(string.Join(Environment.NewLine, "ABC", "abc", "DEF", "def", "GHI", "ghi"));
        }

        [Test]
        public void ShouldProvideInputStreamContentToScript()
        {
            var scriptContent = @"
                param(
                    [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
                    [string] $InputContent
                )
                Write-Output $InputContent
            ";

            var input = new MemoryStream(Encoding.ASCII.GetBytes("hello"));
            var output = _service.ProcessStreamWithScript(scriptContent, input);
            var outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe("hello");
        }

        [TestCase("Process { Write-Output $_", Description = "ProcessThrowsExceptionForCompileError")]
        [TestCase("$null.Split(',')", Description = "ProcessThrowsExceptionForNullReferenceException")]
        [TestCase("Invoke-NonExistentCommand", Description = "ProcessThrowsExceptionForTerminatingError")]
        [TestCase("dir NonExistentFolder", Description = "ProcessThrowsExceptionForNonTerminatingError")]
        [TestCase("Write-Error \"I died!\"", Description = "ProcessThrowsExceptionWhenWritingToErrorStream")]
        [TestCase("throw \"I died!\"", Description = "ProcessThrowsExceptionForThrownException")]
        public void ProcessThrowsExceptionsForBadScripts(string scriptContent)
        {
            var input = new MemoryStream();
            Exception exception = null;
            try
            {
                _service.ProcessStreamWithScript(scriptContent, input);
            }
            catch (Exception x)
            {
                exception = x;
            }

            exception.ShouldNotBeNull();
        }

        [Test]
        public void ProcessReturnsMessagesLoggedByScripts()
        {
            var scriptContent = @"
                Process {
                    Write-Error       ""ERROR""
                    Write-Warning     ""WARNING (#1)""
                    Write-Warning     ""WARNING (#2)""
                    Write-Information ""INFO""
                    Write-Debug       ""DEBUG""
                    Write-Verbose     ""VERBOSE""
                    Write-Progress    ""PROGRESS""
                }
            ";

            var input = new MemoryStream(Encoding.ASCII.GetBytes("hello"));

            var handler = A.Fake<Action<LogLevel, string>>(x => x.Strict());
            //
            // PS Host drops Debug and Verbose by default
            // Progress is ignored by the preprocessor service
            //
            A.CallTo(() => handler(LogLevel.Error, "ERROR")).DoesNothing().Once();
            A.CallTo(() => handler(LogLevel.Warning, "WARNING (#1)")).DoesNothing().Once();
            A.CallTo(() => handler(LogLevel.Warning, "WARNING (#2)")).DoesNothing().Once();
            A.CallTo(() => handler(LogLevel.Information, "INFO")).DoesNothing().Once();

            var options = new ProcessOptions();
            options.ProcessMessageLogged += (sender, e) => handler(e.Level, e.Message);

            try
            {
                _service.ProcessStreamWithScript(scriptContent, input, options);
            }
            catch (Exception e)
            {
                // Since we're doing Write-Error, we're going to get an exception ... don't really care about it though
                Debug.WriteLine(e.ToString());
            }

            A.CallTo(() => handler(LogLevel.Error, "ERROR")).MustHaveHappened();
            A.CallTo(() => handler(LogLevel.Warning, "WARNING (#1)")).MustHaveHappened();
            A.CallTo(() => handler(LogLevel.Warning, "WARNING (#2)")).MustHaveHappened();
            A.CallTo(() => handler(LogLevel.Information, "INFO")).MustHaveHappened();
        }

        [Test]
        public void ValidateThrowsExceptionForCompileError()
        {
            const string ScriptContent = "Process { Write-Output $_";

            Should.Throw<PowerShellValidateException>(() => _service.ValidateScript(ScriptContent));
        }

        [TestCase("Process { Write-Output $_ }", Description = "ValidateSucceedsForValidScript")]
        [TestCase("Invoke-NonExistentCommand", Description = "ValidateSucceedsForValidScriptWithNonExistentCommand")]
        [TestCase("", Description = "ValidateSucceedsForEmptyScript")]
        [TestCase(null, Description = "ValidateSucceedsForNullScript")]
        public void ValidateSucceedsForValidScripts(string scriptContent)
        {
            _service.ValidateScript(scriptContent);
        }

        [Test]
        public void ShouldOnlyAllowRunningDefaultCommands()
        {
            _powerShellPreprocessorOptions.AllowedCommands.ShouldNotContain("Stop-Service");

            string scriptContent = "Get-Help Get-Service";

            var exception = Should.Throw<PowerShellProcessException>(() => _service.ProcessStreamWithScript(scriptContent, new MemoryStream()));
            exception.ToString().ShouldContain("The term 'Get-Help' is not recognized as a name of a cmdlet, function, script file, or executable program.");
        }

        [Test]
        public void ShouldMakeInvokeOdsRequestAvailableForApiSpecificPreprocessors()
        {
            var scriptContent = "(Invoke-OdsApiRequest -RequestPath \"?app=unittest\").StatusCode";
            Exception exception = null;
            var options = new ProcessOptions
            {
                RequiresOdsConnection = false,
                OdsConnectionSettings = new ApiServer
                {
                    Secret = "SomeSecret",
                    Key = "SomeKey",
                    ApiVersion = new ApiVersion
                    {
                        Version = "2.5+"
                    },
                    Url = "https://www.ed-fi.org"
                }
            };

            try
            {
                _service.ProcessStreamWithScript(scriptContent, new MemoryStream(), options);
            }
            catch (Exception e)
            {
                exception = e;
                e.InnerException.ShouldNotBeNull();
                e.InnerException.Message.ShouldContain("The term 'Invoke-OdsApiRequest' is not recognized as a name of a cmdlet");
            }

            exception.ShouldNotBeNull();

            options.RequiresOdsConnection = true;
            _service.ProcessStreamWithScript(scriptContent, new MemoryStream(), options);
        }

        [Test]
        public void ShouldProvideOdsConnectionInformationWhenAsked()
        {
            var odsBaseUrl = "https://localhost:54746/";

            var scriptContent = @"
                Write-Output $ODS.BaseUrl
                Write-Output $ODS.AccessToken
            ";

            var input = new MemoryStream();

            var output = _service.ProcessStreamWithScript(scriptContent, input,
                new ProcessOptions
                {
                    RequiresOdsConnection = true,
                    OdsConnectionSettings = new ApiServer
                    {
                        Url = odsBaseUrl,
                        ApiVersion = new ApiVersion
                        {
                            Version = "2.5+"
                        }
                    }
                });

            var reader = new StreamReader(output);
            var outputOdsBaseUrl = reader.ReadLine();
            var outputOdsAccessToken = reader.ReadLine();
            outputOdsBaseUrl.ShouldBe(odsBaseUrl);
            outputOdsAccessToken.ShouldBe("fake token");
        }

        [Test]
        public void ShouldThrowExceptionIfOdsConnectionSettingsAreMissing()
        {
            Should.Throw<ArgumentException>(() =>
            {
                _service.ProcessStreamWithScript("Write-Output ''", new MemoryStream(),
                    new ProcessOptions { RequiresOdsConnection = true });
            });
        }

        [Test]
        public void ShouldImportModulesFromConfiguration()
        {
            string psModule = @"
function Get-ExecutionPolicyCustom {
    # Make sure custom cmdlets are available for the custom modules
    $A = New-NamedArrayList TestArray

    # Make sure that default cmdlets are not restricted
    Write-Information (New-Object 'System.Text.StringBuilder')

    # Make sure execution policy is set to Bypass. For some reason, it is set to Unrestricted when executing from unit tests,
    # but it is Restricted when running from IIS which is aligned with the documentation
    return (Get-ExecutionPolicy)
}

Export-ModuleMember -Function Get-ExecutionPolicyCustom
";

            string modulePath = Path.Combine(Path.GetTempPath(), $"TestModule-{Guid.NewGuid()}.psm1");
            File.WriteAllText(modulePath, psModule);

            _powerShellPreprocessorOptions.Modules = new List<string>
            {
                modulePath
            };

            try
            {
                var scriptContent = "Get-ExecutionPolicyCustom";
                var input = new MemoryStream();
                var output = _service.ProcessStreamWithScript(scriptContent, input, new ProcessOptions
                {
                    UsePowerShellWithNoRestrictions = true
                });
                var outputContent = new StreamReader(output).ReadToEnd();

                outputContent.ShouldBe("Bypass");
            }
            finally
            {
                File.Delete(modulePath);
                _powerShellPreprocessorOptions.Modules.Clear();
            }
        }

        [Test]
        public void ShouldAllowDefaultCmdlets()
        {
            var scriptContent = @"
Write-Output (Get-Date)
Write-Output (ConvertFrom-Json '')
Write-Output ('some string' | Measure-Object )
Write-Output ('some string' | Out-Null )
Write-Host 'info'
";
            var input = new MemoryStream();
            var output = _service.ProcessStreamWithScript(scriptContent, input);

            Should.NotThrow(() => new StreamReader(output).ReadToEnd()); // just make sure that script processing does not throw an error which means all cmdlets in the script are allowed.
        }

        [Test]
        public void ShouldProvideArrayListCmdLets()
        {
            var scriptContent = @"
$EmptyCollection = @{}
$A  = New-NamedArrayList TestArray -Collection $EmptyCollection -Capacity 15
$Item = @{
    Property1 = 'one'
    Property2 = 'two'
}
Add-CollectionItem -ArrayListName TestArray -Item '1'
Add-CollectionItem -ArrayListName TestArray -Item $Item
Write-Output  $A.Count
Remove-CollectionItem -ArrayListName TestArray -Item $Item
Write-Output  $A.Count

$A = Get-NamedArrayList TestArray
Write-Output  $A.Count
Write-Output $A.Capacity
";
            var input = new MemoryStream();
            var output = _service.ProcessStreamWithScript(scriptContent, input);
            var outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe($"2{Environment.NewLine}1{Environment.NewLine}1{Environment.NewLine}15");
        }

        [Test]
        public void ShouldNotThrowExceptionIfNamedArrayAlreadyExists()
        {
            var scriptContent = @"
$A  = New-NamedArrayList TestArray
$A  = New-NamedArrayList TestArray
";
            var input = new MemoryStream();
            var output = _service.ProcessStreamWithScript(scriptContent, input);
            var outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe("");
        }

        [Test]
        public void ShouldProvideConvertFromFixedWidthCmdlet()
        {
            Action<string, string, string> test = (scriptContent, expected, message) =>
            {
                var input = new MemoryStream();
                var output = _service.ProcessStreamWithScript(scriptContent, input);
                var actual = new StreamReader(output).ReadToEnd();

                Assert.AreEqual(expected, actual, message);
            };

            test("Write-Output (ConvertFrom-FixedWidth -FixedWidthString \"012 45 789\" -FieldMap @(0, 4, 7) -NoTrim)",
                $"012 {Environment.NewLine}45 {Environment.NewLine}789", "Should be able to disable trim behavior");
            test("Write-Output (ConvertFrom-FixedWidth -FixedWidthString \"012 45 789\" -FieldMap @(0, 4, 7))",
                $"012{Environment.NewLine}45{Environment.NewLine}789", "Validate simple map. Trim by Default");
            test("Write-Output (ConvertFrom-FixedWidth -FixedWidthString \"012 45 789\" -FieldMap @((0, 3), (4, 2), 7))",
                $"012{Environment.NewLine}45{Environment.NewLine}789", "Validate complex map");
            test("(ConvertFrom-FixedWidth -FixedWidthString \"012 45 789\" -FieldMap @(0, 4, 7)).ForEach({ Write-Output $_.Length })",
                $"3{Environment.NewLine}2{Environment.NewLine}3", "Should trim by default");
            test("(ConvertFrom-FixedWidth -NoTrim -FixedWidthString \"012 45 789\" -FieldMap @(0, 4, 7)).ForEach({ Write-Output $_.Length })",
                $"4{Environment.NewLine}3{Environment.NewLine}3", "NoTrim at first position");
        }


        [Test]
        public void ShouldThrowExceptionForIncorrectMappingInConvertFromFixedWidthCmdlet()
        {
            var scriptContent = @"
Write-Output (ConvertFrom-FixedWidth -FixedWidthString ""012 45 789"" -FieldMap @((0, 3), (4, 2), (7, 3, 5)))
";
            Should.Throw<PowerShellProcessException>(() => _service.ProcessStreamWithScript(scriptContent, new MemoryStream()));
        }

        [Test]
        public void ShouldProvideCmdletsForCache()
        {
            var cacheIdentifier = Guid.NewGuid().ToString();
            try
            {
                var scriptContent = @"
Write-Output (Get-AgentCacheItem -Key 'someKey')
New-AgentCacheItem -Key 'someKey' -Value 'Hi'
Write-Output (Get-AgentCacheItem -Key 'someKey')
";
                var input = new MemoryStream();
                var output = _service.ProcessStreamWithScript(scriptContent, input, new ProcessOptions
                {
                    CacheIdentifier = cacheIdentifier
                });

                var outputContent = new StreamReader(output).ReadToEnd();
                outputContent.ShouldBe($"{Environment.NewLine}Hi");


                scriptContent = @"
Write-Output (Get-AgentCacheItem -Key 'someKey')
";
                input = new MemoryStream();
                output = _service.ProcessStreamWithScript(scriptContent, input, new ProcessOptions
                {
                    CacheIdentifier = cacheIdentifier
                });

                outputContent = new StreamReader(output).ReadToEnd();
                outputContent.ShouldBe(@"Hi");
            }
            finally
            {
                DataImportCacheManager.DestroyCache(cacheIdentifier);
            }
        }

        [TestCase("Write-Output (Get-AgentCacheItem -Key 'someKey')", Description = "Get-AgentCacheItem")]
        [TestCase("New-AgentCacheItem -Key 'someKey' -Value 'Hi'", Description = "New-AgentCacheItem")]
        public void CacheCmdletsShouldThrowExceptionIfCacheIdentifierWasNotSupplied(string scriptContent)
        {
            Should.Throw<PowerShellProcessException>(() => _service.ProcessStreamWithScript(scriptContent, new MemoryStream()));
        }

        [Test]
        public void ShouldProvideBuiltInVariables()
        {
            var scriptContent = @"
Write-Output $DataImport.MapAttribute
Write-Output $DataImport.Filename
Write-Output $DataImport.PreviewFlag
";
            var input = new MemoryStream();
            var output = _service.ProcessStreamWithScript(scriptContent, input, new ProcessOptions
            {
                FileName = "Students.csv",
                IsDataMapPreview = true,
                MapAttribute = "MapAttribute"
            });
            var outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe($"MapAttribute{Environment.NewLine}Students.csv{Environment.NewLine}True");

            input = new MemoryStream();
            output = _service.ProcessStreamWithScript(scriptContent, input);
            outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe($"{Environment.NewLine}{Environment.NewLine}");
        }

        [Test]
        public void ShouldAllowModifyingBuiltInVariables()
        {
            var scriptContent = @"
$DataImport.MapAttribute = 'test'
$DataImport.Filename = 'new file'
$DataImport.PreviewFlag  = $false
";

            Should.Throw<PowerShellProcessException>(() =>
            {
                var input = new MemoryStream();
                _service.ProcessStreamWithScript(scriptContent, input, new ProcessOptions
                {
                    FileName = "Students.csv",
                    IsDataMapPreview = true,
                    MapAttribute = "MapAttribute"
                });
            });
        }

        [Test]
        public void ShouldAllowRunningPowerShellWithNoRestrictions()
        {
            var scriptContent = "Write-Output $ExecutionContext.SessionState.LanguageMode";
            var input = new MemoryStream();
            var output = _service.ProcessStreamWithScript(scriptContent, input);
            var outputContent = new StreamReader(output).ReadToEnd();

            outputContent.ShouldBe("ConstrainedLanguage");

            input = new MemoryStream();
            output = _service.ProcessStreamWithScript(scriptContent, input, new ProcessOptions
            {
                UsePowerShellWithNoRestrictions = true
            });
            outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe("FullLanguage");

            scriptContent = "Get-Help";
            input = new MemoryStream();
            output = _service.ProcessStreamWithScript(scriptContent, input, new ProcessOptions
            {
                UsePowerShellWithNoRestrictions = true
            });
            outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldNotBeEmpty();
        }

        [Test]
        public void ShouldBeAbleToModifyOdsVariableInFullLanguageMode()
        {
            var scriptContent = @"
            $BaseUrl = New-Object 'System.Uri' 'http://localhost.not.real:57382/api/v2.0/2019/'
            $AccessToken = 'Access Token'
            $ODS = New-Object 'DataImport.Common.Preprocessors.OdsAuthenticationResult' @($BaseUrl, $AccessToken)
            Invoke-OdsApiRequest '/calendarDates'
";
            var exception = Should.Throw<PowerShellProcessException>(() =>
                _service.ProcessStreamWithScript(scriptContent, new MemoryStream(), new ProcessOptions
                {
                    RequiresOdsConnection = true,
                    UsePowerShellWithNoRestrictions = true,
                    OdsConnectionSettings = new ApiServer
                    {
                        Url = "http://localhost:54746/data/v5",
                        TokenUrl = "http://localhost:54746/oauth/token",
                        ApiVersion = new ApiVersion
                        {
                            Version = "3.2"
                        }
                    }
                })
                );

            exception.InnerException.ShouldBeOfType<HttpRequestException>();
        }

        [Test]
        public void ShouldNotAllowOverwritingOdsVariableWithCustomObject()
        {
            var scriptContent = @"
            $BaseUrl = New-Object 'System.Uri' 'http://localhost.not.real:57382/api/v2.0/2019/'
            $AccessToken = 'Access Token'
            $ODS = [PSCustomObject]@{
                AccessToken = 'Some Token'
                BaseUrl = [PSCustomObject]@{
                        AbsoluteUri  = 'http://localhost.not.real:57382/api/v2.0/2019/'
                    }
                }
            Invoke-OdsApiRequest '/calendarDates'
";
            var exception = Should.Throw<PowerShellProcessException>(() =>
                _service.ProcessStreamWithScript(scriptContent, new MemoryStream(), new ProcessOptions
                {
                    RequiresOdsConnection = true,
                    UsePowerShellWithNoRestrictions = true,
                    OdsConnectionSettings = new ApiServer
                    {
                        Url = "http://localhost:54746/data/v5",
                        TokenUrl = "http://localhost:54746/oauth/token",
                        ApiVersion = new ApiVersion
                        {
                            Version = "3.2"
                        }
                    }
                })
            );

            exception.InnerException.ShouldNotBeNull();
            exception.InnerException.ShouldBeOfType<InvalidOperationException>();
            exception.InnerException.Message.ShouldBe("Cannot get authentication result. Either set the 'ODS' variable explicitly or set 'OdsAuthenticator'.");
        }

        [Test]
        public void ShouldHaveApiVersionInDataImportVariable()
        {
            var scriptContent = @"Write-Output $DataImport.ApiVersion";
            var output = _service.ProcessStreamWithScript(scriptContent, new MemoryStream(), new ProcessOptions
            {
                RequiresOdsConnection = true,
                OdsConnectionSettings = new ApiServer
                {
                    Url = "http://localhost:54746/data/v5",
                    TokenUrl = "http://localhost:54746/oauth/token",
                    ApiVersion = new ApiVersion
                    {
                        Version = "3.2"
                    }
                }
            });
            var outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe("3.2");

            output = _service.ProcessStreamWithScript(scriptContent, new MemoryStream(), new ProcessOptions());
            outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe("");
        }

        [Test]
        public void ShouldLogDetailsAboutTerminatingError()
        {
            var exception = Should.Throw<PowerShellProcessException>(() =>
            {
                _service.ProcessStreamWithScript("throw 'this should be in host.InvocationStateInfo.Reason'", new MemoryStream());
            });
            exception.InnerException.ShouldNotBeNull();
            exception.InnerException.Message.ShouldBe("this should be in host.InvocationStateInfo.Reason");
        }

        [Test]
        public void ShouldUseBaseUrlForAllVersionsOfApiServersSoThatExtensionsCanBeAccessed()
        {
            var scriptContent = @"Write-Output $ODS.BaseUrl";
            var output = _service.ProcessStreamWithScript(scriptContent, new MemoryStream(), new ProcessOptions
            {
                RequiresOdsConnection = true,
                OdsConnectionSettings = new ApiServer
                {
                    Url = "http://localhost:54746/data/v5",
                    TokenUrl = "http://localhost:54746/oauth/token",
                    ApiVersion = new ApiVersion
                    {
                        Version = "3.2"
                    }
                }
            });
            var outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe("http://localhost:54746/data/v5"); //Consumers can then provide "/ed-fi/...", "/tpdm/...", etc.

            output = _service.ProcessStreamWithScript(scriptContent, new MemoryStream(), new ProcessOptions
            {
                RequiresOdsConnection = true,
                OdsConnectionSettings = new ApiServer
                {
                    Url = "http://localhost:54746/api/v2.0/2019/",
                    TokenUrl = "http://localhost:54746/oauth/token",
                    ApiVersion = new ApiVersion
                    {
                        Version = "2.5+"
                    }
                }
            });
            outputContent = new StreamReader(output).ReadToEnd();
            outputContent.ShouldBe("http://localhost:54746/api/v2.0/2019/");
        }
    }
}
