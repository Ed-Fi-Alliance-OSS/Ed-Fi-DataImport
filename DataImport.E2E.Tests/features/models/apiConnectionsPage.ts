// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { DataImportPage } from "./dataImportPage";
import { ApiConnectionFormData } from "../interfaces";

export class ApiConnectionsPage extends DataImportPage {
  apiConnectionName = "Automated API Connection";
  get updatedConnectionName(): string {
    return `${this.apiConnectionName} - Updated`;
  }
  addAPIBtn = ".container a.btn-primary";
  apiVersionSection = "#divApiVersion";
  apiLabel = `${this.apiVersionSection} #ApiVersionContent`;
  errorMessages = `#validationSummary li`;
  nameInput = "input#Name";
  urlInput = "input#Url";
  keyInput = "input#Key";
  secretInput = "input#Secret";
  testConnectionBtn = "button#btnTest";
  saveChangesBtn = "button#btnSave";
  table = "#tblApiServers";
  listRows = `${this.tableFunctions.tableBody} tr`;
  // Error message should be fixed after DI-1174 is closed
  messages = {
    invalidURL: "Please enter a valid ODS API URL",
    connectionSuccess: "Connection to API successful!",
    connectionError:
      "Unable to connect to the API: API Test threw an exception: Unable to retrieve an access token. Error message: ",
    connectionSaved: `Connection '${this.apiConnectionName}' was created.`,
    connectionUpdated: `Connection '${this.updatedConnectionName}' was modified.`,
    connectionDeleted: `Connection '${this.apiConnectionName}' was deleted.`,
  };

  path(): string {
    return `${this.url}/ApiServers`;
  }

  pathWhenAdding(): string {
    return `${this.path()}/Add`;
  }

  pathWhenEditing(id: string): string {
    return `${this.path()}/Edit/${id}`;
  }

  async addNewAPI(): Promise<void> {
    await this.page.locator(this.addAPIBtn).click();
  }

  async enterURL(url: string): Promise<void> {
    await this.page.locator(this.urlInput).fill(url);
    await this.page.locator(this.urlInput).press("Enter");
  }

  async waitForVersion(): Promise<void> {
    await this.page.locator(this.apiVersionSection).waitFor({
      state: "visible",
    });
  }

  async isApiSectionVisible(): Promise<boolean> {
    return await this.page.locator(this.apiVersionSection).isVisible();
  }

  async getAPIVersion(): Promise<string | null> {
    return await this.getText(this.apiLabel);
  }

  async getErrorMessages(): Promise<string | null> {
    return await this.getText(this.errorMessages);
  }

  async fillForm(formData: ApiConnectionFormData): Promise<void> {
    await this.page.locator(this.nameInput).fill(formData.name);
    await this.enterURL(formData.url);
    await this.page.locator(this.keyInput).fill(formData.key);
    await this.page.locator(this.secretInput).fill(formData.secret);
  }

  async testConnection(): Promise<void> {
    await this.page.locator(this.testConnectionBtn).click();
  }

  async waitForToast(): Promise<void> {
    await this.page.locator(this.toast).waitFor({
      state: "visible",
    });
  }

  async waitForSaveButton(): Promise<void> {
    await this.waitForButtonEnabled(this.saveChangesBtn);
  }

  isSaveButtonEnabled(): Promise<boolean> {
    return this.page.locator(this.saveChangesBtn).isEnabled();
  }

  async saveConnection(): Promise<void> {
    await this.page.locator(this.saveChangesBtn).click();
  }

  async editAdded(): Promise<string> {
    await this.tableFunctions.waitForTable();
    const url = await this.tableFunctions.getHrefURL({
      tableRowsSelector: this.listRows,
      rowTitle: this.apiConnectionName,
      type: "Edit",
    });
    if (url === "") {
      throw "Couldn't find api connection href to click edit. Verify that item is present";
    }
    await this.clickLinkByURL(url);
    return url.substr(url.lastIndexOf("/") + 1, url.length);
  }

  async clickDelete(): Promise<void> {
    await this.tableFunctions.waitForTable();
    const url = await this.tableFunctions.getHrefURL({
      tableRowsSelector: this.listRows,
      rowTitle: this.apiConnectionName,
      type: "Delete",
    });
    if (url === "") {
      throw "Couldn't find api connection href to click delete. Verify that item is present";
    }
    this.acceptModal();
    await this.clickLinkByURL(url);
  }

  async addInformation(
    type: string,
    name = this.apiConnectionName
  ): Promise<void> {
    await this.fillForm(<ApiConnectionFormData>{
      name,
      url: process.env.API_URL,
      key: process.env.key,
      secret: type === "valid" ? process.env.secret : "fail",
    });
  }

  async updateForm(): Promise<void> {
    await this.fillForm(<ApiConnectionFormData>{
      name: this.updatedConnectionName,
      url: process.env.API_URL,
      key: process.env.key,
      secret: process.env.secret,
    });
  }
}
