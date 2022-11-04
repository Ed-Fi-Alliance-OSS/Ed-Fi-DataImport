// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;

namespace DataImport.Web.Features.Shared
{
    public class ErrorViewModel
    {
        public ErrorViewModel(Exception exception, string controllerName, string actionName)
        {
            Exception = exception;
            ControllerName = controllerName;
            ActionName = actionName;
        }
        public string ActionName { get; }
        public string ControllerName { get; }
        public Exception Exception { get; }
    }
}
