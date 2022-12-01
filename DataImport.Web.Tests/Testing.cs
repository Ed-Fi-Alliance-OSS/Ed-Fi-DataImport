// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Common.Enums;
using DataImport.Models;
using DataImport.Web.Features.Agent;
using DataImport.Web.Features.ApiServers;
using DataImport.Web.Features.BootstrapData;
using DataImport.Web.Features.Configuration;
using DataImport.Web.Features.DataMaps;
using DataImport.Web.Features.Lookup;
using DataImport.Web.Features.Preprocessor;
using DataImport.Web.Services.Swagger;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using File = DataImport.Models.File;
using Microsoft.Extensions.Hosting;

namespace DataImport.Web.Tests
{
    public static class Testing
    {
        public static readonly IServiceProvider Services;
        private static readonly Random Random = new();

        public const string OdsApiV25 = "2.5+";
        public const string OdsApiV311 = "3.1.1";

        static Testing()
        {
            var configuration = new ConfigurationBuilder()
                  .SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile("appsettings.json", false)
                  .AddEnvironmentVariables()
                  .Build();

            var startup = new Startup(configuration);
            var shareName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestShareFolder");
            var appSettings = configuration.GetSection("AppSettings");
            appSettings["ShareName"] = shareName;

            var serviceCollection = new ServiceCollection();
            startup.ConfigureServices(serviceCollection);
            ConfigureTestSpecificIocRules(serviceCollection);

            Services = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureTestSpecificIocRules(IServiceCollection services)
        {
            services.AddSingleton<ISwaggerWebClient, StubSwaggerWebClient>();
            services.AddSingleton<IClock, StubClock>();
            services.AddSingleton<IHostEnvironment, StubHostEnvironment>();
            services.AddLogging();
        }

        public static void With<TService>(Action<TService> useService)
        {
            using (var scope = Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<DataImportDbContext>();

                try
                {
                    database.BeginTransaction();
                    var service = scope.ServiceProvider.GetService<TService>();
                    useService(service);
                    database.CloseTransaction();
                }
                catch (Exception exception)
                {
                    database.CloseTransaction(exception);
                    throw;
                }
            }
        }

        public static async Task With<TService>(Func<TService, Task> useService)
        {
            using (var scope = Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<DataImportDbContext>();

                try
                {
                    database.BeginTransaction();
                    var service = scope.ServiceProvider.GetService<TService>();
                    await useService(service).ConfigureAwait(false);
                    database.CloseTransaction();
                }
                catch (Exception exception)
                {
                    database.CloseTransaction(exception);
                    throw;
                }
            }
        }

        public static async Task Send(IRequest message)
        {
            using (var scope = Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<DataImportDbContext>();

                try
                {
                    database.BeginTransaction();
                    Validator(scope, message)?.Validate(new ValidationContext<IRequest>(message)).ShouldBeSuccessful();
                    await scope.ServiceProvider.GetService<IMediator>().Send(message);
                    database.CloseTransaction();
                }
                catch (Exception exception)
                {
                    database.CloseTransaction(exception);
                    throw;
                }
            }
        }

        public static async Task<TResponse> Send<TResponse>(IRequest<TResponse> message)
        {
            TResponse response;

            using (var scope = Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<DataImportDbContext>();

                try
                {
                    database.BeginTransaction();
                    Validator(scope, message)?.Validate(new ValidationContext<IRequest<TResponse>>(message)).ShouldBeSuccessful();
                    response = await scope.ServiceProvider.GetService<IMediator>().Send(message);
                    database.CloseTransaction();
                }
                catch (Exception exception)
                {
                    database.CloseTransaction(exception);
                    throw;
                }
            }

            return response;
        }

        public static async Task Transaction(Func<IServiceScope, Task> useScope)
        {
            using (var scope = Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<DataImportDbContext>();

                try
                {
                    database.BeginTransaction();
                    await useScope(scope);
                    database.CloseTransaction();
                }
                catch (Exception exception)
                {
                    database.CloseTransaction(exception);
                    throw;
                }
            }
        }

        public static void Transaction(Action<DataImportDbContext> action)
        {
            using (var scope = Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<DataImportDbContext>();

                try
                {
                    database.BeginTransaction();
                    action(database);
                    database.CloseTransaction();
                }
                catch (Exception exception)
                {
                    database.CloseTransaction(exception);
                    throw;
                }
            }
        }

        public static TEntity Query<TEntity>(int id) where TEntity : Entity
        {
            return Query(database => database.Set<TEntity>().Find(id));
        }

        public static TResult Query<TResult>(Func<DataImportDbContext, TResult> query)
        {
            var result = default(TResult);

            Transaction(database =>
            {
                result = query(database);
            });

            return result;
        }

        public static TEntity Find<TEntity>(object key) where TEntity : class
            => Query(database => database.Set<TEntity>().Find(key));

        public static int Count<TEntity>() where TEntity : class
            => Query(database => database.Set<TEntity>().Count());

        public static string SampleLogo(string name = null)
            => SampleString(name) + ".jpg";

        public static string SampleString(string name = null)
        {
            return name == null
                ? Guid.NewGuid().ToString()
                : $"{name}-{Guid.NewGuid()}";
        }

        public static string SampleString(int length)
        {
            return new string(Enumerable.Repeat('X', length).ToArray());
        }

        public static ValidationResult Validation<T>(T message)
        {
            using (var scope = Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<DataImportDbContext>();

                try
                {
                    database.BeginTransaction();
                    var validator = Validator(scope, message);

                    if (validator == null)
                        throw new Exception($"There is no validator for {message.GetType()} messages.");

                    var validationResult = validator.Validate(new ValidationContext<T>(message));
                    database.CloseTransaction();
                    return validationResult;
                }
                catch (Exception exception)
                {
                    database.CloseTransaction(exception);
                    throw;
                }
            }
        }

        private static IValidator Validator<TMessage>(IServiceScope scope, TMessage message)
        {
            var validatorType = typeof(IValidator<>).MakeGenericType(message.GetType());
            return scope.ServiceProvider.GetService(validatorType) as IValidator;
        }

        public static async Task ConfigureForOdsApi(string apiServerUrl, string apiVersion)
        {
            var hasDefaultApiServer = Query(d => d.ApiServers.Any());
            if (!hasDefaultApiServer)
            {
                await AddApiServer(apiServerUrl, apiVersion);
            }
            else
            {
                var defaultApiServer = GetDefaultApiServer();
                await Send(new EditApiServer.Command
                {
                    ViewModel = new AddEditApiServerViewModel
                    {
                        Id = defaultApiServer.Id,
                        ApiVersion = apiVersion,
                        Name = "Default API Connection",
                        Url = apiServerUrl,
                        Key = SampleString("testKey"),
                        Secret = SampleString("testSecret")
                    }
                });
            }

            await Send(new EditConfiguration.Command());
        }

        public static async Task ConfigureForOdsApiV25()
        {
            await ConfigureForOdsApi(StubSwaggerWebClient.ApiServerUrlV25, OdsApiV25);
        }

        public static async Task ConfigureForOdsApiV311()
        {
            await ConfigureForOdsApi(StubSwaggerWebClient.ApiServerUrlV311, OdsApiV311);
        }

        public static async Task<DataMapper[]> TrivialMappings(Resource resource)
        {
            // When tests need to submit the Add / Edit form, and are not concerned
            // with submitting specific mappings, they can simulate submitting the dynamic
            // form with no selections by submitting the initial state of the mappings
            // at time of initial form display. Tests which use these "trivial mappings"
            // simulates a user that witnesses the dynamic form appear for their chosen
            // Resource and then simply save without providing any specific mapping selections.

            var fieldsViewModel = await Send(new DataMapperFields.Query
            {
                ResourcePath = resource.Path,
                ApiVersionId = resource.ApiVersionId
            });

            return fieldsViewModel.Mappings.ToArray();
        }

        public static void Delete<TEntity>(TEntity entity)
            where TEntity : Entity
        {
            Transaction(database =>
            {
                database.Set<TEntity>().Attach(entity);
                database.Set<TEntity>().Remove(entity);
            });
        }

        public static TItem RandomItem<TItem>(params TItem[] items)
        {
            var count = items.Length;

            var skip = Random.Next(maxValue: count);

            return items.Skip(skip).Take(1).Single();
        }

        public static Resource RandomResource(int? apiVersionId = null)
        {
            if (!apiVersionId.HasValue)
            {
                apiVersionId = GetDefaultApiVersion().Id;
            }

            return Query(database =>
            {
                var count = database.Resources.Count(x => x.ApiVersionId == apiVersionId);

                var skip = Random.Next(maxValue: count);

                return database.Resources.Where(x => x.ApiVersionId == apiVersionId).OrderBy(x => x.Id).Skip(skip).Take(1).Single();
            });
        }

        public static ApiServer GetDefaultApiServer()
        {
            return Query(d => d.ApiServers.OrderBy(y => y.Id).First());
        }

        public static ApiVersion GetDefaultApiVersion()
        {
            return Query(d => d.ApiServers.OrderBy(y => y.Id).Select(y => y.ApiVersion).First());
        }

        public static async Task<BootstrapData> AddBootstrapData(Resource resource, JToken data = null)
        {
            var resourcePath = resource.Path;

            var response = await Send(new AddBootstrapData.Command
            {
                Name = SampleString(),
                ResourcePath = resourcePath,
                Data = data == null ? "[]" : data.ToString(Formatting.Indented),
                ApiVersionId = resource.ApiVersionId
            });

            return Query<BootstrapData>(response.BootstrapDataId);
        }

        public static async Task<Script> AddPreprocessor(ScriptType scriptType, bool hasAttribute = false)
        {
            var vm = new AddEditPreprocessorViewModel
            {
                Name = SampleString(),
                ScriptType = scriptType,
            };

            if (scriptType.IsPowerShell())
            {
                vm.RequireOdsApiAccess = false;
                vm.ScriptContent = "Write-Output 'Hello from Unit Tests'";
                vm.HasAttribute = hasAttribute;
            }
            if (scriptType.IsExternal())
            {
                vm.RequireOdsApiAccess = false;
                vm.ExecutablePath = Path.GetTempFileName().Replace('\\', '/');
                vm.ExecutableArguments = "args";
            }

            var response = await Send(new AddPreprocessor.Command { ViewModel = vm });

            return Query<Script>(response.PreprocessorId);
        }

        public static async Task<DataMap> AddDataMap(Resource resource, string[] columnHeaders, DataMapper[] mappings = null, int? preprocessorId = null, string attribute = null)
        {
            if (mappings == null)
                mappings = await TrivialMappings(resource);

            var response = await Send(new AddDataMap.Command
            {
                ApiVersionId = resource.ApiVersionId,
                MapName = SampleString(),
                ResourcePath = resource.Path,
                ColumnHeaders = columnHeaders,
                Mappings = mappings,
                PreprocessorId = preprocessorId,
                Attribute = attribute
            });

            return Query<DataMap>(response.DataMapId);
        }

        public static async Task<Lookup> AddLookup(string sourceTable, string key, string value)
        {
            var response = await Send(new AddLookup.Command
            {
                SourceTable = sourceTable,
                Key = key,
                Value = value
            });

            return Query<Lookup>(response.LookupId);
        }

        public static async Task<Agent> AddAgent(string agentTypeCode, int? apiServerId = null)
        {
            if (!apiServerId.HasValue)
            {
                apiServerId = GetDefaultApiServer().Id;
            }


            var viewModel = new AddEditAgentViewModel
            {
                AgentTypeCode = agentTypeCode,
                Name = SampleString(),
                Enabled = true,
                ApiServerId = apiServerId.Value
            };

            var response = await Send(new AddAgent.Command { ViewModel = viewModel });

            return Query<Agent>(response.AgentId);
        }

        public static async Task<File> UploadFile()
        {
            var agent = await AddAgent(AgentTypeCodeEnum.Manual);

            var fileName = SampleString() + ".csv";
            var fileContent = "Col1,Col2,Col3" + Environment.NewLine + "1,2,3";

            await Send(new UploadFile.Command
            {
                AgentId = agent.Id,
                File = new StubHttpPostedFileBase(fileName, fileContent)
            });

            return Query(database => database.Files.Single(x => x.FileName == fileName));
        }

        public static void UpdateEncryptionKeyValueOnAppConfig(string value)
        {
            var appSettings = Services.GetService<IOptions<AppSettings>>();
            appSettings.Value.EncryptionKey = value;
        }

        public static Task<ApiServer> AddApiServer()
        {
            return AddApiServer(StubSwaggerWebClient.ApiServerUrlV311, OdsApiV311);
        }

        public static async Task<ApiServer> AddApiServer(string url, string apiVersion)
        {
            var viewModel = new AddEditApiServerViewModel
            {
                Name = SampleString("ApiServer"),
                ApiVersion = apiVersion,
                Url = url,
                Key = SampleString("testKey"),
                Secret = SampleString("testSecret")
            };

            var response = await Send(new AddApiServer.Command { ViewModel = viewModel });

            return Query<ApiServer>(response.ApiServerId);
        }
    }
}
