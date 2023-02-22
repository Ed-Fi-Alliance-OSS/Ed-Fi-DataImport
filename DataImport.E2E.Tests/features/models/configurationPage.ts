// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { DataImportPage } from "./dataImportPage";

export class ConfigurationPage extends DataImportPage {
  templateSharingURLInput = "input#TemplateSharingApiUrl";
  allowUserRegistrationInput = "input#InstanceAllowUserRegistration";
  productImprovementInput = "input#EnableProductImprovement";
  updateConfigurationBtn = "button:text('Update Configuration')";
  analyticsTag = "script[src='https://www.google-analytics.com/analytics.js']";

  messages = {
    configurationUpdated: "Configuration was modified.",
  };

  path(): string {
    return `${this.url}/Configuration`;
  }

  //Override
  get isOnPage(): boolean {
    const currentURL = this.page.url();
    const baseURL = currentURL.substring(0, currentURL.indexOf("?"));
    const URL = baseURL === "" ? currentURL : baseURL;
    return URL === this.path();
  }

  async isUserRegistrationEnabled(): Promise<boolean> {
    return await this.page.locator(this.allowUserRegistrationInput).isChecked();
  }

  async isProductImprovementEnabled(): Promise<boolean> {
    return await this.page.locator(this.productImprovementInput).isChecked();
  }

  async changeUserRegistrationValue(): Promise<void> {
    await this.page.locator(this.allowUserRegistrationInput).click();
  }

  async changeProductImprovementValue(): Promise<void> {
    await this.page.locator(this.productImprovementInput).click();
  }

  async clickUpdateConfiguration(): Promise<void> {
    await this.page.locator(this.updateConfigurationBtn).click();
  }

  async updateConfiguration(): Promise<void> {
    await Promise.all([
      this.clickUpdateConfiguration(),
    ]);
  }

  async hasAnalyticsTag(): Promise<boolean> {
    return await this.elementExists(this.analyticsTag);
  }
}
