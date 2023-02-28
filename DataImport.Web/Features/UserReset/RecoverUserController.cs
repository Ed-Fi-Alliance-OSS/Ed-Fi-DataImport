// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using DataImport.Models;
using System.Text.RegularExpressions;

namespace DataImport.Web.Features.UserReset
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class RecoverUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RecoverUserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous()]
        [Consumes("application/x-www-form-urlencoded"), Produces("application/json")]
        public async Task<IActionResult> Post([FromForm] ResetRequest resetRequest)
        {
            var existingUser = await _userManager.FindByNameAsync(resetRequest.UserName);
            if (existingUser.LockoutEnabled)
            {
                await _userManager.SetLockoutEnabledAsync(existingUser, false);
                await _userManager.SetLockoutEndDateAsync(existingUser, null);
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);

            var result = await _userManager.ResetPasswordAsync(existingUser, token, resetRequest.NewPassword);

            if (result.Succeeded)
            {
                return Ok($"Reset password succeeded for {existingUser.UserName}");
            }
            else
            {
                var errorMessage = string.Join("; ", result.Errors
                                    .Select(x => x.Description));
                return StatusCode(500, $"Reset password failed. Errors: {errorMessage}");
            }
        }
    }

    public class Validator : AbstractValidator<ResetRequest>
    {
        private readonly DataImportDbContext _dbContext;
        private readonly IOptions<AppSettings> _options;

        public Validator(DataImportDbContext dbContext, IOptions<AppSettings> options)
        {
            _dbContext = dbContext;
            _options = options;

            RuleFor(x => x.UserName)
              .NotEmpty()
              .Must(UserShouldExists).WithMessage(model =>
                  $"{model.UserName} does not exist. Please provide valid user name.");

            RuleFor(x => x.NewPassword).NotEmpty().Must(FollowPasswordRules)
                .WithMessage("Password does not satisfy password rules." +
                "Rules: At least one upper-case, lower-case, numeric-value, special-character with minimum length of 6.");

            RuleFor(x => x.UserRecoveryToken).NotEmpty().
                Must(MatchAppsettingValue).WithMessage("User recovery token is not valid");
        }

        private bool UserShouldExists(string username)
        {
            return _dbContext.Users.FirstOrDefault(user => user.UserName == username) != null;
        }

        private bool FollowPasswordRules(string password)
        {
            var expression = "(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{6,}";
            var regex = new Regex(expression);
            return regex.IsMatch(password);
        }

        private bool MatchAppsettingValue(string userRecoveryToken)
        {
            return _options.Value.UserRecoveryToken.Equals(userRecoveryToken);
        }
    }

    public class ResetRequest
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public string UserRecoveryToken { get; set; }
    }
}

