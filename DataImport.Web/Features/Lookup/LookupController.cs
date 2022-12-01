// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DataImport.Web.Features.Lookup
{
    public class LookupController : BaseController
    {
        private readonly IMediator _mediator;

        public LookupController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index()
        {
            return View(await _mediator.Send(new LookupIndex.Query()));
        }

        public ActionResult Add()
        {
            return View(new AddLookup.Command());
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddLookup.Command command)
        {
            var response = await _mediator.Send(command);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        public async Task<ActionResult> Edit(int id)
        {
            return View(await _mediator.Send(new EditLookup.Query { Id = id }));
        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditLookup.Command command)
        {
            var response = await _mediator.Send(command);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(DeleteLookup.Command command)
        {
            var response = await _mediator.Send(command);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }
    }
}
