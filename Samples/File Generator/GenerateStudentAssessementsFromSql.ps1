# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

$rootPath = "C:\Temp\DataImport\FileGenerators\"
$sqlfilePath = join-path $rootPath "StudentAssessments.sql"
$connectionStringFilePath = join-path $rootPath "ConnectionString.txt"
$outputFilePath = join-path $rootPath "StudentAssessments.csv"

$sql = (Get-Content $sqlfilePath)
$connectionString = (Get-Content $connectionStringFilePath)

if (Test-Path $outputFilePath) {
    Write-Information "Deleting $outputFilePath prior to regenerating it."
    Remove-Item -Path $outputFilePath
}

$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($sql, $connectionString)
try {
    $table = New-Object System.Data.DataTable
    $adapter.Fill($table) | Out-Null
    $table | Export-Csv -NoTypeInformation -Path $outputFilePath -Encoding UTF8
} finally {
    $adapter.Dispose()
}

return $outputFilePath
