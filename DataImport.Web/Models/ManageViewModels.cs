// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DataImport.Web.Models
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationScheme> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }
    }

    public class SetPasswordValidator : AbstractValidator<SetPasswordViewModel>
    {
        public SetPasswordValidator()
        {
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("The new password and confirmation password do not match.");
        }
    }

    public class ChangePasswordViewModel
    {
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordViewModel>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.OldPassword).NotEmpty();
            RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(100);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword).WithMessage("The new password and confirmation password do not match.");
        }
    }

    public class AddPhoneNumberViewModel
    {
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class AddPhoneNumberValidator : AbstractValidator<AddPhoneNumberViewModel>
    {
        public AddPhoneNumberValidator()
            => RuleFor(x => x.Number)
                .NotEmpty()
                .Must(number => new PhoneAttribute().IsValid(number))
                .WithMessage("Enter a valid phone number.");
    }

    public class VerifyPhoneNumberViewModel
    {
        [Display(Name = "Code")]
        public string Code { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class VerifyPhoneNumberValidator : AbstractValidator<VerifyPhoneNumberViewModel>
    {
        public VerifyPhoneNumberValidator()
        {
            RuleFor(x => x.Code).NotEmpty();
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .Must(number => new PhoneAttribute().IsValid(number))
                .WithMessage("Enter a valid phone number.");
        }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Providers { get; set; }
    }
}