// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Given, Then } from "@cucumber/cucumber";
import { strictEqual } from "assert";
import { models, context } from "../management/setup";
import { validatePath } from "../management/validators";

Given("there's an API connection added", async () => {
  await models.apiConnectionsPage.navigate();

  await models.apiConnectionsPage.addNewAPI();
  await models.apiConnectionsPage.addInformation("valid");
  await models.apiConnectionsPage.testConnection();
  await models.apiConnectionsPage.waitForSaveButton();
  await models.apiConnectionsPage.isSaveButtonEnabled();
  await models.apiConnectionsPage.saveConnection();
});

Given("user is logged in", async () => {
  await models.loginPage.fullLogin(process.env.email, process.env.password);
  await context.storageState({ path: "./state/login-state.json" });
});

Given("it's on the {string} page", async (pageName: string) => {
  let currentPage;

  switch (pageName) {
    case "Activity":
      break;
    case "Log in":
      currentPage = models.loginPage;
      break;
    case "API Connections":
      currentPage = models.apiConnectionsPage;
      break;
    case "Configuration":
      currentPage = models.configurationPage;
      break;
    case "Import / Export Templates":
      currentPage = models.importExportPage;
      break;
    case "Maps":
      currentPage = models.mapsPage;
      break;
    default:
      break;
  }

  if (currentPage) {
    await currentPage.navigate();
    strictEqual(
      await currentPage.pageTitle(),
      pageName,
      "Page Title does not match"
    );
    validatePath(currentPage.path());
  }
});

Then("it's redirected to the {string} page", async (pageName: string) => {
  let currentPage;

  switch (pageName) {
    case "Maps":
      currentPage = models.mapsPage;
      break;
    default:
      break;
  }

  if (currentPage) {
    validatePath(currentPage.path());
  }
});
