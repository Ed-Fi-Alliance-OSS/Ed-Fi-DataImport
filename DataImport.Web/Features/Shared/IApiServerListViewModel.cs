// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataImport.Web.Features.Shared
{
    public interface IApiServerListViewModel
    {
        List<SelectListItem> ApiServers { get; set; }

        [Display(Name = "API Connection")]
        int? ApiServerId { get; set; }
    }
}