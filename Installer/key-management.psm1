# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

<#
.SYNOPSIS
    Generates a new AES key.

.DESCRIPTION
    The New-AESKey function creates a new AES key using the .NET class System.Security.Cryptography.Aes.
    The key size is set to 256 bits, and the key is generated using the GenerateKey method.
    The key is then converted to a Base64 string for easier handling.

.PARAMETER
    This function does not take any parameters.

.EXAMPLE
    PS C:\> New-AESKey

    This command generates a new AES key and outputs it as a Base64 string.

.INPUTS
    None. You cannot pipe objects to New-AESKey.

.OUTPUTS
    System.String. New-AESKey returns the new AES key as a Base64 string.

.NOTES
    The AES key is generated in memory and not saved to disk by this function.
    Be sure to capture the output of this function if you need to use the key later.
#>

function New-AESKey {
    [CmdletBinding(SupportsShouldProcess = $true)]
    [OutputType([System.String])]
    param()
    <#
        .SYNOPSIS
            Generates and Base64-encodes a 256 bit key appropriate for use with AES encryption.

        .EXAMPLE
            Import-Module ./key-management.psm1
            New-AESKey

            # Copy the output and paste into the appsettings file.
    #>
    if ($PSCmdlet.ShouldProcess("AES Key", "New")) {
        $aes = [System.Security.Cryptography.Aes]::Create()
        $aes.KeySize = 256
        $aes.GenerateKey()
        [System.Convert]::ToBase64String($aes.Key)
    }
}

Export-ModuleMember -Function New-AESKey
