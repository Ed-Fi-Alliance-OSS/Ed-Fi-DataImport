# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

Feature: Log in

  #DI-994
  @Sanity
  Scenario: Log in successful
    Given it's on the "Log in" page
    When user enters valid username and password
    And clicks Log in
    Then login is successful

  #DI-995
  Scenario Outline: Log in validation
    Given it's on the "Log in" page
    When user enters login scenario: <Scenario>
    And clicks Log in
    Then login validation message appears

    Examples:
      | Scenario       |
      | no data        |
      | invalid email  |
      | wrong email    |
      | wrong password |
