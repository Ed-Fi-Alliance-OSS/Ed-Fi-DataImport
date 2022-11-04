// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import {
  Before,
  BeforeAll,
  ITestCaseHookParameter,
  setDefaultTimeout,
} from "@cucumber/cucumber";
import { chromium, Browser, BrowserContext, Page } from "playwright";
import { ModelResolver } from "../models/modelResolver";
import { Test } from "../interfaces";

import dotenv = require("dotenv");

dotenv.config();
setDefaultTimeout(60 * 1000);

export let browser: Browser;
export let page: Page;
export let context: BrowserContext;
export let models: ModelResolver;

export const currentTest: Test = {
  Feature: "",
  Scenario: "",
};

Before(async (scenario) => {
  context = process.env.RECORD
    ? await browser.newContext({
        recordVideo: { dir: "./videos" },
        acceptDownloads: true,
        ignoreHTTPSErrors: true,
      })
    : await browser.newContext({
        acceptDownloads: true,
        ignoreHTTPSErrors: true,
      });

  if (process.env.TRACE) {
    await context.tracing.start({ screenshots: true, snapshots: true });
  }

  page = await context.newPage();

  models = new ModelResolver(page);

  setScenarioName(scenario);
});

BeforeAll(async () => {
  browser =
    process.env.GITHUB_ACTIONS || process.env.HEADLESS
      ? await chromium.launch()
      : await chromium.launch({ headless: false });
});

function setScenarioName(scenario: ITestCaseHookParameter) {
  const featureName = scenario.gherkinDocument.feature?.name;
  if (featureName) {
    currentTest.Feature = featureName;
  }

  currentTest.Scenario = scenario.pickle.name;

  const example = getScenarioExample(scenario);
  if (example) {
    currentTest.Scenario += `- ${example}`;
  }
}

function getScenarioExample(
  scenario: ITestCaseHookParameter
): string | undefined {
  try {
    const allExamples = scenario.gherkinDocument.feature?.children.find(
      (s) => s.scenario?.name === scenario.pickle.name
    )?.scenario?.examples[0].tableBody;

    const currentExample = allExamples?.filter((example) =>
      scenario.pickle.astNodeIds.includes(example.id)
    );
    if (currentExample) {
      //Get value of example
      return currentExample[0].cells[0].value;
    }
  } catch {
    return;
  }
  return;
}
