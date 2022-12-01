// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataImport.Web.Features.BootstrapData
{
    public class BootstrapDataController : BaseController
    {
        private readonly IMediator _mediator;

        public BootstrapDataController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index()
        {
            return View(await _mediator.Send(new BootstrapDataIndex.Query()));
        }

        public async Task<ActionResult> Add()
        {
            return View(await _mediator.Send(new AddBootstrapData.Query()));
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddBootstrapData.Command command)
        {
            var response = await _mediator.Send(command);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        public async Task<ActionResult> Edit(EditBootstrapData.Query query)
        {
            return View(await _mediator.Send(query));
        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditBootstrapData.Command command)
        {
            var response = await _mediator.Send(command);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(DeleteBootstrapData.Command command)
        {
            var response = await _mediator.Send(command);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }
    }
}
