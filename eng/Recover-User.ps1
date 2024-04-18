# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

<#
    .SYNOPSIS
        User can unlock the account and reset the password

    .DESCRIPTION
        User can unlock the account and reset the password

    .PARAMETER UserName
        Account user name. The user name should exist already, in order to reset the password.

    .PARAMETER NewPassword
        New password for the account. Password should contain at least one upper-case,
        lower-case, numeric-value, special-character with minimum length of 6.
        New password must be a SecureString ( $securePassword = ConvertTo-SecureString "YourPassword" -AsPlainText -Force)

    .PARAMETER UserRecoveryToken
        User recovery token provided during Data Import installation

    .PARAMETER ApplicationUrl
        Data Import application base url

    .EXAMPLE
    UserName: user1@gmail.com
    NewPassword: Password123$
    UserRecoveryToken: bEnFYNociET2R1Wua3DHzwfU5u
    ApplicationUrl: https://DataImportServer
  #>
param (
    [string]
    [Parameter(Mandatory = $true)]
    $UserName,

    [SecureString]
    [Parameter(Mandatory = $true)]
    $NewPassword,

    [string]
    [Parameter(Mandatory = $true)]
    $UserRecoveryToken,

    [string]
    [Parameter(Mandatory = $true)]
    $ApplicationUrl
)

# Convert SecureString to plain text
$ptr = [System.Runtime.InteropServices.Marshal]::SecureStringToGlobalAllocUnicode($NewPassword)
try {
    $newPasswordText = [System.Runtime.InteropServices.Marshal]::PtrToStringUni($ptr)
} finally {
    [System.Runtime.InteropServices.Marshal]::ZeroFreeGlobalAllocUnicode($ptr)
}

$Body = @{
    UserName          = $UserName
    NewPassword       = $newPasswordText
    UserRecoveryToken = $UserRecoveryToken
}

$contentType = 'application/x-www-form-urlencoded'
$Uri = "$ApplicationUrl/api/RecoverUser"

try {
    $apiResponse = (Invoke-WebRequest -Uri $Uri -Method Post -Body $Body -ContentType $contentType)
    $apiResponse
}
catch {
    Write-Information "Exception details: "
    $exception = $_.Exception
    Write-Information ("`tMessage: " + $exception.Message)
    Write-Information ("`tStatus code: " + $exception.Response.StatusCode)
    Write-Information ("`tStatus description: " + $exception.Response.StatusDescription)

    Write-Information "`tResponse: " -NoNewline
    $errorResponse = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($errorResponse)
    $reader.BaseStream.Position = 0
    $reader.DiscardBufferedData()
    $errorContent = $reader.ReadToEnd();
    Write-Information $errorContent -ForegroundColor red
}
