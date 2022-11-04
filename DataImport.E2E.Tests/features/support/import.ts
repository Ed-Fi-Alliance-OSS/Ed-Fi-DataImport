// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Then, When } from "@cucumber/cucumber";
import { ok, deepStrictEqual, strictEqual } from "assert";
import { models } from "../management/setup";
import { validatePath } from "../management/validators";

let currentScenario: string;

When("clicking import", async () => {
  await models.importExportPage.getTemplatePreview();
});

When("clicking on Import Template", async () => {
  await models.importExportPage.importTemplate();
});

When(/^selecting ([A-z ]*) to import/, async (scenario: string) => {
  currentScenario = scenario;

  switch (currentScenario) {
    case "a valid file":
      await models.importExportPage.selectValidFile();
      break;
    case "no json":
      await models.importExportPage.selectXMLFile();
      break;
    case "invalid json":
      await models.importExportPage.selectInvalidJSONFile();
      break;
    case "no file":
    default:
      break;
  }
});

Then("error message for import scenario appears", async () => {
  switch (currentScenario) {
    case "no file":
      strictEqual(
        await models.importTemplatePage.getErrorMessages(),
        models.importExportPage.messages.noFileSelected,
        "Message does not match"
      );
      break;
    default:
      break;
  }
});

Then("selected template is imported", async () => {
  strictEqual(
    await models.importTemplatePage.getToastMessage(),
    models.importTemplatePage.messages.templateImported,
    "Message does not match"
  );
});

Then("import details load", async () => {
  validatePath(models.importTemplatePage.path());
  ok(
    await models.importTemplatePage.hasTemplateTitle(),
    "Template title not found"
  );
  ok(await models.importTemplatePage.hasDescription(), "Description not found");
});

Then("imported file version warning appears", async () => {
  ok(
    models.importTemplatePage.hasVersionWarning(),
    "Version warning not found"
  );
});

Then("imported file preview appears", async () => {
  deepStrictEqual(
    await models.importTemplatePage.getPreview(),
    models.importTemplatePage.template,
    "Generated JSON does not match template data"
  );
});

Then("Map appears on list", async () => {
  await models.mapsPage.navigate();
  ok(
    await models.mapsPage.tableFunctions.existsInTable(
      models.importTemplatePage.map.name
    ),
    `Map not found`
  );
});

Then("Bootstrap appears on list", async () => {
  await models.bootstrapPage.navigate();
  ok(
    await models.bootstrapPage.tableFunctions.existsInTable(
      models.importTemplatePage.bootstrap.name
    ),
    `Bootstrap not found`
  );
});
