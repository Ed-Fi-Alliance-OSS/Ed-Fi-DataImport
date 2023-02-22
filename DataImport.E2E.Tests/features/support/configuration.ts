// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Given, Then, When } from "@cucumber/cucumber";
import { ok, strictEqual } from "assert";
import { models, currentTest } from "../management/setup";
import { validatePath } from "../management/validators";

Given(/^user registration is (enabled|disabled)/, async (type: string) => {
  if (
    (await models.configurationPage.isUserRegistrationEnabled()) !==
    (type === "enabled")
  ) {
    await models.configurationPage.changeUserRegistrationValue();
    await models.configurationPage.updateConfiguration();
  }
});

Given(/^product improvement is (enabled|disabled)/, async (type: string) => {
  if (
    (await models.configurationPage.isProductImprovementEnabled()) !==
    (type === "enabled")
  ) {
    await models.configurationPage.changeProductImprovementValue();
    await models.configurationPage.updateConfiguration();
  }
});

When(
  /^clicking on (enable|disable) user registration/,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  async (_type: string) => {
    await models.configurationPage.changeUserRegistrationValue();
  }
);

When(
  /^clicking on (enable|disable) product improvement/,
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  async (_type: string) => {
    await models.configurationPage.changeProductImprovementValue();
  }
);

When("clicking on update configuration", async () => {
  await models.configurationPage.updateConfiguration();
  strictEqual(
    await models.configurationPage.getToastMessage(),
    models.configurationPage.messages.configurationUpdated,
    "Message does not match"
  );
});

Then(/^configuration option is (enabled|disabled)/, async (type: string) => {
  if (currentTest.Scenario.includes("User Registration")) {
    ok(
      (await models.configurationPage.isUserRegistrationEnabled()) ===
        (type === "enabled"),
      `Allow user registration check is not ${type}`
    );
  } else if (currentTest.Scenario.includes("Product Improvement")) {
    ok(
      (await models.configurationPage.isProductImprovementEnabled()) ===
        (type === "enabled"),
      `Product improvement check is not ${type}`
    );
  }
});

Then(
  /^register option is (not )?present on login page/,
  async (content: string) => {
    await models.loginPage.logout();
    validatePath(models.loginPage.pathLogout(), true);
    await models.loginPage.clickOnLoginLink();
    strictEqual(
      await models.loginPage.isRegisterOptionVisible(),
      !content,
      content ? "Register option is visible" : "Register option is not visible"
    );
  }
);

Then(/^analytics tag is (not )?present/, async (content: string) => {
  strictEqual(
    await models.configurationPage.hasAnalyticsTag(),
    !content,
    content ? "Analytics tag is not present" : "Analytics tag is not present"
  );
});
