// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Then, When } from "@cucumber/cucumber";
import { strictEqual, ok } from "assert";
import { page, models } from "../management/setup";
import { validatePath } from "../management/validators";

When("adding API connection", async () => {
  await models.apiConnectionsPage.addNewAPI();
  validatePath(models.apiConnectionsPage.pathWhenAdding(), true);

  strictEqual(
    await models.apiConnectionsPage.pageTitle(),
    "Add API Connection",
    "Page Titles do not match"
  );
});

When("editing API connection", async () => {
  const id = await models.apiConnectionsPage.editAdded();
  validatePath(models.apiConnectionsPage.pathWhenEditing(id), true);
  strictEqual(
    await models.apiConnectionsPage.pageTitle(),
    "Edit API Connection",
    "Page Titles do not match"
  );
});

When("deleting API connection", async () => {
  await models.apiConnectionsPage.clickDelete();
});

When(/^inputs (valid|invalid) API URL/, async (type: string) => {
  const api = type === "valid" ? `${process.env.API_URL}` : "/fail";
  await models.apiConnectionsPage.enterURL(api);
});

When(/^adding (valid|invalid) information/, async (type: string) => {
  await models.apiConnectionsPage.addInformation(type);
});

When("testing connection", async () => {
  await models.apiConnectionsPage.testConnection();
  await models.apiConnectionsPage.waitForToast();
});

When("clicking on save connection", async () => {
  await models.apiConnectionsPage.waitForSaveButton();
  ok(models.apiConnectionsPage.isSaveButtonEnabled(), "Save button disabled");
  await models.apiConnectionsPage.saveConnection();
});

Then("API version appears", async () => {
  const apiVersion = process.env.API_Version;
  if (!apiVersion) {
    throw "API Version not found. Verify it is set in the .env file (API_Version)";
  }

  await models.apiConnectionsPage.waitForVersion();
  ok(
    await models.apiConnectionsPage.isApiSectionVisible(),
    "API section not visible"
  );
  strictEqual(
    await models.apiConnectionsPage.getAPIVersion(),
    apiVersion,
    "API Versions do not match"
  );
});

Then("API validation message appears", async () => {
  strictEqual(
    await models.apiConnectionsPage.getErrorMessages(),
    models.apiConnectionsPage.messages.invalidURL,
    "Error message is not the expected"
  );
});

Then(/^connection (success|error) message appears/, async (type: string) => {
  strictEqual(
    await models.apiConnectionsPage.getToastMessage(),
    type === "success"
      ? models.apiConnectionsPage.messages.connectionSuccess
      : models.apiConnectionsPage.messages.connectionError,
    "Message does not match"
  );
});

Then(/^connection can be (saved|updated)/, async (type: string) => {
  await page.waitForNavigation();
  strictEqual(
    await models.apiConnectionsPage.getToastMessage(),
    type === "saved"
      ? models.apiConnectionsPage.messages.connectionSaved
      : models.apiConnectionsPage.messages.connectionUpdated,
    "Message does not match"
  );
});

Then(/^(added|updated) connection appears on list/, async (type: string) => {
  validatePath(models.apiConnectionsPage.path(), true);
  ok(
    await models.apiConnectionsPage.tableFunctions.existsInTable(
      type === "added"
        ? models.apiConnectionsPage.apiConnectionName
        : models.apiConnectionsPage.updatedConnectionName
    ),
    `${type} connection not found`
  );
});

Then("connection is deleted", async () => {
  await page.waitForNavigation();
  strictEqual(
    await models.apiConnectionsPage.getToastMessage(),
    models.apiConnectionsPage.messages.connectionDeleted,
    "Message does not match"
  );
  ok(
    (await models.apiConnectionsPage.tableFunctions.existsInTable(
      models.apiConnectionsPage.apiConnectionName
    )) === false,
    "Connection not deleted"
  );
});

Then("updating information", async () => {
  await models.apiConnectionsPage.updateForm();
});
