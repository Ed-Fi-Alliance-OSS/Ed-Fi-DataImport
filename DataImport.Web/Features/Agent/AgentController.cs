// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Agent
{
    public class AgentController : BaseController
    {
        private readonly IMediator _mediator;

        public AgentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index()
        {
            var viewModel = await _mediator.Send(new AgentIndex.Query());

            return View(viewModel);
        }

        public async Task<ActionResult> Add()
        {
            var viewModel = await _mediator.Send(new AddAgent.Query());

            return View("AddEditAgent", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddEditAgentViewModel vm)
        {
            var response = await _mediator.Send(new AddAgent.Command { ViewModel = vm });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        public async Task<ActionResult> Edit(int id)
        {
            var vm = await _mediator.Send(new EditAgent.Query { Id = id });

            return View("AddEditAgent", vm);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AddEditAgentViewModel vm)
        {
            var response = await _mediator.Send(new EditAgent.Command { ViewModel = vm });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Archive(int id)
        {
            var response = await _mediator.Send(new ArchiveAgent.Command { Id = id });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> ToggleAgentStatus(int id)
        {
            var response = await _mediator.Send(new ToggleAgentStatus.Command { Id = id });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> TestAgentConnection(AgentFiles.Query query)
        {
            var result = await _mediator.Send(query);

            return Json(result);
        }

        public async Task<ActionResult> UploadFile(int id)
        {
            var vm = await _mediator.Send(new UploadFile.Query() { AgentId = id });

            return View("Upload", vm);
        }

        [HttpPost]
        public async Task<ActionResult> UploadFile(UploadFile.Command command)
        {
            var response = await _mediator.Send(command);
            if (response != null)
                ToastMessage(response);
            else
                ErrorMessage("Error Manually Uploading File to Agent. Please check the logs.");

            return RedirectToAction("Index", "Activity");
        }
    }
}
