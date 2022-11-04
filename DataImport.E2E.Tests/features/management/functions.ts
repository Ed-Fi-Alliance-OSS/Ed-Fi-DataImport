// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { mkdir, readFile } from "fs/promises";
import { page, currentTest, context } from "./setup";
import { StringDecoder } from "string_decoder";

export async function saveTrace(): Promise<void> {
  if (process.env.TRACE) {
    const traceFolder = "./traces";
    mkdir(traceFolder).catch(() => {});
    const path = `${traceFolder}/${currentTest.Feature}/${currentTest.Scenario}/trace.zip`;

    await context.tracing.stop({ path });
  }
}

export async function takeScreenshot(name: string): Promise<void> {
  await page.screenshot({
    path: `./screenshots/${currentTest.Feature}/${currentTest.Scenario}/${name}.png`,
  });
}

export async function getJSONFileContent<T>(path: string): Promise<T> {
  const bufferResult = await readFile(path);
  const decoder = new StringDecoder();

  try {
    return JSON.parse(decoder.write(bufferResult));
  } catch (e) {
    throw `Unable to get JSON File content: ${e}`;
  }
}
