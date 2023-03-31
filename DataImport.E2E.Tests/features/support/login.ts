// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Then, When } from "@cucumber/cucumber";
import { strictEqual } from "assert";
import { models } from "../management/setup";
import { ok } from "assert";

let currentScenario: string;

When("clicks Log in", async () => {
  await models.loginPage.login();
});

When(/^user enters login scenario: ([A-z ]*)/, async (scenario: string) => {
  currentScenario = scenario;
  let email = process.env.email;
  let password = process.env.password;

  switch (currentScenario) {
    case "invalid email":
      email = "Not an email";
      break;
    case "wrong email":
      email = "wrongemail@edfi.org";
      break;
    case "wrong password":
      password += "2";
      break;
    default:
      break;
  }

  if (!currentScenario.includes("no data")) {
    await models.loginPage.fillForm(email, password);
  }
});

When("user enters valid username and password", async () => {
  await models.loginPage.fillForm(process.env.email, process.env.password);
});

Then("login is successful", async () => {
  strictEqual(
    await models.activityPage.pageTitle(),
    "Activity",
    "Page Title does not match"
  );
});

Then("login validation message appears", async () => {
  switch (currentScenario) {
    case "invalid email":
      strictEqual(
        await models.loginPage.emailValidationMessage(),
        models.loginPage.messages.invalidEmail,
        "Email validation message does not match"
      );
      break;
    case "wrong email":
    case "wrong password":
      strictEqual(
        await models.loginPage.generalValidationMessage(),
        models.loginPage.messages.invalidAttempt,
        "Validation message does not match"
      );
      break;
    case "no data":
      strictEqual(
        await models.loginPage.emailValidationMessage(),
        models.loginPage.messages.emailRequired,
        "Email validation message does not match"
      );
      strictEqual(
        await models.loginPage.passwordValidationMessage(),
        models.loginPage.messages.passwordRequired,
        "Password validation message does not match"
      );
      break;
    default:
      break;
  }
});
