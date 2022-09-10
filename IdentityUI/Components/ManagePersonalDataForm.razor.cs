using IdentityUI.Common;
using IdentityUI.Enums;
using Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System.Security.Claims;
using System.Text.Json;

namespace IdentityUI.Components
{
    public partial class ManagePersonalDataForm
    {
        [Inject]
        private UserManager<ApplicationUser> _userManager { get; set; }
        [Inject]
        private IHttpContextAccessor _context { get; set; }
        [Inject]    
        private IJSRuntime _jsRuntime { get; set; }
        [Inject]
        private NavigationManager _navigationManager { get; set; }

        private AlertMessage _alertMessage = AlertMessage.Hide();
        private PasswordModel _model = new PasswordModel();
        private bool _isLoading = false;
        private bool _isConfirmDelete = false;
        private ApplicationUser _user;
        private IJSObjectReference module { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/IdentityUI/scripts/ui_utils.js");
            }
        }

        private async void OnValidSubmit()
        {
            _isLoading = true;

            try
            {
                var result = await DeleteUserAccountAsync(_model.Password);
                if (result.Succeeded)
                {
                    _navigationManager.NavigateTo($"/Account/Logout?returnUrl=/Account/Deleted", forceLoad: true);
                }
                else
                {
                    _alertMessage = AlertMessage.Show(AlertType.AlertDanger, result.Errors);
                }

            }
            catch (Exception)
            {
                _alertMessage = AlertMessage.ShowDefaultError();
            }

            _isLoading = false;
            StateHasChanged();
        }

        private async Task DownloadPersonalDataAsync()
        {
            var id = _context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return;

            var personalData = new Dictionary<string, string>();
            var personalDataProps = typeof(IdentityUser).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            personalData.Add($"Authenticator Key", await _userManager.GetAuthenticatorKeyAsync(user));

            var file = JsonSerializer.SerializeToUtf8Bytes(personalData);

            await module.InvokeAsync<string>("DownloadFile", "Personal_Data.json", "application/octet-stream", file);
        }

        private void ConfirmAccountDelete()
        {
            _isConfirmDelete = true;
            _alertMessage = AlertMessage.Show(AlertType.AlertDanger
                ,new [] { "Hesabınızı Silmek İstediğinize Eminmisiniz" });
        }

        private async Task<ActionResult> DeleteUserAccountAsync(string password)
        {
            var id = _context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return ActionResult.Failed(new[] { "Bilinmeyen Kullanıcı" });
            }

            var isPasswordSet = await _userManager.HasPasswordAsync(user);
            if (isPasswordSet)
            {
                if (!await _userManager.CheckPasswordAsync(user, password))
                {
                    return ActionResult.Failed(new[] { "Şifre Hatalı" });
                }
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                return ActionResult.Failed(new[] { "Ne yazıyor"});
            }

            return ActionResult.Success;
        }
    }
}
