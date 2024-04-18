# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.


$confFile = "$PSScriptRoot/.tssconf"

Write-Information "== Template Sharing Service User Enrollment ==`n"

#########################################################################################
###   HELPERS
#########################################################################################

function Write-Labeled-Value ($label, $value){
  Write-Information "$($label): " -NoNewline
  Write-Information $value
}

function Write-Middle-Highlight ($before, $highlighted, $after){
  Write-Information $before
  Write-Information $highlighted
  Write-Information $after
}

function Invoke-YesNoRetryLoop  ($prompt, $default){
  $result = Read-Host -Prompt $prompt

  if($result -eq '') {
    return $default
  }
  if($result -ne 'y' -and $result -ne 'n') {
    return Invoke-YesNoRetryLoop  $prompt $default
  }

  return $result
}

#########################################################################################
###   WORKFLOW
#########################################################################################

function Get-ConfigInput {
  $tssUrl = Read-Host -Prompt "Template sharing URL (Press Enter for  ""https://template-sharing.ed-fi.org"")"
  If ('' -eq $tssUrl) { $tssUrl = 'https://template-sharing.ed-fi.org' }
  $tssAuthId = Read-Host -Prompt "Client ID (Press Enter for ""Administrator"")"
  If ('' -eq $tssAuthId) { $tssAuthId = 'Administrator' }
  $tssAuthSecret = Read-Host -Prompt "Client Secret"

  $conf = @{
    Url = $tssUrl
    Id = $tssAuthId
    Secret = $tssAuthSecret
  }

  Set-Content -Path $confFile -Value ($conf | ConvertTo-Json)
  Write-Information "Config file saved"
  return $conf
}

function Get-Auth-Token ($conf) {
  $authUrl = "$($conf.Url)/identity/connect/token"
  $authRequest = @{
    Grant_Type = "client_credentials"
    Client_Id = ($conf.Id)
    Client_Secret = ($conf.Secret)
  }
  try {
    $authResponse = Invoke-RestMethod -Method 'Post' -Uri $authUrl -Body $authRequest -ContentType "application/x-www-form-urlencoded"
  } catch {
    Write-Error "Authentication failed: "
    Write-Error $_.Exception.Message

    $retry = Invoke-YesNoRetryLoop  'Re-enter configuration and try again [y/N]?' 'n'
    if ($retry -eq "y") {
      $conf = Get-ConfigInput
      return Get-Auth-Token $conf
    }else {
      Write-Middle-Highlight -before "See " -highlighted $confFile -after " for configuration info"
      Write-Information "`nExiting"
      exit
    }
  }

  Write-Information "`nAuthenticated successfully"
  return $authResponse.access_token
}

function Get-ClientInput  {
    Write-Information "`nInput new client details:"

  $clientName = Read-Host -Prompt "Client/Organization Name"
  $clientFullName = Read-Host -Prompt "Full Name"
  $clientEmail = Read-Host -Prompt "Email"

  Write-Information "`nConfirm details:"
  Write-Labeled-Value "Organization" $clientName
  Write-Labeled-Value "Full Name" $clientFullName
  Write-Labeled-Value "Email" $clientEmail

  $retry = Invoke-YesNoRetryLoop  'Accept details [Y/n]?' 'y'
  if ($retry -eq "n") {
    return Get-ClientInput
  }

  return @{
    Name = $clientName
    FullName = $clientFullName
    Email = $clientEmail
    Id = ($(New-Guid).ToString("N").subString(16))
    Secret = ($(New-Guid).ToString("N"))
  }
}

#########################################################################################
###   MAIN
#########################################################################################

If (-not(Test-Path -Path $confFile)) {
  Write-Middle-Highlight -before "Configuration file (" -highlighted $confFile -after ") not found"
  Write-Information "Please input Template Sharing configuration:"

  $conf = Get-ConfigInput
} Else {
  Write-Information "Loading configuration from file"
  try {
    $conf = Get-Content -Path $confFile | ConvertFrom-Json
  }
  catch {
    Write-Information "Failed to load configuration: "
    Write-Information $_.Exception.Message

    $retry = Invoke-YesNoRetryLoop  'Re-enter configuration and try again [y/N]?' 'n'
    if ($retry -eq "y") {
      $conf = Get-ConfigInput
    }else {
      Write-Middle-Highlight -before "See " -highlighted $confFile -after " for configuration info"
      Write-Information "`nExiting"
      exit
    }
  }
}

$tssClientUrl = "$($conf.Url)/identity/api/client"

$token = Get-Auth-Token $conf

$client = Get-ClientInput

$addClientJson =@{
  clientId = ($client.Id)
  clientSecret = ($client.Secret)
  clientName = ($client.Name)
  claims = @(
     @{
        type ="role"
        value ="Consumer"
     },
     @{
        type = "role"
        value = "Publisher"
     }
  )

  submitter = @{
     name = ($client.FullName)
     organization = ($client.Name)
     email = ($client.Email)
  }
}

$headers = @{
  'Authorization' = "Bearer $token"
}

try {
  Invoke-RestMethod -Method 'Post' -Uri $tssClientUrl -Body ($addClientJson | ConvertTo-Json) -Headers $headers -ContentType "application/json"

  Write-Information "`nClient created successfully"

  Write-Information "`nClient details:"
  Write-Labeled-Value "Organization" $client.Name
  Write-Labeled-Value "Full Name" $client.FullName
  Write-Labeled-Value "Email" $client.Email
  Write-Labeled-Value "Client Key" $client.Id
  Write-Labeled-Value "Client Secret" $client.Secret
} catch {
  Write-Information "Client creation failed failed: "
  Write-Information $_.Exception.Message
  Write-Information "Please check info and try again. See full exception below`n"
  throw $_
}

Write-Information "`nExiting"
