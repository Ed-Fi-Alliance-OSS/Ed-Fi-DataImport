// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Web.Infrastructure;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Web.Tests.Infrastructure
{
    public class TimeSpanExtensionsTests
    {
        [Test]
        public void ShouldProvideBriefNaturalLanguageSummaryOfTimeSpans()
        {
            ReadableDuration(days: 5, hours: 4, minutes: 3, seconds: 2).ShouldBe("5 days, 4 hours, 3 minutes, and 2 seconds");
            ReadableDuration(days: 1, hours: 1, minutes: 1, seconds: 1).ShouldBe("1 day, 1 hour, 1 minute, and 1 second");
            
            ReadableDuration(days: 0, hours: 4, minutes: 3, seconds: 2).ShouldBe("4 hours, 3 minutes, and 2 seconds");
            ReadableDuration(days: 5, hours: 0, minutes: 3, seconds: 2).ShouldBe("5 days, 3 minutes, and 2 seconds");
            ReadableDuration(days: 5, hours: 4, minutes: 0, seconds: 2).ShouldBe("5 days, 4 hours, and 2 seconds");
            ReadableDuration(days: 5, hours: 4, minutes: 3, seconds: 0).ShouldBe("5 days, 4 hours, and 3 minutes");

            ReadableDuration(days: 0, hours: 0, minutes: 3, seconds: 2).ShouldBe("3 minutes and 2 seconds");
            ReadableDuration(days: 0, hours: 4, minutes: 0, seconds: 2).ShouldBe("4 hours and 2 seconds");
            ReadableDuration(days: 0, hours: 4, minutes: 3, seconds: 0).ShouldBe("4 hours and 3 minutes");
            ReadableDuration(days: 5, hours: 0, minutes: 0, seconds: 2).ShouldBe("5 days and 2 seconds");
            ReadableDuration(days: 5, hours: 0, minutes: 3, seconds: 0).ShouldBe("5 days and 3 minutes");
            ReadableDuration(days: 5, hours: 4, minutes: 0, seconds: 0).ShouldBe("5 days and 4 hours");

            ReadableDuration(days: 5, hours: 0, minutes: 0, seconds: 0).ShouldBe("5 days");
            ReadableDuration(days: 0, hours: 4, minutes: 0, seconds: 0).ShouldBe("4 hours");
            ReadableDuration(days: 0, hours: 0, minutes: 3, seconds: 0).ShouldBe("3 minutes");
            ReadableDuration(days: 0, hours: 0, minutes: 0, seconds: 2).ShouldBe("2 seconds");

            ReadableDuration(days: 0, hours: 0, minutes: 0, seconds: 0).ShouldBe("0 seconds");
        }

        [Test]
        public void ShouldDescribeAbsoluteValue()
        {
            var today = DateTime.Today;
            var yesterday = DateTime.Today.AddDays(-1);

            (today - yesterday).ToReadableDuration().ShouldBe("1 day");
            (yesterday - today).ToReadableDuration().ShouldBe("1 day");
        }

        private static string ReadableDuration(int days, int hours, int minutes, int seconds)
            => new TimeSpan(days, hours, minutes, seconds)
                .ToReadableDuration();
    }
}