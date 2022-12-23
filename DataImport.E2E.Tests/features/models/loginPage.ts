// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { DataImportPage } from "./dataImportPage";

export class LoginPage extends DataImportPage {
  emailInput = "input#Input_Email";
  passwordInput = "input#Input_Password";
  submitBtn = 'button[type="submit"]';
  emailValidation = ".field-validation-error #Input_Email-error";
  passwordValidation = ".field-validation-error #Input_Password-error";
  generalValidation = ".validation-summary-errors li";
  userDropdown = "ul.navbar-right a.dropdown-toggle";
  logoutBtn = "ul.dropdown-menu a:text('Log off')";

  messages = {
    register: "Don't have an account? Register.",
    emailRequired: "The Email field is required.",
    passwordRequired: "The Password field is required.",
    invalidAttempt: "Invalid login attempt.",
    invalidEmail: "The Email field is not a valid e-mail address.",
  };

  path(): string {
    return `${this.url}/Account/Login`;
  }

  pathLogout(): string {
    return `${this.url}/Account/Logout`
  }

  async fillForm(username?: string, password?: string): Promise<void> {
    if (username && password) {
      await this.page.locator(this.emailInput).fill(username);
      await this.page.locator(this.passwordInput).fill(password);
    } else {
      throw "Could not find email or password. Verify that variables are set in the .env file";
    }
  }

  async login(): Promise<void> {
    await this.page.locator(this.submitBtn).click();
  }

  async emailValidationMessage(): Promise<string | null> {
    return this.getText(this.emailValidation);
  }

  async passwordValidationMessage(): Promise<string | null> {
    return this.getText(this.passwordValidation);
  }

  async generalValidationMessage(): Promise<string | null> {
    return this.getText(this.generalValidation);
  }

  async fullLogin(username?: string, password?: string): Promise<void> {
    await this.navigate();
    await this.fillForm(username, password);
    await this.login();
  }

  async isRegisterOptionVisible(): Promise<boolean> {
    return await this.hasText(this.messages.register);
  }

  async logout(): Promise<void> {
    await this.page.locator(this.userDropdown).click();
    await Promise.all([
      this.waitForResponse("/Account/Logout", 302),
      this.page.locator(this.logoutBtn).click(),
      this.page.waitForNavigation(),
    ]);
  }
}
