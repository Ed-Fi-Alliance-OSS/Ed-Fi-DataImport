// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using DataImport.Models;
using DataImport.Server.TransformLoad.Features;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Server.TransformLoad.Tests.Features
{
    [SetCulture("en-US")]
    [TestFixture]
    public class HelperTests
    {
        [Test]
        public void Test_TimeToExecute()
        {
            var mockAgent = new Agent();
            var mockNow = DateTime.Parse("11/30/2017 12:00pm"); // This is a Thursday

            mockAgent.LastExecuted = DateTime.Parse("11/29/2017 12:00pm"); // This is a Wednesday
            var mockSchedule = new AgentSchedule
            {
                Day = 4, // Thursday
                Hour = 12, // 12pm
                Minute = 30 // :30 minutes
            };

            mockAgent.AgentSchedules.Add(mockSchedule);
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeFalse();

            mockNow = DateTime.Parse("11/30/2017 12:50pm"); // This is a Thursday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeTrue();

            mockAgent.LastExecuted = DateTime.Parse("11/30/2017 12:50pm"); // This is a Thursday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeFalse();

            var mockSchedule2 = new AgentSchedule
            {
                Day = 5, // Friday
                Hour = 20, // 8pm
                Minute = 0 // :00 minutes
            };

            mockAgent.AgentSchedules.Add(mockSchedule2);
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeFalse();

            mockNow = DateTime.Parse("12/01/2017 8:01pm"); // This is a Friday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeTrue();

            mockAgent.LastExecuted = DateTime.Parse("12/01/2017 8:01pm"); // This is a Friday

            mockNow = DateTime.Parse("12/03/2017 8:01pm"); // This is a Sunday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeFalse();

            mockNow = DateTime.Parse("12/07/2017 12:29pm"); // This is a Thursday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeFalse();

            mockNow = DateTime.Parse("12/07/2017 12:31pm"); // This is a Thursday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeTrue();

            mockAgent.LastExecuted = DateTime.Parse("12/07/2017 12:31pm"); // This is a Thursday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeFalse();

            mockNow = DateTime.Parse("12/08/2017 08:00am"); // This is a Friday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeFalse();

            mockNow = DateTime.Parse("12/08/2017 08:00pm"); // This is a Friday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeTrue();

            mockNow = DateTime.Parse("12/10/2017 08:00am"); // This is a Sunday
            Helper.ShouldExecuteOnSchedule(mockAgent, mockNow).ShouldBeFalse();
        }

        [Test]
        public void ConvertPathToUri()
        {
            const string testPath = @"C:\Temp\fileName12345.log";
            var result = Common.Helpers.UrlUtility.ConvertLocalPathToUri(testPath);

            // Assert
            result.ShouldBe("file:///C:/Temp/fileName12345.log");
        }

        [Test]
        public void CombinesUrls()
        {
            Common.Helpers.UrlUtility.CombineUri("https://localhost:54746/api/", "/calendarDates").ShouldBe("https://localhost:54746/api/calendarDates");
            Common.Helpers.UrlUtility.CombineUri("https://localhost:54746/api", "calendarDates").ShouldBe("https://localhost:54746/api/calendarDates");
            Common.Helpers.UrlUtility.CombineUri("https://localhost:54746/api/", "calendarDates").ShouldBe("https://localhost:54746/api/calendarDates");
            Common.Helpers.UrlUtility.CombineUri("https://localhost:54746/api/", "calendarDates?limit=100").ShouldBe("https://localhost:54746/api/calendarDates?limit=100");
            Common.Helpers.UrlUtility.CombineUri().ShouldBeEmpty();
            Common.Helpers.UrlUtility.CombineUri("https://localhost:54746/api/").ShouldBe("https://localhost:54746/api");
            Common.Helpers.UrlUtility.CombineUri("https://localhost:54746", "api", "calendarDates").ShouldBe("https://localhost:54746/api/calendarDates");
        }
    }
}
