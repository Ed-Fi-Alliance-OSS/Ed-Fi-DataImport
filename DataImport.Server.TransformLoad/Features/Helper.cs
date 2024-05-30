// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataImport.Server.TransformLoad.Features
{
    public static class Helper
    {
        public const string EducationOrganizationIdJsonPath = "educationOrganizationReference.educationOrganizationId";
        public const string SchoolIdJsonPath = "reportedSchoolReference.schoolId";

        public static async Task<bool> DoesFileExistInLog(DataImportDbContext dbContext, int agentId, string file)
        {
            var shortFileName = file.Substring(file.LastIndexOf('/') + 1);

            var fileCount = await dbContext.Files
                .CountAsync(f => (f.FileName == shortFileName && f.AgentId == agentId));

            var fileFound = fileCount != 0;

            return fileFound;
        }

        public static async Task<File> GetFileRow(DataImportDbContext dbContext, int agentId, string file)
        {
            var shortFileName = file.Substring(file.LastIndexOf('/') + 1);

            var fileResult = await dbContext.Files
                .FirstAsync(f => (f.FileName == shortFileName && f.AgentId == agentId));

            return fileResult;
        }
        public static bool ShouldExecuteOnSchedule(Agent agent, DateTimeOffset? nowDate = null)
        {
            if (!nowDate.HasValue)
                nowDate = DateTimeOffset.Now;

            var shouldRun = false;

            if (agent.AgentSchedules.Count <= 0) return false;
            var nowDay = (int) nowDate.Value.DayOfWeek;
            var nowHour = nowDate.Value.Hour;
            var nowMinute = nowDate.Value.Minute;

            IEnumerable<AgentSchedule> sortedSchedule = from schedules in agent.AgentSchedules
                                                        orderby schedules.Day ascending, schedules.Hour ascending, schedules.Minute ascending
                                                        select schedules;

            foreach (AgentSchedule schedule in sortedSchedule)
            {
                var scheduleDateTime = DateTime.Parse(nowDate.Value.Date.ToShortDateString() + " " + schedule.Hour + ":" + schedule.Minute);
                scheduleDateTime = scheduleDateTime.AddDays(-((int) nowDate.Value.DayOfWeek - schedule.Day));

                if (!agent.LastExecuted.HasValue || scheduleDateTime > agent.LastExecuted)
                {
                    if (schedule.Day <= nowDay)
                    {
                        if (schedule.Hour < nowHour)
                            shouldRun = true;
                        else if (schedule.Hour == nowHour && schedule.Minute <= nowMinute)
                            shouldRun = true;
                    }
                }
            }
            return shouldRun;
        }

        public static int? GetEdOrgIdFromCsv(Dictionary<string, string> currentRow, string selectedIngestionLogEdOrgIdColumn)
        {
            int edOrgId;

            if (string.IsNullOrEmpty(selectedIngestionLogEdOrgIdColumn))
                return null;

            if (!currentRow.ContainsKey(selectedIngestionLogEdOrgIdColumn))
                return null;

            string csvValue = currentRow[selectedIngestionLogEdOrgIdColumn] as string;

            if (string.IsNullOrEmpty(csvValue))
                return null;

            int.TryParse(csvValue, out edOrgId);

            return edOrgId;
        }

        public static int? GetEdOrgIdFromJsonTransformed(JToken jToken)
        {
            int? edOrgId = null;
            JToken jtokenEdOrgId = jToken.SelectToken(EducationOrganizationIdJsonPath);
            if (jtokenEdOrgId != null)
                edOrgId = jtokenEdOrgId.Value<int?>();

            if (edOrgId == null)
            {
                JToken jtokenSchoolId = jToken.SelectToken(SchoolIdJsonPath);
                if (jtokenSchoolId != null)
                    edOrgId = jtokenSchoolId.Value<int?>();
            }
            return edOrgId;
        }
    }
}
