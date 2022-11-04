// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Locator } from "playwright";
import { page } from "./setup";

export class TableFunctions {
  private _table = "table";

  get table(): string {
    return this._table;
  }

  set table(newTable: string) {
    this._table = newTable;
  }

  get tableBody(): string {
    return `${this.table} tbody`;
  }

  async getCellContent(column: string, row = 1): Promise<string | undefined> {
    const columnSelector = `${
      this.tableBody
    } tr:nth-child(${row}) td:nth-child(${await this.getIndexByColumnName(
      column
    )})`;
    return await page.locator(columnSelector).innerText();
  }

  async existsInTable(element: string): Promise<boolean> {
    await this.waitForTable();
    const result = this.getFromTable(this.tableBody, element);
    return (await result.count()) > 0;
  }

  async waitForTable(): Promise<void> {
    await page.waitForSelector(this.tableBody, {
      state: "visible",
    });
  }

  async markCheckboxByValueInRow({
    columnForValue,
    searchedValue,
    checkboxColumn,
  }: {
    columnForValue: string;
    searchedValue: string;
    checkboxColumn: string;
  }): Promise<{ row: number; column: number }> {
    const position = await this.getXYPosition({
      columnForValue,
      searchedValue,
      checkboxColumn,
    });

    await page.check(
      `${this.tableBody} tr:nth-child(${position.row}) > td:nth-child(${position.column}) input[type="checkbox"]`
    );
    return { row: position.row, column: position.column };
  }

  async getXYPosition({
    columnForValue,
    searchedValue,
    checkboxColumn,
  }: {
    columnForValue: string;
    searchedValue: string;
    checkboxColumn: string;
  }): Promise<{ row: number; column: number }> {
    const indexForValue = await this.getIndexByColumnName(columnForValue);

    const rowIndex = await this.getRowByValueInTable(
      indexForValue,
      searchedValue
    );

    const checkboxColumnIndex = await this.getIndexByColumnName(checkboxColumn);

    return { row: rowIndex, column: checkboxColumnIndex };
  }

  async getHrefURL({
    tableRowsSelector,
    rowTitle,
    type,
  }: {
    tableRowsSelector: string;
    rowTitle: string;
    type: string;
  }): Promise<string> {
    let hrefURL = "";
    await page.$$(tableRowsSelector).then(async (elements) => {
      for (const element of elements) {
        if (await element.$(`text=${rowTitle}`)) {
          const hrefAttribute = await (
            await element.$(`a[title="${type}"]`)
          )?.getAttribute("href");
          if (hrefAttribute) {
            hrefURL = hrefAttribute;
          }
        }
      }
    });
    return hrefURL;
  }

  async getAllRowsFromColumn(column: string): Promise<string[]> {
    const rows: Array<string> = [];

    const index = await this.getIndexByColumnName(column);
    const columnContent = `tr > td:nth-child(${index})`;

    await page.$$(columnContent).then(async (elements) => {
      for (const element of elements) {
        rows.push(await element.innerText());
      }
    });

    return rows;
  }

  protected getFromTable(tableBodySelector: string, element: string): Locator {
    return page.locator(`${tableBodySelector} :text("${element}")`);
  }

  private async getRowByValueInTable(column: number, value: string) {
    let rowNumber = -1;
    const headers = `${this.tableBody} tr td:nth-child(${column})`;
    await page.$$(headers).then(async (elements) => {
      for (
        let index = 0;
        index < elements.length && rowNumber === -1;
        index++
      ) {
        if ((await elements[index].innerText()) === value) {
          rowNumber = index + 1;
        }
      }
    });
    if (rowNumber === -1) {
      throw `Unable to get row number for ${value} in column ${column}`;
    }
    return rowNumber;
  }

  private async getIndexByColumnName(column: string): Promise<number> {
    let columnNumber = -1;
    const headers = `${this.table} tr th`;
    await page.$$(headers).then(async (elements) => {
      for (
        let index = 0;
        index < elements.length && columnNumber === -1;
        index++
      ) {
        if ((await elements[index].innerText()) === column) {
          columnNumber = index + 1;
        }
      }
    });
    if (columnNumber === -1) {
      throw `Unable to get index by column name ${column}`;
    }
    return columnNumber;
  }
}
