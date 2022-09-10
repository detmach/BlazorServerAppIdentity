using IdentityUI.Common;
using IdentityUI.Enums;
using IdentityUI.Interfaces;
using Models;
using IdentityUI.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityUI.Components
{
    public partial class RegisterForm
    {
        [Inject]
        SignInManager<ApplicationUser> _signInManager { get; set; }
        [Inject]
        UserManager<ApplicationUser> _userManager { get; set; }
        [Inject]
        IUserStore<ApplicationUser> _userStore { get; set; }
        [Inject]
        IEmailService _emailService { get; set; }
        [Inject]
        NavigationManager _navigationManager { get; set; }

        private RegisterModel _model = new RegisterModel();
        private bool _isLoading = false;
        private bool _isRegistered = false;
        private AlertMessage _alertMessage = AlertMessage.Hide();
        private AlertMessage _registrationFinishedAlert = AlertMessage.Hide();


        private async void OnValidSubmit()
        {
            _isLoading = true;
            var result = await RegisterUserAsync(_model);

            if (result.Succeeded)
            {
                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    await SendActivationEmailAsync(_model.Email);
                    _isRegistered = true;
                    _registrationFinishedAlert = AlertMessage.Show(AlertType.AlertSuccess
                        , "Hesabınız Başarı ile Kaydedildi", 
                        new string[] { "Lütfen Email Adresiniz Onaylayınız" });
                }
                else
                {
                    Guid key = Guid.NewGuid();
                    AuthentificationService.Logins[key] = new LoginModel { Email = _model.Email, Password = _model.Password };
                    _navigationManager.NavigateTo($"/Account/Login?key={key}", true);
                }
            }
            else
            {
                if (result.Errors != null)
                {
                    _alertMessage = AlertMessage.Show(AlertType.AlertDanger
                        , result.Errors);
                }
            }

            _isLoading = false;
            StateHasChanged();
        }

        private async Task<ActionResult> RegisterUserAsync(RegisterModel model)
        {
            var user = CreateUser(model);

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                return ActionResult.Success;
            }

            return ActionResult.Failed(result.Errors.Select(x => x.Description).ToArray());
        }

        private async Task SendActivationEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return;

            var user = await _userManager.FindByNameAsync(email);
            if (user == null) return;

            var baseUri = _navigationManager.BaseUri;
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{baseUri}Account/ConfirmEmail?user={user.Id}&code={code}";
            await _emailService.SendEmailAsync(user.Email, "Lütfen Email Adresiniz Onaylayınız",
                $"Lütfen Email Adresiniz Onaylayınız <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Lütfen Email Adresiniz Onaylayınız");
        }

        private ApplicationUser CreateUser(RegisterModel model)
        {
            try
            {
                return new ApplicationUser { UserName = model.Email, Email = model.Email };
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor");
            }
        }
    }
}