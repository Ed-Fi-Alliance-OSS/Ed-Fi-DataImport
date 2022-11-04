# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

Feature: API Connections

  Tests for API Connections page. /DataImport/ApiServers

  Background: User is logged in and it's in the correct page
    Given user is logged in
    Given it's on the "API Connections" page

  #DI-998
  Scenario: Infer API Version
    When adding API connection
    And inputs valid API URL
    Then API version appears

  #DI-999
  Scenario: Invalid API version
    When adding API connection
    And inputs invalid API URL
    Then API validation message appears

  #DI-1000
  Scenario: Test API connection success
    When adding API connection
    And adding valid information
    And testing connection
    And connection success message appears

  #DI-1001
  Scenario: Test API connection error
    When adding API connection
    And adding invalid information
    And testing connection
    Then connection error message appears

  #DI-1002
  @Sanity
  Scenario: Add API connection
    When adding API connection
    And adding valid information
    And testing connection
    And clicking on save connection
    Then connection can be saved
    And added connection appears on list

  #DI-1003
  Scenario: Delete API connection
    Given there's an API connection added
    When deleting API connection
    Then connection is deleted

  #DI-1004
  Scenario: Edit API connection
    Given there's an API connection added
    When editing API connection
    And updating information
    And testing connection
    And clicking on save connection
    Then connection can be updated
    And updated connection appears on list