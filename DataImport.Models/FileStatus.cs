// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq;

//Disable warnings about using obsolete members like FileStatus.Delete.
#pragma warning disable 618

namespace DataImport.Models
{
    public enum FileStatus
    {
        ErrorLoading = 1,
        ErrorTransform = 2,
        ErrorUploaded = 3,
        Loaded = 4,
        Loading = 5,
        Transforming = 6,
        Uploaded = 7,
        Retry = 8,

        [Obsolete("Use Canceled and Loaded to represent files that have been processed " +
                  "and no longer exist in storage. Keep this enum value so that preexisting " +
                  "records remain valid.")]
        Deleted = 9,

        Canceled = 10
    }

    public static class FileStatusExtensions
    {
        public static bool CanBeRetried(this FileStatus currentStatus)
        {
            switch (currentStatus)
            {
                // Problematic files can be retried.
                case FileStatus.ErrorLoading:
                case FileStatus.ErrorTransform:
                case FileStatus.ErrorUploaded:
                    return true;

                // There's no need to offer a retry for work that is already pending.
                case FileStatus.Uploaded:
                case FileStatus.Retry:
                    return false;

                // Completed work and in-progress work cannot be retried.
                case FileStatus.Loaded:
                case FileStatus.Loading:
                case FileStatus.Transforming:
                case FileStatus.Deleted:
                case FileStatus.Canceled:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException(nameof(currentStatus), currentStatus, $"Cannot determine whether files with status {currentStatus} can be retried.");
            }
        }

        public static readonly FileStatus[] CancelableStatuses = {
            FileStatus.ErrorLoading,
            FileStatus.ErrorTransform,
            FileStatus.ErrorUploaded,
            FileStatus.Uploaded,
            FileStatus.Retry
        };

        public static bool CanBeCanceled(this FileStatus currentStatus)
        {
            // Problematic files and pending work can be canceled.
            if (CancelableStatuses.Contains(currentStatus))
                return true;

            //Completed work and in-progress work cannot be canceled.
            if (currentStatus == FileStatus.Loaded || currentStatus == FileStatus.Loading ||
                currentStatus == FileStatus.Transforming || currentStatus == FileStatus.Deleted ||
                currentStatus == FileStatus.Canceled)
                return false;

            throw new ArgumentOutOfRangeException(nameof(currentStatus), currentStatus,
                $"Cannot determine whether files with status {currentStatus} can be canceled.");
        }
    }
}