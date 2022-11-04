// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common;
using DataImport.Web.Helpers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DataImport.Web.Features.DataMaps
{
    public class DataMapsController : BaseController
    {
        private readonly IMediator _mediator;

        private string[] ColumnHeaders
        {
            get
            {
                if (TempData["ColumnHeaders"] is string[] s)
                {
                    return s;
                }

                return new string[] { };
            }
            set => TempData["ColumnHeaders"] = value ?? new string[] { };
        }

        private DataTable CsvTablePreview
        {
            get
            {
                if (TempData["CsvDataPreview"] is string table)
                {
                    return JsonConvert.DeserializeObject<DataTable>(table);
                }

                return null;
            }
            set => TempData["CsvDataPreview"] = JsonConvert.SerializeObject(value);
        }

        private AddEditDataMapViewModel ViewModel
        {
            get
            {
                if (TempData["ViewModel"] is string addEditDataMapViewModel)
                {
                    return JsonConvert.DeserializeObject<AddEditDataMapViewModel>(addEditDataMapViewModel);
                }
                return null;
            }
            set => TempData["ViewModel"] = JsonConvert.SerializeObject(value);
        }

        public DataMapsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<ActionResult> Index()
        {
            return View(await _mediator.Send(new DataMapIndex.Query()));
        }

        public async Task<ActionResult> Add()
        {
            var sourceCsvHeaders = ColumnHeaders;

            var viewModel = await _mediator.Send(new AddDataMap.Query
            {
                SourceCsvHeaders = sourceCsvHeaders
            });

            viewModel.CsvPreviewDataTable = CsvTablePreview;

            MapTempViewModel(viewModel);

            return View("AddEdit", viewModel);
        }

        [HttpPost]
        public async Task<ActionResult> Add(AddDataMap.Command vm)
        {

            var response = await _mediator.Send(vm);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> AddModelFields(DataMapperFields.Query query)
        {
            return PartialView("_PartialDataMapperFields", await _mediator.Send(query));
        }

        public async Task<ActionResult> RetrieveDescriptors(RetrieveDescriptors.Query query)
        {
            // Clear ModelState so Html.DropDownListFor takes selected value from the Model rather than ModelState.
            // See this for more details https://stackoverflow.com/questions/24387263/html-dropdownlistfor-selecting-item-based-on-modelstate-rather-than-model    
            ViewData.ModelState.Clear();

            return PartialView("_DescriptorIndex", await _mediator.Send(query));
        }

        public async Task<ActionResult> Edit(EditDataMap.Query query)
        {
            query.SourceCsvHeaders = ColumnHeaders;

            var vm = await _mediator.Send(query);

            MapTempViewModel(vm);

            vm.CsvPreviewDataTable = CsvTablePreview;

            return View("AddEdit", vm);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(EditDataMap.Command vm)
        {
            var response = await _mediator.Send(vm);
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> Delete(int id)
        {
            var response = await _mediator.Send(new DeleteDataMap.Command { Id = id });
            ToastMessage(response);

            return this.RedirectToActionJson("Index");
        }

        [HttpPost]
        public async Task<ActionResult> UploadFile(IFormFile uploadSampleFile, int dataMapId, int? preprocessorId, string mapName, int? apiVersionId, string resourcePath, int? apiServerId, string attribute)
        {
            var csvData = await _mediator.Send(new UploadCsvFile.Command { FileBase = uploadSampleFile, PreprocessorId = preprocessorId, ApiServerId = apiServerId, Attribute = attribute });

            ColumnHeaders = csvData.ColumnHeaders;
            CsvTablePreview = csvData.TablePreview;

            ViewModel = new AddEditDataMapViewModel
            {
                MapName = mapName,
                PreprocessorId = preprocessorId,
                ApiVersionId = apiVersionId,
                ResourcePath = resourcePath,
                ApiServerId = apiServerId,
                PreprocessorLogMessages = csvData.PreprocessorLogMessages,
                CsvError = csvData.CsvError,
                Attribute = attribute
            };

            return dataMapId == 0
                ? RedirectToAction("Add")
                : RedirectToAction("Edit", new { Id = dataMapId });
        }

        private void MapTempViewModel(AddEditDataMapViewModel viewModel)
        {
            var tempViewModel = ViewModel;
            if (tempViewModel != null)
            {
                viewModel.MapName = tempViewModel.MapName;
                viewModel.PreprocessorId = tempViewModel.PreprocessorId;
                viewModel.ApiVersionId = tempViewModel.ApiVersionId;
                viewModel.ResourcePath = tempViewModel.ResourcePath;
                viewModel.ApiServerId = tempViewModel.ApiServerId;
                viewModel.PreprocessorLogMessages = tempViewModel.PreprocessorLogMessages;
                viewModel.Attribute = tempViewModel.Attribute;
                viewModel.CsvError = tempViewModel.CsvError;
            }
        }
    }
}