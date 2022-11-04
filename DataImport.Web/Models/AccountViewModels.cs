// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace DataImport.Web.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginConfirmationValidator : AbstractValidator<ExternalLoginConfirmationViewModel>
    {
        public ExternalLoginConfirmationValidator() => RuleFor(x => x.Email).NotEmpty();
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        public string Provider { get; set; }

        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class VerifyCodeValidator : AbstractValidator<VerifyCodeViewModel>
    {
        public VerifyCodeValidator()
        {
            RuleFor(x => x.Provider).NotEmpty();
            RuleFor(x => x.Code).NotEmpty();
        }
    }

    public class ForgotViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ForgotValidator : AbstractValidator<ForgotViewModel>
    {
        public ForgotValidator() => RuleFor(x => x.Email).NotEmpty();
    }

    public class LoginViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class LoginValidator : AbstractValidator<LoginViewModel>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class RegisterViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }

    public class RegisterValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("The password and confirmation password do not match.");
        }
    }

    public class ResetPasswordViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordViewModel>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
            RuleFor(x => x.ConfirmPassword).Equal(x => x.Password).WithMessage("The password and confirmation password do not match.");
        }
    }

    public class ForgotPasswordViewModel
    {
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordViewModel>
    {
        public ForgotPasswordValidator() => RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
