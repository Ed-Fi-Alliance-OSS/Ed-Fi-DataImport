// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Given, Then, When } from "@cucumber/cucumber";
import { ok, deepStrictEqual } from "assert";
import { models } from "../management/setup";
import { getJSONFileContent } from "../management/functions";
import { DataImportTemplateSummary } from "../interfaces";
import { API_Versions } from "../enums";

let currentScenario: string;
const currentVersion = {
  map: "",
  bootstrap: "",
};

Given("there's a template imported", async () => {
  try {
    await models.importExportPage.navigate();
    await models.importExportPage.selectValidFile();
    await models.importExportPage.getTemplatePreview();
    await models.importExportPage.selectODSVersionByIndex();
    await models.importExportPage.importTemplate();
  } catch (e) {
    const errors = await models.exportTemplatePage.getValidationErrors();
    throw errors ? errors : e;
  }
  const apiVersion = process.env.API_Version;
  if (!apiVersion) {
    throw "API Version not found. Verify it is set in the .env file (API_Version)";
  }
  currentVersion.map = apiVersion;
  currentVersion.bootstrap = apiVersion;
});

When("clicking export", async () => {
  await models.importExportPage.navigate();
  await models.importExportPage.generateTemplateExport();
});

When("selecting Map", async () => {
  ok(
    await models.exportTemplatePage.hasMapsSubtitle(),
    "Maps subtitle not found"
  );

  await models.exportTemplatePage.selectMap(currentVersion.map);
  ok(await models.exportTemplatePage.isMapChecked(), "Map not checked");
  await models.exportTemplatePage.clickContinue();
});

When("selecting Bootstrap", async () => {
  ok(
    await models.exportTemplatePage.hasBootstrapSubtitle(),
    "Bootstrap subtitle not found"
  );
  await models.exportTemplatePage.selectBootstrap(currentVersion.bootstrap);
  ok(
    await models.exportTemplatePage.isBootstrapChecked(),
    "Bootstrap not checked"
  );
  await models.exportTemplatePage.clickContinue();
});

When(/^entering ([A-z ]*) in export form/, async (scenario: string) => {
  currentScenario = scenario;

  switch (currentScenario) {
    case "title and description":
      await models.exportTemplatePage.setTitle();
      await models.exportTemplatePage.setDescription();
      break;
    case "title":
      await models.exportTemplatePage.setTitle();
      break;
    case "description":
      await models.exportTemplatePage.setDescription();
      break;
    case "no data":
    default:
      break;
  }
});

When(/^clicking preview( for invalid request)?/, async (data) => {
  await models.exportTemplatePage.generatePreview(data ? 400 : 200);
});

When(
  /^adding (Map|Bootstrap) for previous version/,
  async (content: string) => {
    await models.importExportPage.navigate();

    if (content === "Map") {
      currentVersion.map = API_Versions.Version34;
      models.mapsPage.addMap(currentVersion.map);
      await models.importExportPage.selectMapFor34File();
    } else {
      currentVersion.bootstrap = API_Versions.Version34;
      models.bootstrapPage.addBootstrap(currentVersion.bootstrap);
      await models.importExportPage.selectBootstrapFor34File();
    }
    await models.importExportPage.getTemplatePreview();

    await models.importExportPage.selectODSVersion(
      content === "Map" ? currentVersion.map : currentVersion.bootstrap
    );
    await models.importExportPage.importTemplate();
  }
);

Then("export preview loads", async () => {
  ok(await models.exportTemplatePage.hasPreviewSubtitle());
});

Then("file can be downloaded", async () => {
  await models.exportTemplatePage.downloadFile();
});

Then("downloaded file is valid", async () => {
  const templateFromFile = await getJSONFileContent<DataImportTemplateSummary>(
    models.exportTemplatePage.downloadedPath
  );

  deepStrictEqual(
    templateFromFile.template,
    models.importTemplatePage.template,
    "Downloaded data does not match template data"
  );
});

Then("error message for export scenario appears", async () => {
  const errors = await models.exportTemplatePage.getErrorMessages();
  if (!errors) {
    throw "errors not found";
  }
  const shouldHaveTitleMsg =
    currentScenario === "description" || currentScenario === "no data";
  const shouldHaveDescMsg =
    currentScenario === "title" || currentScenario === "no data";

  ok(
    errors.includes(models.exportTemplatePage.messages.noTitle) ===
      shouldHaveTitleMsg,
    `Title should ${shouldHaveTitleMsg ? "" : "not"} be present`
  );
  ok(
    errors.includes(models.exportTemplatePage.messages.noDescription) ===
      shouldHaveDescMsg,
    `Description should ${shouldHaveDescMsg ? "" : "not"} be present`
  );
});

Then("Bootstrap step will not appear", async () => {
  ok(!(await models.exportTemplatePage.hasBootstrapSubtitle()));
  ok(
    await models.exportTemplatePage.hasSummarySubtitle(),
    "Summary subtitle not found"
  );
});

Then("added Bootstrap will not have checkbox", async () => {
  ok(await models.exportTemplatePage.hasBootstrapSubtitle());
  ok(await models.exportTemplatePage.isCheckDisabled(currentVersion.bootstrap));
});
