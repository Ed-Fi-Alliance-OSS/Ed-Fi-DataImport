// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Given, Then, When } from "@cucumber/cucumber";
import { notStrictEqual, ok } from "assert";
import { models } from "../management/setup";
import { validatePath } from "../management/validators";

const productionTemplateSharingURL = "https://template-sharing.ed-fi.org/";
const templateFilter = {
  field: "",
  data: "",
};

Given("it has the correct Template Sharing URL", async () => {
  await models.configurationPage.navigate();
  notStrictEqual(
    await models.configurationPage.getTemplateSharingURL(),
    productionTemplateSharingURL,
    "Tests must not be pointing to production template sharing"
  );
});

When(
  /^entering template sharing scenario: ([0-9]*[A-z .-]*) in ([A-z ]+)/,
  async (data: string, filter: string) => {
    await models.templateSharingPage.tableFunctions.waitForTable();

    templateFilter.data = data;
    templateFilter.field = filter;

    switch (filter) {
      case "Template Name":
        await models.templateSharingPage.filterByTemplate(data);
        break;
      case "Organization":
        await models.templateSharingPage.filterByOrg(data);
        break;
      case "ODS Version":
        templateFilter.field = "Ed-Fi ODS / API Version";
        await models.templateSharingPage.selectByVersion(data);
        break;
      default:
        break;
    }

    await models.templateSharingPage.filterList();
  }
);

When("clicking view detail", async () => {
  await models.templateSharingPage.tableFunctions.waitForTable();
  await models.templateSharingPage.setData();
  await models.templateSharingPage.clickView();
  await models.templateSharingPage.setFormURL();
});

When("selecting an ODS version", async () => {
  await models.templateSharingPage.selectODSVersion();
});

When("clicking Import", async () => {
  await models.templateSharingPage.clickImport();
});

Then("TSS list is filtered", async () => {
  await models.templateSharingPage.waitForLoader();
  await models.templateSharingPage.tableFunctions.waitForTable();

  ok(
    await models.templateSharingPage.isListFiltered(templateFilter),
    "List is not filtered"
  );
});

Then("template details load", async () => {
  validatePath(models.templateSharingPage.detailsPath());
  ok(
    await models.templateSharingPage.hasTemplateTitle(),
    "Template title not found"
  );
  ok(
    await models.templateSharingPage.hasSubmitterOrganization(),
    "Submitter organization not found"
  );
});

Then("template details version warning appears", async () => {
  ok(
    (await models.templateSharingPage.getVersionWarning())?.includes(
      models.templateSharingPage.templateInformation.odsVersion
    ),
    "Version warning not found"
  );
});

Then("template preview appears", async () => {
  const preview = await models.templateSharingPage.getPreview();
  if (preview) {
    ok(JSON.parse(preview), "Unable to parse preview into a valid JSON");
  }
});

Then("template is imported", async () => {
  await models.templateSharingPage.verifyTemplateImported();
});
