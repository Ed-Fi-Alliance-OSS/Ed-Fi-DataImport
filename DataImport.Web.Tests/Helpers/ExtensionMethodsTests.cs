// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Common.ExtensionMethods;
using DataImport.Web.Helpers;
using NUnit.Framework;
using Shouldly;
using System.IO;
using System.Text;

namespace DataImport.Web.Tests.Helpers
{
    [SetCulture("en-US")]
    public class ExtensionMethodsTests
    {
        [Test]
        public void ShouldReturnExpectedTotalLinesBySkippingTheEmptyLinesAndHeader()
        {
            // Arrange
            var csvContent = "adminyear,DistrictNumber,DistrictName \n"
                             + "2017,255901,Grand Bend ISD \n"
                             + "2018,255901,Grand Bend ISD \n \n";

            var byteArray = Encoding.ASCII.GetBytes(csvContent);
            var stream = new MemoryStream(byteArray);

            // Act
            var result = stream.TotalLines(true);

            // Assert
            result.ShouldBe(2);
        }

        [Test]
        public void ShouldReturnExpectedTotalLinesWithNoLineEndingSequence()
        {
            // Arrange
            var csvContent = "adminyear,DistrictNumber,DistrictName \n"
                             + "2017,255901,Grand Bend ISD \n"
                             + "2018,255901,Grand Bend ISD";

            var byteArray = Encoding.ASCII.GetBytes(csvContent);
            var stream = new MemoryStream(byteArray);

            // Act
            var result = stream.TotalLines(true);

            // Assert
            result.ShouldBe(2);
        }

        [Test]
        public void ShouldReturnExpectedTotalLinesWhenRowWithEmptyValuesExists()
        {
            // Arrange
            var csvContent = "adminyear,DistrictNumber,DistrictName \n"
                             + "2017,255901,Grand Bend ISD \n"
                             + "2018,255901,Grand Bend ISD \n"
                             + " , , ,";

            var byteArray = Encoding.ASCII.GetBytes(csvContent);
            var stream = new MemoryStream(byteArray);

            // Act
            var result = stream.TotalLines(true);

            // Assert
            result.ShouldBe(2);
        }

        [Test]
        public void ShouldReturnExpectedTotalLinesWhenPassingNonCsvFile()
        {
            // Arrange
            var content = @"<Root>
                                 <Person>
                                  <Name>Name1</Name>
                                  <Age>20</Age>
                                </Person>
                                <Person>
                                  <Name>Name2</Name>
                                  <Age>27</Age>
                                </Person>
                           </Root>";

            var byteArray = Encoding.ASCII.GetBytes(content);
            var stream = new MemoryStream(byteArray);

            // Act
            var result = stream.TotalLines(false);

            // Assert
            result.ShouldBe(10);
        }

        [Test]
        public void ToDescriptorNameShouldReturnEmptyStringIfDescriptorIsNull()
        {
            string descriptor = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.AreEqual(string.Empty, descriptor.ToDescriptorName());
        }
    }
}
