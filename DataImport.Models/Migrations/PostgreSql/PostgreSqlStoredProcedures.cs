// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.Models.Migrations.PostgreSql
{
    public class PostgreSqlStoredProcedures
    {
        public const string AddApplicationLog = @"create or replace procedure public.ApplicationLog_AddEntry(
                                                machineName Varchar(200),
                                                logged Timestamp(6) WITH TIME ZONE,
                                                level Varchar(5),
                                                userName Varchar(200),
                                                message Text,
                                                logger Varchar(300),
                                                properties Text,
                                                serverName Varchar(200),
                                                port Varchar(100),
                                                url Varchar(2000),
                                                serverAddress Varchar(100),
                                                remoteAddress Varchar(100),
                                                exception Text
                                                )
                                            language plpgsql
                                            as $$
                                            begin
                                                INSERT INTO public.ApplicationLogs(
                                                MachineName,
                                                Logged,
                                                Level,
                                                UserName,
                                                Message,
                                                Logger,
                                                Properties,
                                                ServerName,
                                                Port,
                                                Url,
                                                ServerAddress,
                                                RemoteAddress,
                                                Exception
                                                ) VALUES (
                                                machineName,
                                                logged,
                                                level,
                                                userName,
                                                message,
                                                logger,
                                                properties,
                                                serverName,
                                                port,
                                                url,
                                                serverAddress,
                                                remoteAddress,
                                                exception
                                                );
                                            commit;
                                            end;$$";

       
    }
}
