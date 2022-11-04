// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { page } from "./setup";
import { ok, strictEqual } from "assert";

export function validatePath(path: string, full = false): void {
  const url = page.url();
  if (full) {
    strictEqual(url, path, "Path does not match");
  } else {
    ok(url.includes(path), `Path ${url} does not include ${path}`);
  }
}
