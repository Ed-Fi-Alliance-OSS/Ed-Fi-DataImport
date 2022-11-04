// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Assessment
{
    public class AssessmentController : BaseController
    {
        private readonly IMediator _mediator;

        public AssessmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index(AssessmentIndex.Query query)
        {
            return View(await _mediator.Send(query));
        }

        public async Task<ActionResult> Details(AssessmentDetails.Query query)
        {
            return View(await _mediator.Send(query));
        }

        public async Task<ActionResult> ObjectiveAssessmentDetails(ObjectiveAssessmentDetails.Query query)
        {
            return PartialView("_ObjectiveAssessmentDetails", await _mediator.Send(query));
        }
    }
}