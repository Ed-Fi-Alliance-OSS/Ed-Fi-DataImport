// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { DataImportPage } from "./dataImportPage";

enum importFilePaths {
  VALID = "./data/Import Data - Student Assessments.json",
  XML = "./data/Invalid.xml",
  INVALID_JSON = "./data/Empty.JSON",
  MAP_34 = "./data/Map for v34.json",
  BOOTSTRAP_34 = "./data/Bootstrap for v34.json",
}

export class ImportExportPage extends DataImportPage {
  importBtn = "button:text('import')";
  exportBtn = "a.btn-primary:text('export')";
  importTemplateBtn = "button:text('Import Template')";
  fileInput = "input#File";
  apiVersionSelect = "select#ApiVersion";

  messages = {
    noFileSelected: "'Import Template' must not be empty.",
  };

  path(): string {
    return `${this.url}/Share/FileIndex`;
  }

  async selectFile(file: string): Promise<void> {
    await this.uploadFile(this.fileInput, file);
  }

  async selectValidFile(): Promise<void> {
    await this.selectFile(importFilePaths.VALID);
  }

  async selectXMLFile(): Promise<void> {
    await this.selectFile(importFilePaths.XML);
  }

  async selectInvalidJSONFile(): Promise<void> {
    await this.selectFile(importFilePaths.INVALID_JSON);
  }

  async selectMapFor34File(): Promise<void> {
    await this.selectFile(importFilePaths.MAP_34);
  }

  async selectBootstrapFor34File(): Promise<void> {
    await this.selectFile(importFilePaths.BOOTSTRAP_34);
  }

  async clickImportTemplate(): Promise<void> {
    await this.page.locator(this.importTemplateBtn).click();
  }

  async selectODSVersion(value: string): Promise<void> {
    await this.page.locator(this.apiVersionSelect).selectOption({ value });
  }

  async selectODSVersionByIndex(index = 1): Promise<void> {
    await this.page.locator(this.apiVersionSelect).selectOption({ index });
  }

  async importTemplate(): Promise<void> {
    await Promise.all([
      this.waitForResponse("/Share/FileImport"),
      this.clickImportTemplate(),
      this.page.waitForNavigation(),
    ]);
  }

  async clickImport(): Promise<void> {
    await this.page.locator(this.importBtn).click();
  }

  private async clickExport(): Promise<void> {
    await this.page.locator(this.exportBtn).click();
  }

  async generateTemplateExport(): Promise<void> {
    await Promise.all([this.waitForResponse("FileExport"), this.clickExport()]);
  }

  async getTemplatePreview(): Promise<void> {
    await Promise.all([
      this.waitForResponse("FileImportUpload"),
      this.clickImport(),
    ]);
  }
}
