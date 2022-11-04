// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.TestHelpers;
using DataImport.Web.Features.ApiServers;
using Shouldly;

namespace DataImport.Web.Tests.Features.Configuration
{
    using DataImport.Web.Features.Configuration;
    using NUnit.Framework;
    using System.Linq;
    using System.Threading.Tasks;
    using static Testing;

    public class EditConfigurationTests
    {
        [Test]
        public async Task ShouldDisplayCurrentConfiguration()
        {
            var editForm = await Send(new EditConfiguration.Query());

            editForm.ShouldMatch(new EditConfiguration.ViewModel
            {
                ConfigurationFailureMsg = null
            });
        }

        [Test]
        public async Task ShouldDisplayGuidanceForMissingConfiguration()
        {
            var apiServers = Query(x => x.ApiServers.ToList());

            try
            {
                // Deleting the api server records for testing. Disassociate existing API Agents from ApiServers first
                Query(d =>
                {
                    var agents = d.Agents.ToList();
                    agents.ForEach(a => a.ApiServerId = null);
                    d.SaveChanges();
                    return agents;
                });

                foreach (var apiServer in apiServers)
                {
                    await Send(new DeleteApiServer.Command { Id = apiServer.Id });
                }

                var editForm = await Send(new EditConfiguration.Query
                {
                    OdsApiServerException = true
                });

                editForm.ShouldMatch(new EditConfiguration.ViewModel
                {
                    ConfigurationFailureMsg = "In order to proceed, please configure the ODS API Server.",
                    InstanceAllowUserRegistration = false
                });
            }
            finally
            {
                // Adding the deleted api server configuration record
                await ConfigureForOdsApiV311();
            }
        }

        [Test]
        public async Task ShouldModifyServerConfiguration()
        {
            try
            {
                var editForm = await Send(new EditConfiguration.Query
                {
                    OdsApiServerException = false
                });

                await Send(ViewModelToCommand(editForm));

                var updatedEditForm = await Send(new EditConfiguration.Query
                {
                    OdsApiServerException = false
                });

                updatedEditForm.ShouldMatch(new EditConfiguration.ViewModel
                {
                    ConfigurationFailureMsg = null,
                    InstanceAllowUserRegistration = false
                });
            }
            finally
            {
                // Reset to a well known configuration.
                await ConfigureForOdsApiV311();
            }
        }

        private static EditConfiguration.Command ViewModelToCommand(EditConfiguration.ViewModel editForm)
        {
            return new EditConfiguration.Command
            {
                InstanceAllowUserRegistration = editForm.InstanceAllowUserRegistration
            };
        }

        [Test]
        public async Task ShouldNotChangePowerShellPreprocessorOptions()
        {
            Query(d =>
            {
                var configuration = d.Configurations.Single();

                configuration.AvailableCmdlets = "Some-Cmdlet";
                configuration.ImportPSModules = "Some-PsModule";

                return configuration;
            });

            var config = Query(d => d.Configurations.Single());
            config.AvailableCmdlets.ShouldBe("Some-Cmdlet");
            config.ImportPSModules.ShouldBe("Some-PsModule");

            var editForm = await Send(new EditConfiguration.Query());
            await Send(ViewModelToCommand(editForm));

            config = Query(d => d.Configurations.Single());
            config.AvailableCmdlets.ShouldBe("Some-Cmdlet");
            config.ImportPSModules.ShouldBe("Some-PsModule");

            Query(d =>
            {
                var configuration = d.Configurations.Single();

                configuration.AvailableCmdlets = null;
                configuration.ImportPSModules = null;

                return configuration;
            });
            config = Query(d => d.Configurations.Single());
            config.AvailableCmdlets.ShouldBeNull();
            config.ImportPSModules.ShouldBeNull();
        }
    }
}
