// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { DataImportPage } from "./dataImportPage";

export class TemplateSharingPage extends DataImportPage {
  templateInformation = {
    organization: "",
    templateName: "",
    odsVersion: "",
    formURL: "",
  };
  templateFilterInput = "input#Title";
  organizationInput = "input#Organization";
  filterBtn = "button#filter";
  table = "#tblAvailableTemplates";
  apiVersionSelect = "select#ApiVersion";
  viewLinkSelector = "td.footable-last-visible a";
  versionInformation = ".alert-info p";
  templatePreviewArea = "textarea#Template";
  formSelector = "div form[data-callback='redirect']";
  importBtn = "button#btnSubmit";

  path(): string {
    return `${this.url}/Share/ExchangeIndex`;
  }

  detailsPath(): string {
    return `${this.url}/Share/ExchangeImport`;
  }

  async filterByTemplate(text: string): Promise<void> {
    await this.page.fill(this.templateFilterInput, text);
  }

  async filterByOrg(text: string): Promise<void> {
    await this.page.fill(this.organizationInput, text);
  }

  async selectByVersion(value: string): Promise<void> {
    await this.page.locator(this.apiVersionSelect).selectOption({ value });
  }

  async filterList(): Promise<void> {
    await this.page.locator(this.filterBtn).click();
  }

  async isListFiltered(filter: {
    field: string;
    data: string;
  }): Promise<boolean> {
    return (await this.tableFunctions.getAllRowsFromColumn(filter.field)).every(
      (row) => row.includes(filter.data)
    );
  }

  async clickView(row = 1): Promise<void> {
    const tableRow = `${this.tableFunctions.tableBody} tr:nth-child(${row})`;
    const viewLink = `${tableRow} ${this.viewLinkSelector}`;
    await this.page.locator(viewLink).click();
  }

  async hasSubmitterOrganization(): Promise<boolean> {
    return await this.hasText(this.templateInformation.organization);
  }

  async hasTemplateTitle(): Promise<boolean> {
    return await this.hasText(this.templateInformation.templateName);
  }

  async setData(): Promise<void> {
    const templateName = await this.tableFunctions.getCellContent(
      "Template Name"
    );
    const organization = await this.tableFunctions.getCellContent(
      "Organization"
    );
    const odsVersion = await this.tableFunctions.getCellContent(
      "Ed-Fi ODS / API Version"
    );

    if (templateName && organization && odsVersion) {
      this.templateInformation.templateName = templateName;
      this.templateInformation.organization = organization;
      this.templateInformation.odsVersion = odsVersion;
    } else {
      throw "Unable to read template data";
    }
  }

  async setFormURL(): Promise<void> {
    const formURL = await this.getFormActionURL(this.formSelector);
    if (formURL) {
      this.templateInformation.formURL = formURL;
    } else {
      throw "Unable to read form URL";
    }
  }

  async getVersionWarning(): Promise<string | null> {
    return await this.getText(this.versionInformation);
  }

  async getPreview(): Promise<string | null> {
    return await this.getText(this.templatePreviewArea);
  }

  async selectODSVersion(): Promise<void> {
    await this.page.locator(this.apiVersionSelect).selectOption({ index: 1 });
  }

  async clickImport(): Promise<void> {
    await this.waitForButtonEnabled(this.importBtn);
    await this.page.locator(this.importBtn).click();
  }

  async verifyTemplateImported(): Promise<void> {
    await Promise.all([
      this.waitForResponse(this.templateInformation.formURL),
      this.clickImport(),
    ]);
  }
}
