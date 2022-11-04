// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Page } from "playwright";
import { TableFunctions } from "../management/tableFunctions";

export abstract class DataImportPage {
  page: Page;
  tableFunctions: TableFunctions;

  title = ".container h2";
  toast = "#toast-container";
  loadingSelector = ".footable-loader";
  validationErrors = "div#validationSummary:not(.hidden)";

  get url(): string {
    if (!process.env.URL) {
      throw "URL not found. Verify that URL is set in .env file";
    }
    return process.env.URL;
  }

  get isOnPage(): boolean {
    const currentURL = this.page.url();
    const baseURL = currentURL.substring(0, currentURL.indexOf("?"));
    const URL = baseURL === "" ? currentURL : baseURL;
    return URL === this.path();
  }

  constructor(page: Page) {
    this.page = page;
    this.tableFunctions = new TableFunctions();
  }

  acceptModal(): void {
    this.page.on("dialog", async (dialog) => await dialog.accept());
  }

  abstract path(): string;

  async navigate(): Promise<void> {
    if (!this.isOnPage) {
      await this.page.goto(this.path(), { waitUntil: "networkidle" });
    }
  }

  async pageTitle(): Promise<string | null> {
    return await this.getText(this.title);
  }

  async getToastMessage(): Promise<string | null> {
    return await this.getText(this.toast);
  }

  async clickLinkByURL(url: string): Promise<void> {
    await this.page.locator(`a[href="${url}"]`).click();
  }

  async uploadFile(selector: string, fileName: string): Promise<void> {
    await this.page.locator(selector).setInputFiles(fileName);
  }

  async waitForLoader(): Promise<void> {
    const loading = this.page.locator(this.loadingSelector);
    await loading.waitFor({ state: "visible" }).then(
      async () =>
        await loading.waitFor({
          state: "hidden",
        })
    );
  }

  async getFormActionURL(selector: string): Promise<string | null> {
    return await this.page.locator(selector).getAttribute("action");
  }

  async getValidationErrors(): Promise<string | null> {
    return await this.getText(this.validationErrors);
  }

  async waitForResponse(url: string, status = 200): Promise<void> {
    await this.page.waitForResponse(
      (response) => response.url().includes(url) && response.status() === status
    );
  }

  protected async getText(text: string): Promise<string | null> {
    return this.page.textContent(text);
  }

  protected async hasText(text: string, selector = "div"): Promise<boolean> {
    return await this.elementExists(
      `div.container ${selector}:has-text("${text}")`
    );
  }

  protected async hasSubtitle(text: string): Promise<boolean> {
    const selector = `div.container h4:has-text("${text}")`;
    return (
      (await this.elementExists(selector)) &&
      (await this.elementIsVisible(selector))
    );
  }

  protected async elementExists(selector: string): Promise<boolean> {
    return (await this.page.locator(selector).count()) > 0;
  }

  protected async elementIsVisible(selector: string): Promise<boolean> {
    return await this.page.locator(selector).isVisible();
  }

  protected async waitForButtonEnabled(btn: string): Promise<void> {
    this.page.$(btn).then(async (e) => {
      await e?.waitForElementState("enabled");
    });
  }
}
