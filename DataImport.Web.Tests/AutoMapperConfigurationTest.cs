// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using AutoMapper;
using NUnit.Framework;

namespace DataImport.Web.Tests
{
    [TestFixture]
    public class AutoMapperConfigurationTest
    {
        [Test]
        public void AssertConfigurationIsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddMaps(typeof(Startup).Assembly));
            config.AssertConfigurationIsValid();
        }
    }
}
