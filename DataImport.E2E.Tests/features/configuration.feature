# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

Feature: Configuration

  Configuration page

  Background: User is logged in and it's in the correct page
    Given user is logged in
    And it's on the "Configuration" page

  #DI-1017
  Scenario: Allow User Registration
    Given user registration is disabled
    When clicking on enable user registration
    And clicking on update configuration
    Then configuration option is enabled
    And register option is present on login page

  #DI-1018
  Scenario: Disable User Registration
    Given user registration is enabled
    When clicking on disable user registration
    And clicking on update configuration
    Then configuration option is disabled
    And register option is not present on login page

  #DI-1061
  Scenario: Enable Product Improvement
    Given product improvement is disabled
    When clicking on enable product improvement
    And clicking on update configuration
    Then configuration option is enabled
    And analytics tag is present

  #DI-1062
  Scenario: Disable Product Improvement
    Given product improvement is enabled
    When clicking on disable product improvement
    And clicking on update configuration
    Then configuration option is disabled
    And analytics tag is not present