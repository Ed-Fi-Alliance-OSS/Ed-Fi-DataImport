// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataImport.Web.Services
{
    public class Page<T>
    {
        private readonly Func<int, int, Task<IReadOnlyList<T>>> _getApiRecordsAsync;
        private readonly Func<int, int, IEnumerable<T>> _getApiRecords;

        public static readonly int DefaultPageSize = 20;

        private Page(Func<int, int, Task<IReadOnlyList<T>>> getApiRecordsAsync)
        {
            _getApiRecordsAsync = getApiRecordsAsync;
        }

        public Page(Func<int, int, IEnumerable<T>> getApiRecords)
        {
            _getApiRecords = getApiRecords;
        }

        public static async Task<PagedList<T>> FetchAsync(Func<int, int, Task<IReadOnlyList<T>>> getApiRecords, int pageNumber) => await FetchAsync(getApiRecords, pageNumber, DefaultPageSize);

        public static async Task<PagedList<T>> FetchAsync(Func<int, int, Task<IReadOnlyList<T>>> getApiRecords, int pageNumber, int pageSize)
        {
            var service = new Page<T>(getApiRecords);

            return await service.PagedListAsync(pageNumber, pageSize);
        }

        private async Task<PagedList<T>> PagedListAsync(int pageNumber, int pageSize)
        {
            if (pageSize >= 100)
            {
                throw new ArgumentException("Page numbers of 100 or greater are not currently supported due to limit constraints on the EdFi API");
            }

            var humanPageNumber = pageNumber == 0 ? 1 : pageNumber;

            var recordsToOffset = (humanPageNumber - 1) * pageSize;

            var records = await _getApiRecordsAsync(recordsToOffset, pageSize + 1);

            return new PagedList<T>
            {
                PageNumber = humanPageNumber,
                NextPageHasResults = records.Count > pageSize,
                Items = records.Take(pageSize).ToList()
            };
        }

        public static PagedList<T> Fetch(Func<int, int, IEnumerable<T>> getApiRecords, int pageNumber)
        {
            return Fetch(getApiRecords, pageNumber, DefaultPageSize);
        }

        public static PagedList<T> Fetch(Func<int, int, IEnumerable<T>> getApiRecords, int pageNumber, int pageSize)
        {
            var service = new Page<T>(getApiRecords);

            return service.PagedList(pageNumber, pageSize);
        }

        private PagedList<T> PagedList(int pageNumber, int pageSize)
        {
            if (pageSize >= 100)
            {
                throw new ArgumentException("Page numbers of 100 or greater are not currently supported due to limit constraints on the EdFi API");
            }

            var humanPageNumber = pageNumber == 0 ? 1 : pageNumber;

            var recordsToOffset = (humanPageNumber - 1) * pageSize;

            var records = _getApiRecords(recordsToOffset, pageSize + 1);

            return new PagedList<T>
            {
                PageNumber = humanPageNumber,
                NextPageHasResults = records.Count() > pageSize,
                Items = records.Take(pageSize)
            };
        }
    }

    public class PagedList<T>
    {
        public int PageNumber { get; set; }
        public bool NextPageHasResults { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}
