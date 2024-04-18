# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

function Invoke-RegenerateFile {
    param (
        [string]
        $Path,

        [string]
        $NewContent
    )

    $oldContent = Get-Content -Path $Path

    if ($new_content -ne $oldContent) {
        $relative_path = Resolve-Path -Relative $Path
        Write-Information "Generating $relative_path"
        [System.IO.File]::WriteAllText($Path, $NewContent, [System.Text.Encoding]::UTF8)
    }
}

function Invoke-Execute {
    param (
        [ScriptBlock]
        $Command
    )

    $global:lastexitcode = 0
    & $Command

    if ($lastexitcode -ne 0) {
        throw "Error executing command: $Command"
    }
}

function Invoke-Step {
    param (
        [ScriptBlock]
        $block
    )

    $command = $block.ToString().Trim()

    Write-Information $command

    &$block
}

function Invoke-Main {
    param (
        [ScriptBlock]
        $MainBlock
    )

    try {
        &$MainBlock
        Write-Information "Build Succeeded"
        exit 0
    } catch [Exception] {
        Write-Error $_.Exception.Message
        Write-Error "Build Failed"
        exit 1
    }
}

Export-ModuleMember -Function *
