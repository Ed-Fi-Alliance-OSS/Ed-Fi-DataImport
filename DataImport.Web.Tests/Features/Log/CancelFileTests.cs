// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Threading.Tasks;
using DataImport.Models;
using DataImport.Web.Features.Log;
using NUnit.Framework;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.Log
{
    public class CancelFileTests
    {
        [Test]
        public async Task ShouldMarkProblematicFilesAsCanceledAndDeleteFromStorage()
        {
            var uploaded = await UploadFile();

            var fileId = uploaded.Id;
            var localPath = new Uri(uploaded.Url).LocalPath;

            try
            {
                Transaction(database =>
                {
                    var file = database.Files.Find(fileId);
                    file.Status = FileStatus.ErrorLoading;
                    database.SaveChanges();
                });

                var before = Query<File>(fileId);
                System.IO.File.Exists(localPath).ShouldBe(true);
                before.Status.ShouldBe(FileStatus.ErrorLoading);

                await Send(new CancelFile.Command
                {
                    Id = fileId
                });

                var after = Query<File>(fileId);
                System.IO.File.Exists(localPath).ShouldBe(false);
                after.Status.ShouldBe(FileStatus.Canceled);
            }
            catch
            {
                System.IO.File.Delete(localPath);
                throw;
            }
        }
    }
}