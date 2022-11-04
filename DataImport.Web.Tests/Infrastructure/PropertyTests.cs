// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using DataImport.TestHelpers;
using DataImport.Web.Infrastructure;
using NUnit.Framework;

namespace DataImport.Web.Tests.Infrastructure
{
    [TestFixture]
    public class PropertyTests
    {
        private class Sample
        {
            public int Integer { get; set; }
            public int? NullableInteger { get; set; }
            public Sample Parent { get; set; }
        }

        [Test]
        public void ShouldGetPropertyInfoFromLambdaExpression()
        {
            var integer = Property.From<Sample, int>(x => x.Integer);
            var nullableInteger = Property.From<Sample, int?>(x => x.NullableInteger);
            var sample = Property.From<Sample, Sample>(x => x.Parent);

            new[] { integer, nullableInteger, sample }
                .Select(x => new { x.Name, x.PropertyType })
                .ShouldMatch(
                    new { Name = "Integer", PropertyType = typeof(int) },
                    new { Name = "NullableInteger", PropertyType = typeof(int?) },
                    new { Name = "Parent", PropertyType = typeof(Sample) });
        }
    }
}
