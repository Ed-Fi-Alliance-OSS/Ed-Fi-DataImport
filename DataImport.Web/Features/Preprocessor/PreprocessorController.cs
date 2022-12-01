// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DataImport.Web.Features.Preprocessor
{
    public class PreprocessorController : BaseController
    {
        private readonly IMediator _mediator;
        public PreprocessorController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index()
        {
            var viewModel = await _mediator.Send(new PreprocessorIndex.Query());

            return View(viewModel);
        }

        public async Task<ActionResult> Add()
        {
            var viewModel = await _mediator.Send(new AddPreprocessor.Query());

            return View("AddEditPreprocessor", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddEditPreprocessorViewModel vm)
        {
            var response = await _mediator.Send(new AddPreprocessor.Command { ViewModel = vm });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var response = await _mediator.Send(new DeletePreprocessor.Command { Id = id });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        public async Task<ActionResult> Edit(EditPreprocessor.Query query)
        {
            var vm = await _mediator.Send(query);

            return View("AddEditPreprocessor", vm);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(AddEditPreprocessorViewModel vm)
        {
            var response = await _mediator.Send(new EditPreprocessor.Command { ViewModel = vm });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }
    }
}
