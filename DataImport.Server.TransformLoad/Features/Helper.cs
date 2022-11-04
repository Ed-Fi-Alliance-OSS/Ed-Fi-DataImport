// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataImport.Server.TransformLoad.Features
{
    public static class Helper
    {
        public static async Task<bool> DoesFileExistInLog(DataImportDbContext dbContext, int agentId, string file)
        {
            var shortFileName = file.Substring(file.LastIndexOf('/') + 1);

            var fileCount = await dbContext.Files
                .CountAsync(f => (f.FileName == shortFileName && f.AgentId == agentId));

            var fileFound = fileCount != 0;

            return fileFound;
        }

        public static bool ShouldExecuteOnSchedule(Agent agent, DateTimeOffset? nowDate = null)
        {
            if (!nowDate.HasValue)
                nowDate = DateTimeOffset.Now;

            var shouldRun = false;

            if (agent.AgentSchedules.Count <= 0) return false;
            var nowDay = (int)nowDate.Value.DayOfWeek;
            var nowHour = nowDate.Value.Hour;
            var nowMinute = nowDate.Value.Minute;

            IEnumerable<AgentSchedule> sortedSchedule = from schedules in agent.AgentSchedules
                                                        orderby schedules.Day ascending, schedules.Hour ascending, schedules.Minute ascending
                                                        select schedules;

            foreach (AgentSchedule schedule in sortedSchedule)
            {
                var scheduleDateTime = DateTime.Parse(nowDate.Value.Date.ToShortDateString() + " " + schedule.Hour + ":" + schedule.Minute);
                scheduleDateTime = scheduleDateTime.AddDays(-((int)nowDate.Value.DayOfWeek - schedule.Day));

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
    }
}
