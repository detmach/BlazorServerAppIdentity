﻿using IdentityUI.Common;
using IdentityUI.Enums;
using Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace IdentityUI.Components
{
    public partial class ChangePasswordForm
    {
        [Inject]
        private UserManager<IdentityUser> _userManager { get; set; }
        [Inject]
        private IHttpContextAccessor _context { get; set; }

        private AlertMessage _alertMessage = AlertMessage.Hide();
        private ChangePasswordModel _model = new ChangePasswordModel();
        private bool _isLoading = false;

        private async void OnValidSubmit()
        {
            _isLoading = true;

            try
            {
                var id = _context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(id);               
                var result = await _userManager.ChangePasswordAsync(user, _model.OldPassword, _model.NewPassword);
                if (result.Succeeded)
                {
                    _alertMessage = AlertMessage.Show(AlertType.AlertSuccess
                        , new[] { "Şifreniz Başarılı bir şekilde değiştirildi" });
                }
                else
                {
                    _alertMessage = AlertMessage.Show(AlertType.AlertDanger,
                        result.Errors.Select(x => x.Description).ToArray());
                }

            }
            catch (Exception)
            {
                _alertMessage = AlertMessage.ShowDefaultError();
            }

            _isLoading = false;
            StateHasChanged();

        }
    }
}
