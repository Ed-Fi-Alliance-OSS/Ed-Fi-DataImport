// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Services;
using FluentValidation;
using MediatR;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DataImport.Web.Features.ApiServers
{
    public class AddEditApiServerViewModel : IRequest
    {
        public int? Id { get; set; }

        [Display(Name = "Name")]
        public string Name { get; set; }

        [Display(Name = "URL")]
        public string Url { get; set; }

        [Display(Name = "API Version")]
        public string ApiVersion { get; set; }

        [Display(Name = "Tenant")]
        public string Tenant { get; set; }

        [Display(Name = "Context")]
        public string Context { get; set; }

        [Display(Name = "Key")]
        public string Key { get; set; }

        [Display(Name = "Secret")]
        public string Secret { get; set; }

        public string ConfigurationFailureMsg { get; set; }

        public string EncryptionFailureMsg { get; set; }

        public async Task MapTo(ApiServer apiServer, ApiVersion apiVersion, IEncryptionService encryptionService, string encryptionKey, IConfigurationService configurationService)
        {
            if (apiVersion.Version != ApiVersion)
            {
                throw new ArgumentException($"Incorrect ApiVersion. Expected {ApiVersion}, actual: {apiVersion.Version}", nameof(apiVersion));
            }

            apiServer.Name = Name;
            apiServer.Url = Url;
            apiServer.ApiVersion = apiVersion;
            apiServer.Context = Context;
            apiServer.Tenant = Tenant;

            if (!SensitiveText.IsMasked(Key))
            {
                apiServer.Key = encryptionService.TryEncrypt(Key, encryptionKey, out var encryptedKey) ? encryptedKey : string.Empty;
            }

            if (!SensitiveText.IsMasked(Secret))
            {
                apiServer.Secret = encryptionService.TryEncrypt(Secret, encryptionKey, out var encryptedKey) ? encryptedKey : string.Empty;
            }

            apiServer.TokenUrl = await configurationService.GetTokenUrl(apiServer.Url, apiServer.ApiVersion.Version, apiServer.Tenant, apiServer.Context);
            apiServer.AuthUrl = await configurationService.GetAuthUrl(apiServer.Url, apiServer.ApiVersion.Version, apiServer.Tenant, apiServer.Context);
        }

        public class Validator : AbstractValidator<AddEditApiServerViewModel>
        {
            private readonly DataImportDbContext _dbContext;

            public Validator(DataImportDbContext dbContext)
            {
                _dbContext = dbContext;

                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Name).Must(ValidateApiServerName).WithMessage("API Connection with name '{PropertyValue}' already exists.");
                RuleFor(x => x.Url).NotEmpty().WithName("URL");
                RuleFor(x => x.Key).NotEmpty();
                RuleFor(x => x.Secret).NotEmpty();
            }

            private bool ValidateApiServerName(AddEditApiServerViewModel viewModel, string apiServerName)
            {
                var apiServers = _dbContext.ApiServers.Where(x => x.Name == apiServerName && (!viewModel.Id.HasValue || (viewModel.Id.HasValue && x.Id != viewModel.Id))).ToList();

                if (apiServers.Count == 0)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
