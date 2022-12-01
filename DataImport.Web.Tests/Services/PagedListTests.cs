// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using DataImport.Web.Services;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Web.Tests.Services
{
    [TestFixture]
    public class PagedListTests
    {
        private static IEnumerable<object> ListOfObjects => new object[100];

        private static PagedList<object> FetchPagedObjects(int pageNumber) =>
            Page<object>.Fetch((offset, size) => ListOfObjects.Skip(offset).Take(size), pageNumber);

        private static PagedList<object> FetchPagedObjects(int pageNumber, int pageSize) =>
            Page<object>.Fetch((offset, size) => ListOfObjects.Skip(offset).Take(size), pageNumber, pageSize);

        [Test]
        public void ShouldReturnHumanPageNumber()
        {
            var pageNumber = 0;

            var pagedList = FetchPagedObjects(pageNumber);
            pagedList.PageNumber.ShouldBe(1);

            pageNumber = 1;

            pagedList = FetchPagedObjects(pageNumber);
            pagedList.PageNumber.ShouldBe(1);

            pageNumber = 15;

            pagedList = FetchPagedObjects(pageNumber);
            pagedList.PageNumber.ShouldBe(15);
        }

        [Test]
        public void ShouldCalculateOffsetCorrectlyForFirstPage()
        {
            const int PageNumber = 0;
            const int ConfiguredPageSize = 30;

            Page<object>.Fetch((offset, size) =>
            {
                offset.ShouldBe(0);

                return ListOfObjects;
            }, PageNumber);

            Page<object>.Fetch((offset, size) =>
            {
                offset.ShouldBe(0);

                return ListOfObjects;
            }, PageNumber, ConfiguredPageSize);
        }

        [Test]
        public void ShouldCalculateOffsetCorrectlyForNPage()
        {
            var pageNumber = 2;
            const int ConfiguredPageSize = 25;

            Page<object>.Fetch((offset, size) =>
            {
                offset.ShouldBe(Page<object>.DefaultPageSize);

                return ListOfObjects;
            }, pageNumber);

            pageNumber = 10;

            Page<object>.Fetch((offset, size) =>
            {
                offset.ShouldBe(180);

                return ListOfObjects;
            }, pageNumber);

            Page<object>.Fetch((offset, size) =>
            {
                var calculatedOffSet = (pageNumber - 1) * ConfiguredPageSize;

                offset.ShouldBe(calculatedOffSet);

                return ListOfObjects;
            }, pageNumber, ConfiguredPageSize);
        }

        [Test]
        public void ShouldRequestOneAdditionalRecord()
        {
            Page<object>.Fetch((offset, size) =>
            {
                size.ShouldBe(Page<object>.DefaultPageSize + 1);

                return ListOfObjects;
            }, 0);

            const int ConfiguredPageSize = 50;

            Page<object>.Fetch((offset, size) =>
            {
                size.ShouldBe(51);

                return ListOfObjects;
            }, 0, ConfiguredPageSize);
        }

        [Test]
        public void ShouldDetectIfNextPageHasResults()
        {
            const int ConfiguredPageSize = 50;
            var pageNumber = 1;

            var pagedObjects = FetchPagedObjects(pageNumber, ConfiguredPageSize);
            pagedObjects.NextPageHasResults.ShouldBeTrue();

            pageNumber = 2;

            pagedObjects = FetchPagedObjects(pageNumber, ConfiguredPageSize);
            pagedObjects.NextPageHasResults.ShouldBeFalse();
        }

        [Test]
        public void ShouldReturnSameAmountOfItemsAsPageSize()
        {
            const int ConfiguredPageSize = 10;
            const int PageNumber = 1;

            var pagedObjects = FetchPagedObjects(PageNumber, ConfiguredPageSize);

            pagedObjects.Items.Count().ShouldBe(ConfiguredPageSize);
        }

        [Test]
        public void ShouldNotAllowPageSizeOf100()
        {
            var pageSize = 99;

            Should.NotThrow(() => FetchPagedObjects(1, pageSize));

            pageSize = 100;

            Should.Throw<ArgumentException>(() => FetchPagedObjects(1, pageSize));
        }
    }
}
