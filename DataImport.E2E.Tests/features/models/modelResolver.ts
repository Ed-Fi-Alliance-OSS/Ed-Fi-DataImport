// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

import { Page } from "playwright";
import { ActivityPage } from "./activityPage";
import { ApiConnectionsPage } from "./apiConnectionsPage";
import { DataImportPage } from "./dataImportPage";
import { LoginPage } from "./loginPage";
import { TemplateSharingPage } from "./templateSharingPage";
import { ConfigurationPage } from "./configurationPage";
import { ImportExportPage } from "./importExportPage";
import { MapsPage } from "./mapsPage";
import { BootstrapPage } from "./bootstrapPage";
import { ImportTemplatePage } from "./importTemplatePage";
import { ExportTemplatePage } from "./exportTemplatePage";

export class ModelResolver {
  diPages: Array<DataImportPage> = [];

  public get apiConnectionsPage(): ApiConnectionsPage {
    let model = this.getModel<ApiConnectionsPage>(ApiConnectionsPage.name);
    if (!model) {
      model = new ApiConnectionsPage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get loginPage(): LoginPage {
    let model = this.getModel<LoginPage>(LoginPage.name);
    if (!model) {
      model = new LoginPage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get activityPage(): ActivityPage {
    let model = this.getModel<ActivityPage>(ActivityPage.name);
    if (!model) {
      model = new ActivityPage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get templateSharingPage(): TemplateSharingPage {
    let model = this.getModel<TemplateSharingPage>(TemplateSharingPage.name);
    if (!model) {
      model = new TemplateSharingPage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get configurationPage(): ConfigurationPage {
    let model = this.getModel<ConfigurationPage>(ConfigurationPage.name);
    if (!model) {
      model = new ConfigurationPage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get importExportPage(): ImportExportPage {
    let model = this.getModel<ImportExportPage>(ImportExportPage.name);
    if (!model) {
      model = new ImportExportPage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get importTemplatePage(): ImportTemplatePage {
    let model = this.getModel<ImportTemplatePage>(ImportTemplatePage.name);
    if (!model) {
      model = new ImportTemplatePage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get exportTemplatePage(): ExportTemplatePage {
    let model = this.getModel<ExportTemplatePage>(ExportTemplatePage.name);
    if (!model) {
      model = new ExportTemplatePage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get mapsPage(): MapsPage {
    let model = this.getModel<MapsPage>(MapsPage.name);
    if (!model) {
      model = new MapsPage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  public get bootstrapPage(): BootstrapPage {
    let model = this.getModel<BootstrapPage>(BootstrapPage.name);
    if (!model) {
      model = new BootstrapPage(this.page);
      this.diPages.push(model);
    }
    return model;
  }

  constructor(public page: Page) {}

  getModel<T extends DataImportPage>(name: string): T {
    const model = this.diPages.find((p) => p.constructor.name === name) as T;
    return model;
  }
}
