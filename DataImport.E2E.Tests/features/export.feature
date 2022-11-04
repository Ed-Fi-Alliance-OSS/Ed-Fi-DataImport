# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

@ImportData
Feature: Export

  Export template option

  Background: User is logged in and it's in the correct page
    Given user is logged in
    And there's a template imported
    And it's on the "Import / Export Templates" page

  #DI-1002
  @Sanity
  Scenario: Export File
    When clicking export
    And selecting Map
    And selecting Bootstrap
    And entering title and description in export form
    And clicking preview
    Then export preview loads
    And file can be downloaded
    And downloaded file is valid

  #DI-1034
  Scenario Outline: Export Data - Title Validation
    When clicking export
    And selecting Map
    And selecting Bootstrap
    And entering <Scenario> in export form
    And clicking preview for invalid request
    Then error message for export scenario appears

    Examples:
      | Scenario    |
      | title       |
      | description |
      | no data     |

  #DI-1024
  Scenario: Export Data - Map for different versions
    When adding Map for previous version
    And clicking export
    And selecting Map
    Then Bootstrap step will not appear

  #DI-1025
  Scenario: Export Data - Bootstrap for different versions
    When adding Bootstrap for previous version
    And clicking export
    And selecting Map
    Then added Bootstrap will not have checkbox