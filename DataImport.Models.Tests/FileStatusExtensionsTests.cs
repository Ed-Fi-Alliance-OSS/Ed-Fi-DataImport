// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using NUnit.Framework;
using Shouldly;

//Disable warnings about using obsolete members like FileStatus.Delete.
#pragma warning disable 618

namespace DataImport.Models.Tests
{
    public class FileStatusExtensionsTests
    {
        [Test]
        public void ShouldDetermineWhetherFileCanBeRetried()
        {
            //This Length check ensures that we can detect the need
            //to update the following assertions for completeness,
            //such as when a new FileStatus is added.
            AllFileStatuses().Length.ShouldBe(10);

            // Problematic files can be retried.
            FileStatus.ErrorLoading.CanBeRetried().ShouldBe(true);
            FileStatus.ErrorTransform.CanBeRetried().ShouldBe(true);
            FileStatus.ErrorUploaded.CanBeRetried().ShouldBe(true);

            // There's no need to offer a retry for work that is already pending.
            FileStatus.Uploaded.CanBeRetried().ShouldBe(false);
            FileStatus.Retry.CanBeRetried().ShouldBe(false);

            // Completed work and in-progress work cannot be retried.
            FileStatus.Loaded.CanBeRetried().ShouldBe(false);
            FileStatus.Loading.CanBeRetried().ShouldBe(false);
            FileStatus.Transforming.CanBeRetried().ShouldBe(false);
            FileStatus.Deleted.CanBeRetried().ShouldBe(false);
            FileStatus.Canceled.CanBeRetried().ShouldBe(false);

            //Unexpected values are untrusted.
            Action unanswerableQuestion = () => ((FileStatus) (-1)).CanBeRetried();
            unanswerableQuestion.ShouldThrow<ArgumentOutOfRangeException>()
                .Message.ShouldStartWith("Cannot determine whether files with status -1 can be retried.");
        }

        [Test]
        public void ShouldDetermineWhetherFileCanBeCancelled()
        {
            //This Length check ensures that we can detect the need
            //to update the following assertions for completeness,
            //such as when a new FileStatus is added.
            AllFileStatuses().Length.ShouldBe(10);

            // Problematic files and pending work can be canceled.
            FileStatus.ErrorLoading.CanBeCanceled().ShouldBe(true);
            FileStatus.ErrorTransform.CanBeCanceled().ShouldBe(true);
            FileStatus.ErrorUploaded.CanBeCanceled().ShouldBe(true);
            FileStatus.Uploaded.CanBeCanceled().ShouldBe(true);
            FileStatus.Retry.CanBeCanceled().ShouldBe(true);

            //Completed work and in-progress work cannot be canceled.
            FileStatus.Loaded.CanBeCanceled().ShouldBe(false);
            FileStatus.Loading.CanBeCanceled().ShouldBe(false);
            FileStatus.Transforming.CanBeCanceled().ShouldBe(false);
            FileStatus.Deleted.CanBeCanceled().ShouldBe(false);
            FileStatus.Canceled.CanBeCanceled().ShouldBe(false);

            //Unexpected values are untrusted.
            Action unanswerableQuestion = () => ((FileStatus) (-1)).CanBeCanceled();
            unanswerableQuestion.ShouldThrow<ArgumentOutOfRangeException>()
                .Message.ShouldStartWith("Cannot determine whether files with status -1 can be canceled.");
        }

        private static FileStatus[] AllFileStatuses()
            => (FileStatus[]) Enum.GetValues(typeof(FileStatus));
    }
}
