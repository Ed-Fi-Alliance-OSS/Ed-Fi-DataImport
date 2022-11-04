// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.Preprocessors;
using DataImport.Models;
using DataImport.Web.Features.Shared;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataImport.Web.Features.Share
{
    public class FileImport : ImportBase
    {
        public class FileUploadForm : IApiVersionSpecificRequest
        {
            private Command _command;

            [Display(Name = "Import Template")]
            [Accept(".json")]
            public IFormFile File { get; set; }

            public Command AsCommand()
            {
                return _command ?? (_command = new Command
                {
                    Import = SharingModel.Deserialize(File)
                });
            }

            public string GetApiVersion()
            {
                if (File == null)
                {
                    return string.Empty;
                }

                return AsCommand().Import.ApiVersion;
            }
        }

        public class Form : IApiVersionSpecificRequest
        {
            private Command _command;

            public string Template { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }

            [Display(Name = "Ed-Fi ODS / API Version")]
            public string ApiVersion { get; set; }
            public string OriginalApiVersion { get; set; }
            public List<SelectListItem> ApiVersions { get; set; }

            public bool OverwriteExistingPreprocessors { get; set; }

            public Command AsCommand()
            {
                return _command ?? (_command = new Command
                {
                    Import = new SharingModel
                    {
                        Title = Title,
                        Description = Description,
                        ApiVersion = ApiVersion,
                        Template = JsonConvert.DeserializeObject<SharingTemplate>(Template)
                    },
                    OverwritePreprocessors = OverwriteExistingPreprocessors
                });
            }

            public string GetApiVersion()
            {
                return ApiVersion;
            }
        }

        public class FileUploadFormValidator : AbstractValidator<FileUploadForm>
        {
            public FileUploadFormValidator()
                => RuleFor(x => x.File).NotNull().WithName("Import Template");
        }

        public class Validator : AbstractValidator<Form>
        {
            public Validator(ILogger logger, DataImportDbContext database, IPowerShellPreprocessorService powerShellPreprocessorService, IJsonValidator jsonValidator)
            {
                RuleFor(x => x.Template).NotNull();
                RuleFor(x => x.ApiVersion).NotNull().WithName("Ed-Fi ODS / API Version");

                When(x => x.Template != null && x.ApiVersion != null, () =>
                {
                    RuleFor(x => x)
                        .SafeCustom(logger, (form, context) =>
                        {
                            var sharingModelValidator = new SharingModelValidator(logger, database, powerShellPreprocessorService, jsonValidator, form.OverwriteExistingPreprocessors);
                            var sharingModel = form.AsCommand().Import;

                            context.AddFailures(sharingModelValidator.Validate(sharingModel));
                        });
                });
            }
        }
    }
}
