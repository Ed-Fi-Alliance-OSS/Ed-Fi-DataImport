// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
using DataImport.Common;
using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DataImport.Web.Features.School
{
    public class SchoolController : BaseController
    {
        private readonly IMediator _mediator;

        public SchoolController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index(Index.Query query)
        {
            return View(await _mediator.Send(query));
        }

        public async Task<ActionResult> Details(SchoolDetails.Query query)
        {
            return View(await _mediator.Send(query));
        }

        public async Task<ActionResult> StudentDetails(StudentDetails.Query query)
        {
            return PartialView("_StudentDetails", await _mediator.Send(query));
        }

        public async Task<ActionResult> StaffDetails(StaffDetails.Query query)
        {
            return PartialView("_StaffDetails", await _mediator.Send(query));
        }

        public async Task<ActionResult> SectionDetails(SectionDetails.Query query)
        {
            return PartialView("_SectionDetails", await _mediator.Send(query));
        }
    }
}