// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { DataImportPage } from "./dataImportPage";

export class ExportTemplatePage extends DataImportPage {
  continueBtn = "button:text('Continue'):visible";
  titleInput = "input#Title";
  descriptionInput = "input#Description";
  previewBtn = "div#step-collect-metadata button.btn-primary";
  downloadBtn = "#step-preview button.btn-primary";
  downloadedPath = "";
  errorMessages = "#validationSummary";

  messages = {
    noTitle: "'Title' must not be empty.",
    noDescription: "'Description' must not be empty.",
  };

  mapTablePosition = {
    row: -1,
    column: -1,
  };
  bootstrapTablePosition = {
    row: -1,
    column: -1,
  };

  path(): string {
    return `${this.url}/Share/FileExport`;
  }

  async hasMapsSubtitle(): Promise<boolean> {
    return await this.hasSubtitle("Data Maps");
  }

  async hasBootstrapSubtitle(): Promise<boolean> {
    return await this.hasSubtitle("Bootstrap Data");
  }

  async hasSummarySubtitle(): Promise<boolean> {
    return await this.hasSubtitle("Template Summary");
  }

  async hasPreviewSubtitle(): Promise<boolean> {
    return await this.hasSubtitle("Preview");
  }

  async downloadFile(): Promise<void> {
    const [download] = await Promise.all([
      this.page.waitForEvent("download"),
      this.page.locator(this.downloadBtn).click(),
    ]);
    const path = await download.path();
    if (!path) {
      throw "Unable to find downloaded file";
    }
    this.downloadedPath = path;
  }

  async selectMap(currentVersion: string): Promise<void> {
    if (!currentVersion) {
      throw "API version is required to select related map";
    }
    this.tableFunctions.table = "#step-select-data-maps table";
    await this.tableFunctions.waitForTable();
    this.mapTablePosition = await this.tableFunctions.markCheckboxByValueInRow({
      searchedValue: `Automated - SA Map for ${currentVersion}`,
      columnForValue: "Data Map Name",
      checkboxColumn: "Export?",
    });
  }

  async isMapChecked(): Promise<boolean> {
    if (
      this.mapTablePosition.row === -1 &&
      this.mapTablePosition.column === -1
    ) {
      throw "Position of map in table is not set";
    }
    return await this.page
      .locator(
        `${this.tableFunctions.tableBody} tr:nth-child(${this.mapTablePosition.row}) > td:nth-child(${this.mapTablePosition.column}) input[type="checkbox"]`
      )
      .isChecked();
  }

  async getErrorMessages(): Promise<string | null> {
    return await this.getText(this.errorMessages);
  }

  async selectBootstrap(currentVersion: string): Promise<void> {
    if (!currentVersion) {
      throw "API version is required to select related bootstrap";
    }
    this.tableFunctions.table = "#step-select-bootstraps table";
    await this.tableFunctions.waitForTable();
    this.bootstrapTablePosition =
      await this.tableFunctions.markCheckboxByValueInRow({
        searchedValue: `Automated - SA Bootstrap for ${currentVersion}`,
        columnForValue: "Bootstrap Name",
        checkboxColumn: "Export?",
      });
  }

  async isBootstrapChecked(): Promise<boolean> {
    if (
      this.bootstrapTablePosition.row === -1 &&
      this.bootstrapTablePosition.column === -1
    ) {
      throw "Position of bootstrap in table is not set";
    }
    return await this.page
      .locator(
        `${this.tableFunctions.tableBody} tr:nth-child(${this.bootstrapTablePosition.row}) > td:nth-child(${this.bootstrapTablePosition.column}) input[type="checkbox"]`
      )
      .isChecked();
  }

  async clickContinue(): Promise<void> {
    await this.page.locator(this.continueBtn).click();
  }

  async setTitle(): Promise<void> {
    await this.page
      .locator(this.titleInput)
      .fill("Automation - Student Assessment");
  }

  async setDescription(): Promise<void> {
    await this.page
      .locator(this.descriptionInput)
      .fill("This is the automation test for the export");
  }

  private async clickPreview(): Promise<void> {
    await this.page.locator(this.previewBtn).click();
  }

  async generatePreview(status = 200): Promise<void> {
    await Promise.all([
      this.waitForResponse("/Share/FileExport", status),
      this.clickPreview(),
    ]);
  }

  async isCheckDisabled(currentVersion: string): Promise<boolean> {
    this.tableFunctions.table = "#step-select-bootstraps table";
    await this.tableFunctions.waitForTable();
    const position = await this.tableFunctions.getXYPosition({
      searchedValue: `Automated - SA Bootstrap for ${currentVersion}`,
      columnForValue: "Bootstrap Name",
      checkboxColumn: "Export?",
    });

    const locator = this.page.locator(
      `${this.tableFunctions.tableBody} tr:nth-child(${position.row}) > td:nth-child(${position.column}) input[type="checkbox"]`
    );
    return await locator.isHidden();
  }
}
