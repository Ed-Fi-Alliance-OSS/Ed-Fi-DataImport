// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Activity
{
    public class ActivityController : BaseController
    {
        private readonly IMediator _mediator;

        public ActivityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public ViewResult Index(GetActivity.Query query)
        {
            return View(new GetActivity.ViewModel
            {
                ApiServerId = query.ApiServerId
            });
        }

        public async Task<ActionResult> Activity(GetActivity.Query query)
        {
            ViewBag.SelectApiConnectionAction = "Index";
            return PartialView("_Activity", await _mediator.Send(query));
        }
    }
}
