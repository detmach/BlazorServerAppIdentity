﻿using IdentityUI.Common;
using IdentityUI.Enums;
using Models;
using IdentityUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;

namespace IdentityUI.Components
{
    public partial class LoginForm
    {

        private LoginModel _model = new LoginModel();
        private bool _isLoading = false;
        private bool _isResendVisible = false;
        private AlertMessage _alertMessage = AlertMessage.Hide();

        private async Task<ChallengeResult> OnLoginGoogle()
        {
            string provider = "Google";
            // Request a redirect to the external login provider.
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = "/account/login",
            };
            return new ChallengeResult(provider, authenticationProperties);
        }
        private async void OnValidSubmit()
        {
            _alertMessage = AlertMessage.Hide();
            _isLoading = true;

            var result = await GetUserIdentityResultAsync(_model);

            if (result.Succeeded)
            {
                Guid key = Guid.NewGuid();
                AuthentificationService.Logins[key] = _model;
                _navigationManager.NavigateTo($"/Account/Login?key={key}", true);
            }
            else
            {
                if (result.Errors != null)
                {
                    _alertMessage = AlertMessage.Show(AlertType.AlertDanger,
                                    result.Errors.Select(x => x.Description));

                    if (result.Errors.Any(x => x.Code == "Email Onaylanmadı"))
                    {
                        _isResendVisible = true;
                    }
                }

            }
            _isLoading = false;

            StateHasChanged();
        }

        private async void OnResendEmailClick()
        {
            _isLoading = true;

            if (!string.IsNullOrEmpty(_model.Email))
            {
                await ReSendActivationEmailAsync(_model.Email);
                _alertMessage = AlertMessage.Show(AlertType.AlertSuccess
                    , new List<string> { "Email Kontrol Check" });
            }
            _isLoading = _isResendVisible = false;

            StateHasChanged();
        }

        private async Task<IdentityResult> GetUserIdentityResultAsync(LoginModel model)
        {
            // we cannot use singin manager so we have to go around.
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = nameof(model.Email),
                    Description = $"Unknown user {model.Email}."
                });
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, model.Password);

            if (!isValidPassword)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = nameof(model.Email),
                    Description = $"Could not authentificate user {model.Email}."
                });
            }

            // Check if email is confired;
            if (_userManager.Options.SignIn.RequireConfirmedAccount)
            {
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "Email Not Confirmed",
                        Description = $"Email {user.Email} is not confirmed."
                    });
                }
            }

            return IdentityResult.Success;
        }

        private async Task ReSendActivationEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return;
            //in case email is altered before pressing button.
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return;

            var baseUri = _navigationManager.BaseUri;
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{baseUri}account/confirmemail?user={user.Id}&code={code}";
            await _emailService.SendEmailAsync(user.Email,"Email Onayla",
                $"{"asdasd"} " +
                $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>{"Tıkla"}</a>.");
         
        }
    }
}
