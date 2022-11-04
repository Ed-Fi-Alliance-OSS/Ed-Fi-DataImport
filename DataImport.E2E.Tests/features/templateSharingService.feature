# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

Feature: Template Sharing Service

  Tests for TSS.

  Background: User is logged in and it's in the correct page
    Given user is logged in
    And it has the correct Template Sharing URL
    And it's on the "Template Sharing" page

  #DI-1006
  Scenario Outline: List Filter
    When entering template sharing scenario: <Data> in <Filter>
    Then TSS list is filtered

    Examples:
      | Filter        | Data               |
      | Template Name | Student Assessment |
      | Organization  | Ed-Fi              |
      | ODS Version   | 5.                 |
      | ODS Version   | 3.                 |

  #DI-1012
  @Sanity
  Scenario: View Detail
    When clicking view detail
    Then template details load
    And template details version warning appears
    And template preview appears

#This test must be skipped until the functionality to create a custom template has been solved DI-1011
# DI-1013
# Scenario: Template Sharing Service - Import
#     When clicking view detail
#     And selecting an ODS version
#     Then template is imported
