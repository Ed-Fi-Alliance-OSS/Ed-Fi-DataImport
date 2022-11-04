// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Models;
using DataImport.Web.Helpers;
using DataImport.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Share
{
    public class ShareController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly DataImportDbContext _dbContext;
        private readonly SharingPreprocessorValidationService _sharingPreprocessorValidationService;

        public ShareController(IMediator mediator, DataImportDbContext dbContext, SharingPreprocessorValidationService sharingPreprocessorValidationService)
        {
            _mediator = mediator;
            _dbContext = dbContext;
            _sharingPreprocessorValidationService = sharingPreprocessorValidationService;
        }

        public ActionResult FileIndex()
        {
            return View(new FileImport.FileUploadForm());
        }

        public async Task<ActionResult> FileExport(FileExport.Query query)
        {
            return View(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<ActionResult> FileExport(FileExport.Command command)
        {
            var export = await _mediator.Send(command);

            return Content(export.Serialize(), "application/json");
        }

        [HttpPost]
        public ActionResult FileImportUpload(FileImport.FileUploadForm fileUploadForm)
        {
            if (ModelState.IsValid)
            {
                var command = fileUploadForm.AsCommand();

                var overwriteExistingPreprocessors = _sharingPreprocessorValidationService.HasConflictingPreprocessors(command.Import, out var conflictingPreprocessors);

                var model = new FileImport.Form
                {
                    Template = command.Import.SerializeTemplate(),
                    Title = command.Import.Title,
                    Description = command.Import.Description,

                    OriginalApiVersion = command.Import.ApiVersion,
                    ApiVersion = command.Import.ApiVersion,
                    ApiVersions = VersionSelectList(),

                    OverwriteExistingPreprocessors = overwriteExistingPreprocessors
                };

                ViewBag.ConflictingPreprocessors = conflictingPreprocessors;

                return View("FileImport", model);
            }

            return View("FileIndex", fileUploadForm);
        }

        [HttpPost]
        public async Task<ActionResult> FileImport(FileImport.Form form)
        {
            var response = await _mediator.Send(form.AsCommand());
            ToastMessage(response);
            return this.RedirectToActionJson("Index", "DataMaps");
        }

        private List<SelectListItem> VersionSelectList()
        {
            return _dbContext.ApiVersions
                .OrderBy(x => x.Version)
                .Select(x => x.Version)
                .ToList()
                .ToSelectListItems("Select API Version");
        }
    }
}
