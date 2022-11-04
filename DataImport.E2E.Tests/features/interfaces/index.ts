// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

export interface Test {
  Feature: string;
  Scenario: string;
}

export interface ApiConnectionFormData {
  name: string;
  url: string;
  key: string;
  secret: string;
}

export interface MapTemplate {
  name: string;
  resourcePath: string;
}

export interface BootstrapTemplate {
  name: string;
  resourcePath: string;
}

export interface DataImportTemplateDetails {
  maps: Array<MapTemplate>;
  bootstraps: Array<BootstrapTemplate>;
  lookups: Array<unknown>;
}

export interface DataImportTemplateSummary {
  title: string;
  description: string;
  apiVersion: string;
  template: DataImportTemplateDetails;
}
