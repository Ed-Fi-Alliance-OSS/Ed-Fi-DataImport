// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using DataImport.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace DataImport.Web.Features.UserReset
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [AllowAnonymous]
    public class RecoverUserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOptions<AppSettings> _options;

        public RecoverUserController(UserManager<ApplicationUser> userManager, IOptions<AppSettings> options)
        {
            _userManager = userManager;
            _options = options;
        }

        [HttpPost]
        [AllowAnonymous()]
        [ActionName("Reset")]
        [Consumes("application/x-www-form-urlencoded"), Produces("application/json")]
        public async Task<IActionResult> Reset([FromForm] ResetRequest resetRequest)
        {
            if (ModelState.IsValid)
            {
                if (!string.IsNullOrEmpty(_options.Value.UserRecoveryToken) &&
                    resetRequest.UserRecoveryToken.Equals(_options.Value.UserRecoveryToken))
                {
                    var existingUser = await _userManager.FindByNameAsync(resetRequest.UserName);
                    if (existingUser != null)
                    {
                        if (existingUser.LockoutEnabled)
                        {
                            await _userManager.SetLockoutEnabledAsync(existingUser, false);
                            await _userManager.SetLockoutEndDateAsync(existingUser, null);
                        }
                        var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);

                        var result = await _userManager.ResetPasswordAsync(existingUser, token, resetRequest.NewPassword);

                        return Ok(result.Succeeded ? $"Reset password succeeded for {existingUser.UserName}" : $"Reset password failed");
                    }
                }
                return Unauthorized("User recovery token is not valid");
            }
            return BadRequest();
        }
    }

    public class ResetRequest
    {
        public string UserName { get; set; }
        public string NewPassword { get; set; }
        public string UserRecoveryToken { get; set; }
    }
}

