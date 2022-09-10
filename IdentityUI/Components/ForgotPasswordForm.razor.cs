using IdentityUI.Common;
using IdentityUI.Enums;
using IdentityUI.Interfaces;
using Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using System.Text;
using System.Text.Encodings.Web;

namespace IdentityUI.Components
{
    public partial class ForgotPasswordForm
    {
        [Inject]
        NavigationManager _navigationManager { get; set; }
        [Inject]
        UserManager<ApplicationUser> _userManager { get; set; }
        [Inject]
        IEmailService _emailService { get; set; }

        private ForgotPasswordModel _model = new ForgotPasswordModel();
        private bool _isLoading = false;
        private bool _isResetCompleted = false;
        private AlertMessage _alertMessage = AlertMessage.Hide();
        private AlertMessage _resetCompletedMessage = AlertMessage.Hide();

        private async void OnValidSubmit()
        {
            _isLoading = true;

            var result = await ResetPasswordAsync(_model.Email);

            if (result.Succeeded)
            {
                var test = "Eposta Kontrol";
                _isResetCompleted = true;
                _resetCompletedMessage = AlertMessage.Show(AlertType.AlertSuccess
                    ,"Success", new[] { "Şifremi unuttum" });
            }
            else
            {
                _alertMessage = AlertMessage.Show(AlertType.AlertDanger, result.Errors);
            }

            _isLoading = false;

            StateHasChanged();
        }

        private async Task<ActionResult> ResetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
            {
                return ActionResult.Failed(new[] { "Kullanıcı Bulunamadı" });
            }

            if ((await _userManager.IsEmailConfirmedAsync(user)) == false)
            {
                return ActionResult.Failed(new[] { "Hesabınız Aktif değil" });
            }

            await SendForgotPasswordEmailAsync(user);

            return ActionResult.Success;
        }


        private async Task SendForgotPasswordEmailAsync(ApplicationUser user)
        {
            var baseUri = _navigationManager.BaseUri;
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = $"{baseUri}Account/ResetPassword?code={code}&email={user.Email}";
            await _emailService.SendEmailAsync(user.Email, "Reset Password" ,
                $"Reset Password <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>Tıkla</a>.");
        }
    }
}
