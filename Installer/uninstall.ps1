# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

import-module -force "$PSScriptRoot/Install-EdFiDataImport.psm1"

<#
This script will uninstall your existing Data Import installation.

.EXAMPLE
    $p = @{
        ToolsPath = "C:/temp/tools"
        WebSiteName="Ed-Fi-3"
        WebApplicationPath="c:/inetpub/Ed-Fi-3/DataImport-3"
        WebApplicationName = "DataImport-3"
    }
#>

$p = @{
    ToolsPath = "C:/temp/tools"
}

Uninstall-EdFiDataImport @p