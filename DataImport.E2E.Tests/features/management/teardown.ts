// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { After, AfterAll } from "@cucumber/cucumber";
import { API_Versions } from "../enums";
import { TestStepResultStatus } from "@cucumber/messages";
import { saveTrace, takeScreenshot } from "./functions";
import { page, browser, models, currentTest } from "./setup";

After(async (scenario) => {
  scenario.result?.status === TestStepResultStatus.PASSED
    ? await takeScreenshot("SUCCESS")
    : await takeScreenshot("FAIL");

  const steps = scenario.pickle.steps.map((step) => step.text);
  await cleanup(steps);

  await saveTrace();
});

AfterAll(() => {
  if (!page?.isClosed()) {
    browser.close();
  }
});

async function cleanup(steps: string[]) {
  try {
    if (page.url() === models.loginPage.path()) {
      await models.loginPage.fullLogin(process.env.email, process.env.password);
    }

    if (
      currentTest.Scenario.includes("Add API connection") ||
      (!currentTest.Scenario.includes("Delete API connection") &&
        steps.includes("there's an API connection added"))
    ) {
      await cleanAPIConnection();
    }

    if (steps.includes("it's on the 'Configuration' page")) {
      await rollbackConfiguration(steps);
    }

    if (
      steps.includes("clicking on Import Template") ||
      steps.includes("there's a template imported")
    ) {
      await cleanBootstrap();
      await cleanMap();
    }

    if (steps.includes("adding Map for previous version")) {
      await cleanMap(API_Versions.Version34);
    }

    if (steps.includes("adding Bootstrap for previous version")) {
      await cleanBootstrap(API_Versions.Version34);
    }
  } catch (error) {
    console.info(
      `Item to delete for scenario ${currentTest.Scenario} not found\n${error}`
    );
  }
}

async function cleanAPIConnection() {
  await models.apiConnectionsPage.navigate();
  await models.apiConnectionsPage.clickDelete();
}

async function rollbackConfiguration(steps: string[]) {
  await models.configurationPage.navigate();

  if (steps.includes("user registration")) {
    await models.configurationPage.changeUserRegistrationValue();
  } else if (steps.includes("clicking on enable product improvement")) {
    await models.configurationPage.changeProductImprovementValue();
  }

  await models.configurationPage.clickUpdateConfiguration();
}

async function cleanMap(version = API_Versions.Version53) {
  await models.mapsPage
    .navigate()
    .then(async () => await models.mapsPage.deleteMap(version));
}

async function cleanBootstrap(version = API_Versions.Version53) {
  await models.bootstrapPage
    .navigate()
    .then(async () => await models.bootstrapPage.deleteBootstrap(version));
}
