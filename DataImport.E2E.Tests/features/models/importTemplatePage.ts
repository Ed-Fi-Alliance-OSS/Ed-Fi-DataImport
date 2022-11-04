// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { DataImportPage } from "./dataImportPage";
import {
  DataImportTemplateSummary,
  DataImportTemplateDetails,
  MapTemplate,
  BootstrapTemplate,
} from "../interfaces";
import { getJSONFileContent } from "../management/functions";

enum importFilePaths {
  VALID = "./data/Import Data - Student Assessments.json",
}

export class ImportTemplatePage extends DataImportPage {
  versionInformation = ".alert-info p";
  templatePreviewArea = "textarea#Template";
  errorMessages = ".validation-summary-errors li";

  messages = {
    templateImported: "",
  };

  private _testData: DataImportTemplateSummary | undefined;
  get testData(): DataImportTemplateSummary {
    if (!this._testData) {
      throw "Test Data not set";
    }
    return this._testData;
  }

  get template(): DataImportTemplateDetails {
    if (this.testData?.template) {
      return this.testData?.template;
    }
    throw "Template not set";
  }

  get map(): MapTemplate {
    if (this.testData?.template?.maps) {
      return this.testData.template.maps[0];
    }
    throw "Map not found";
  }

  get bootstrap(): BootstrapTemplate {
    if (this.testData?.template?.bootstraps) {
      return this.testData.template.bootstraps[0];
    }
    throw "Bootstrap not found";
  }

  async setData(): Promise<void> {
    this._testData = await getJSONFileContent<DataImportTemplateSummary>(
      importFilePaths.VALID
    );

    this.messages.templateImported = `Template '${this.testData?.title}' was imported.`;
  }

  path(): string {
    return `${this.url}/Share/FileImportUpload`;
  }

  async hasTemplateTitle(): Promise<boolean> {
    return await this.hasText(this.testData.title);
  }

  async hasDescription(): Promise<boolean> {
    return await this.hasText(this.testData.description);
  }

  async getVersionWarning(): Promise<string | null> {
    return await this.getText(this.versionInformation);
  }

  async getErrorMessages(): Promise<string | null> {
    return await this.getText(this.errorMessages);
  }

  async hasVersionWarning(): Promise<boolean | undefined> {
    return (await this.getVersionWarning())?.includes(this.testData.apiVersion);
  }

  async getPreview(): Promise<DataImportTemplateDetails> {
    const preview = await this.getText(this.templatePreviewArea);
    if (preview) {
      return JSON.parse(preview) as DataImportTemplateDetails;
    }
    throw "Preview not found";
  }

  // async hasExpectedPreview(): Promise<boolean> {
  //   let preview = await this.getPreview();
  //   let template = this.testData?.template;
  //   if (preview && template)
  //   {
  //     return (JSON.parse(preview) as DataImportTemplateDetails) === template;
  //   }
  //   return false;
  // }
}
