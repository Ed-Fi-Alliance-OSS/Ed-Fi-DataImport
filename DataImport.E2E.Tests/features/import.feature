# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

Feature: Import

  Import template option

  Background: User is logged in and it's in the correct page
    Given user is logged in
    And it's on the "Import / Export Templates" page

  #DI-1020
  @ImportData
  Scenario: Select file to Import
    When selecting a valid file to import
    And clicking import
    Then import details load
    And imported file version warning appears
    And imported file preview appears

  #DI-1021
  @ImportData @Sanity
  Scenario: Import Data
    When selecting a valid file to import
    And clicking import
    And clicking on Import Template
    Then selected template is imported
    And it's redirected to the "Maps" page
    And Map appears on list
    And Bootstrap appears on list

  #DI-1022
  Scenario Outline: Import Data - File Validation
    When selecting <Scenario> to import
    And clicking import
    Then error message for import scenario appears

    Examples:
      | Scenario |
      | no file  |
# | non json     | - Requires DI-1027 to be fixed
# | invalid json | - Requires DI-1028 to be fixed