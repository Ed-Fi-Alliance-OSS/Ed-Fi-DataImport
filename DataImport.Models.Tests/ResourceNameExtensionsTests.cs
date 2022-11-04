// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using NUnit.Framework;
using Shouldly;

namespace DataImport.Models.Tests
{
    public class ResourceNameExtensionsTests
    {
        [Test]
        public void ShouldMapOdsV2ResourcePathsToUserFacingNames()
        {
            ResourceName("/schoolYearTypes").ShouldBe("School Year Types");
            ResourceName("/weaponDescriptors").ShouldBe("Weapon Descriptors");
            ResourceName("/customResource").ShouldBe("Custom Resource");
        }

        [Test]
        public void ShouldMapOdsV3CoreResourcePathsToUserFacingNames()
        {
            ResourceName("/ed-fi/schoolYearTypes").ShouldBe("School Year Types");
            ResourceName("/ed-fi/weaponDescriptors").ShouldBe("Weapon Descriptors");
        }

        [Test]
        public void ShouldMapOdsV3ExtensionResourcePathsToUserFacingNames()
        {
            ResourceName("/my-extension/schoolYearTypes").ShouldBe("School Year Types [My Extension]");
            ResourceName("/myExtension/completelyUnanticipatedConcept").ShouldBe("Completely Unanticipated Concept [My Extension]");
        }

        [Test]
        public void ShouldMapUnexpectedResourcePathStringsToReasonableUserFacingNames()
        {
            ResourceName("notAPath").ShouldBe("Not a Path");
            ResourceName("path/missingLeadingSlash").ShouldBe("Missing Leading Slash [Path]");
            ResourceName("/unexpected//slashes/count").ShouldBe("Unexpected Slashes Count");
            ResourceName("already separated").ShouldBe("Already Separated");
            ResourceName("").ShouldBe("");
            ResourceName(null).ShouldBe("");
        }

        private static string ResourceName(string resourcePath)
        {
            var viaResource = new Resource { Path = resourcePath }.ToResourceName();
            var viaBootstrapData = new BootstrapData { ResourcePath = resourcePath }.ToResourceName();
            var viaDataMap = new DataMap { ResourcePath = resourcePath }.ToResourceName();
            var viaString = resourcePath.ToResourceName();

            viaResource.ShouldBe(viaBootstrapData);
            viaBootstrapData.ShouldBe(viaDataMap);
            viaDataMap.ShouldBe(viaString);

            return viaString;
        }
    }
}
