// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataImport.Web.Features.ApiServers
{
    public class ApiServersController : BaseController
    {
        private readonly IMediator _mediator;

        public ApiServersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index()
        {
            var viewModel = await _mediator.Send(new ApiServerIndex.Query());

            return View(viewModel);
        }

        public async Task<ActionResult> Add()
        {
            var viewModel = await _mediator.Send(new AddApiServer.Query());

            return View("AddEditApiServer", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddEditApiServerViewModel vm)
        {
            var response = await _mediator.Send(new AddApiServer.Command { ViewModel = vm });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var response = await _mediator.Send(new DeleteApiServer.Command { Id = id });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        public async Task<ActionResult> Edit(EditApiServer.Query query)
        {
            var vm = await _mediator.Send(query);

            return View("AddEditApiServer", vm);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AddEditApiServerViewModel vm)
        {
            var response = await _mediator.Send(new EditApiServer.Command { ViewModel = vm });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> TestOdsApiConfiguration(TestApiServerConnection.Query query)
        {
            var result = await _mediator.Send(query);
            return new OkObjectResult(result);
        }

        [HttpPost]
        public async Task<string> InferOdsApiVersion(InferOdsApiVersion.Query query)
        {
            return await _mediator.Send(query);
        }
    }
}