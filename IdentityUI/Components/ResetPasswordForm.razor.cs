using IdentityUI.Common;
using IdentityUI.Enums;
using Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace IdentityUI.Components
{
    public partial class ResetPasswordForm
    {
        [Inject]
        NavigationManager _navigationManager { get; set; }
        [Inject]
        UserManager<ApplicationUser> _userManager { get; set; }

        private bool _isLoading = false;
        private bool _isResetCompleted = false;
        private StringValues _email;
        private StringValues _code;
        private ResetPasswordModel _model = new ResetPasswordModel();
        private AlertMessage _alertMessage = AlertMessage.Hide();

        private async void OnValidSubmit()
        {
            _isLoading = true;

            try
            {
                var uri = _navigationManager.ToAbsoluteUri(_navigationManager.Uri);

                QueryHelpers.ParseQuery(uri.Query).TryGetValue("email", out _email);
                QueryHelpers.ParseQuery(uri.Query).TryGetValue("code", out _code);

                _model.Code = _code;
                _model.Email = _email;

                var result = await ResetPasswordAsync(_model);

                if (result.Succeeded)
                {
                    _alertMessage = AlertMessage.Show(AlertType.AlertSuccess, "Başarılı"
                    , new[] { "Şifreniz Başarı ile Değiştirildi." });
                    _isResetCompleted = true;
                }
                else
                {
                    _alertMessage = AlertMessage.Show(AlertType.AlertDanger, result.Errors);
                }
            }
            catch (Exception ex)
            {
                _alertMessage = AlertMessage.Show(AlertType.AlertDanger, new[] { ex.Message });
            }

            _isLoading = false;
            StateHasChanged();
        }

        private async Task<ActionResult> ResetPasswordAsync(ResetPasswordModel model)
        {

            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Code) || string.IsNullOrEmpty(model.Password))
            {
                return ActionResult.Failed("Hata Oluştu");
            }

            model.Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null)
            {
                return ActionResult.Failed("Hata Oluştu1");
            }

            var resetResult = await _userManager.ResetPasswordAsync(user, model.Code, model.Password);

            if (resetResult.Succeeded)
            {
                return ActionResult.Success;
            }

            return ActionResult.Failed(resetResult.Errors.Select(x => x.Description).ToArray());

        }
    }
}




