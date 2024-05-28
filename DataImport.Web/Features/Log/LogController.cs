// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Log
{
    public class LogController : BaseController
    {
        private readonly IMediator _mediator;

        public LogController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ViewResult Index(FilesLog.Query query)
        {
            return View(new LogViewModel
            {
                ApiServerId = query.ApiServerId
            });
        }

        [HttpPost]
        public async Task<ActionResult> IngestionLog(LogViewModel.Filters filters, int pageNumber)
        {
            return PartialView("_IngestionLog",
                await _mediator.Send(new IngestionLog.Query { LogFilters = filters, PageNumber = pageNumber }));
        }

        [HttpGet]
        public async Task<ActionResult> IngestionLog7xFilters(OdsApi7xFilters.Query query)
        {
            return PartialView("_OdsApi7xPartial", await _mediator.Send(query));
        }

        [HttpGet]
        public async Task<ActionResult> IngestionLogEdOrgIds(EducationOrganizationId.Query query)
        {
            return PartialView("_EducationOrganizationIds", await _mediator.Send(query));
        }

        public async Task<ActionResult> ApplicationLog(ApplicationLog.Query query)
        {
            return PartialView("_ApplicationLog", await _mediator.Send(query));
        }

        public async Task<ActionResult> FilesLog(FilesLog.Query query)
        {
            ViewBag.SelectApiConnectionAction = "Index";
            return PartialView("_FilesLog", await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<ActionResult> Retry(RetryFile.Command command)
        {
            var response = await _mediator.Send(command);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Cancel(CancelFile.Command command)
        {
            var response = await _mediator.Send(command);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }
    }
}
