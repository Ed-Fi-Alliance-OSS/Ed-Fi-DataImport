// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using System;
using DataImport.TestHelpers;
using DataImport.Web.Helpers;
using FluentValidation.Results;
using MediatR;
using Shouldly;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests
{
    public static class Assertions
    {
        public static void ShouldValidate<TResult>(this IRequest<TResult> message)
            => Validation(message).ShouldBeSuccessful();

        public static void ShouldNotValidate<TResult>(this IRequest<TResult> message, params string[] expectedErrors)
            => Validation(message).ShouldBeFailure(expectedErrors);

        public static void ShouldBeSuccessful(this ValidationResult result)
        {
            var indentedErrorMessages = result
                .Errors
                .OrderBy(x => x.ErrorMessage)
                .Select(x => "    " + x.ErrorMessage)
                .ToArray();

            var actual = String.Join(Environment.NewLine, indentedErrorMessages);

            result.IsValid.ShouldBeTrue($"Expected no validation errors, but found {result.Errors.Count}:{Environment.NewLine}{actual}");
        }

        public static void ShouldBeFailure(this ValidationResult result, params string[] expectedErrors)
        {
            result.IsValid.ShouldBeFalse("Expected validation errors, but the message passed validation.");

            result.Errors
                .OrderBy(x => x.ErrorMessage)
                .Select(x => x.ErrorMessage)
                .ShouldMatch(expectedErrors.OrderBy(x => x).ToArray());
        }

        public static void AssertToast(this ToastResponse response, string message, bool isSuccess = true)
        {
            response.IsSuccess.ShouldBe(isSuccess);
            response.Message.ShouldMatch(message);
        }
    }
}
