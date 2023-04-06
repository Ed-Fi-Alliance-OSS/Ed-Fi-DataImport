// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { API_Versions } from "../enums";
import { DataImportPage } from "./dataImportPage";

export class BootstrapPage extends DataImportPage {
  private _addedBootstraps: Array<string> = [];

  get bootstrapFor52(): string {
    return this.getBootstrap(API_Versions.Version52);
  }

  get mapFor340(): string {
    return this.getBootstrap(API_Versions.Version34);
  }

  table = "table#tblBootstrapData";
  listRows = `${this.tableFunctions.tableBody} tr`;

  path(): string {
    return `${this.url}/BootstrapData`;
  }

  getBootstrap(name: string): string {
    const bootstrap = this._addedBootstraps.find((b) => b === name);
    if (bootstrap) {
      return bootstrap;
    }
    throw `Bootstrap ${name} not set`;
  }

  addBootstrap(newBootstrap: string): void {
    if (!this._addedBootstraps.find((b) => b === newBootstrap)) {
      this._addedBootstraps.push(newBootstrap);
    }
  }

  async deleteBootstrap(bootstrapName: string): Promise<void> {
    await this.tableFunctions.waitForTable();
    const url = await this.tableFunctions.getHrefURL({
      tableRowsSelector: this.listRows,
      rowTitle: bootstrapName,
      type: "Delete",
    });
    if (url === "") {
      throw "Couldn't find bootstrap href to click delete. Verify that item is present";
    }
    await this.executeDelete(url);
  }

  async executeDelete(url: string): Promise<void> {
    await Promise.all([
      this.waitForResponse("/DataImport/BootstrapData/Delete"),
      this.clickLinkByURL(url),
    ]);
  }
}
