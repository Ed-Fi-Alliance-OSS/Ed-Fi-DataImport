// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { DataImportPage } from "./dataImportPage";
import { API_Versions } from "../enums";

export class MapsPage extends DataImportPage {
  private _addedMaps: Array<string> = [];

  get mapFor340(): string {
    return this.getMap(API_Versions.Version34);
  }

  listRows = `${this.tableFunctions.tableBody} tr`;

  path(): string {
    return `${this.url}/DataMaps`;
  }

  getMap(name: string): string {
    const map = this._addedMaps.find((m) => m === name);
    if (map) {
      return map;
    }
    throw `Map ${name} not set`;
  }

  addMap(newMap: string): void {
    if (!this._addedMaps.find((m) => m === newMap)) {
      this._addedMaps.push(newMap);
    }
  }

  async deleteMap(mapName: string): Promise<void> {
    await this.tableFunctions.waitForTable();
    const url = await this.tableFunctions.getHrefURL({
      tableRowsSelector: this.listRows,
      rowTitle: mapName,
      type: "Delete",
    });
    if (url === "") {
      throw "Couldn't find map href to click delete. Verify that item is present";
    }
    await this.executeDelete(url);
  }

  async executeDelete(url: string): Promise<void> {
    await Promise.all([
      this.waitForResponse("/DataImport/DataMaps/Delete"),
      this.clickLinkByURL(url),
    ]);
  }
}
