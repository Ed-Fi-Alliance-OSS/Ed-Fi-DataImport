// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace DataImport.Web.Features.Shared
{
    public class CommonController : BaseController
    {
        private readonly IMediator _mediator;

        public CommonController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<ActionResult> RetrieveResources(RetrieveResources.Query query)
        {
            return Json(await _mediator.Send(query));
        }
    }
}