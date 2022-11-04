# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.


$confFile = "$PSScriptRoot/.tssconf"

Write-Host "== Template Sharing Service User Enrollment ==`n" -ForegroundColor Green

#########################################################################################
###   HELPERS
#########################################################################################

function Write-Labeled-Value ($label, $value){
  Write-Host "$($label): " -NoNewline
  Write-Host $value -ForegroundColor Cyan
}

function Write-Middle-Highlight ($before, $highlighted, $after){
  Write-Host $before -NoNewline
  Write-Host $highlighted -NoNewline -ForegroundColor Cyan
  Write-Host $after
}

function Prompt-YN-Retry-Loop ($prompt, $default){
  $result = Read-Host -Prompt $prompt

  if($result -eq '') {
    return $default
  }
  if($result -ne 'y' -and $result -ne 'n') {
    return Prompt-YN-Retry-Loop $prompt $default
  }

  return $result
}

#########################################################################################
###   WORKFLOW
#########################################################################################

function Prompt-For-Config {
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
  Write-Host "Config file saved" -ForegroundColor Green
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
    Write-Host "Authentication failed: " -NoNewLine -ForegroundColor Red
    Write-Host $_.Exception.Message

    $retry = Prompt-YN-Retry-Loop 'Re-enter configuration and try again [y/N]?' 'n'
    if ($retry -eq "y") {
      $conf = Prompt-For-Config
      return Get-Auth-Token $conf
    }else {
      Write-Middle-Highlight "See " $confFile " for configuration info"
      Write-Host "`nExiting"
      exit
    }
  }

  Write-Host "`nAuthenticated successfully" -ForegroundColor Green
  return $authResponse.access_token
}

function Prompt-For-Client {
  Write-Host "`nInput new client details:" -ForegroundColor Yellow

  $clientName = Read-Host -Prompt "Client/Organization Name"
  $clientFullName = Read-Host -Prompt "Full Name"
  $clientEmail = Read-Host -Prompt "Email"

  Write-Host "`nConfirm details:" -ForegroundColor Yellow
  Write-Labeled-Value "Organization" $clientName
  Write-Labeled-Value "Full Name" $clientFullName
  Write-Labeled-Value "Email" $clientEmail

  $retry = Prompt-YN-Retry-Loop 'Accept details [Y/n]?' 'y'
  if ($retry -eq "n") {
    return Prompt-For-Client
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
  Write-Middle-Highlight "Configuration file (" $confFile ") not found"
  Write-Host "Please input Template Sharing configuration:" -ForegroundColor Yellow

  $conf = Prompt-For-Config
} Else {
  Write-Host "Loading configuration from file"
  try {
    $conf = Get-Content -Path $confFile | ConvertFrom-Json
  }
  catch {
    Write-Host "Failed to load configuration: " -NoNewline -ForegroundColor Red
    Write-Host $_.Exception.Message

    $retry = Prompt-YN-Retry-Loop 'Re-enter configuration and try again [y/N]?' 'n'
    if ($retry -eq "y") {
      $conf = Prompt-For-Config
    }else {
      Write-Middle-Highlight "See " $confFile " for configuration info"
      Write-Host "`nExiting"
      exit
    }
  }
}

$tssClientUrl = "$($conf.Url)/identity/api/client"

$token = Get-Auth-Token $conf

$client = Prompt-For-Client

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

  Write-Host "`nClient created successfully" -ForegroundColor Green

  Write-Host "`nClient details:" -ForegroundColor Yellow
  Write-Labeled-Value "Organization" $client.Name
  Write-Labeled-Value "Full Name" $client.FullName
  Write-Labeled-Value "Email" $client.Email
  Write-Labeled-Value "Client Key" $client.Id
  Write-Labeled-Value "Client Secret" $client.Secret
} catch {
  Write-Host "Client creation failed failed: " -NoNewLine -ForegroundColor Red
  Write-Host $_.Exception.Message
  Write-Host "Please check info and try again. See full exception below`n"
  throw $_
}

Write-Host "`nExiting"
