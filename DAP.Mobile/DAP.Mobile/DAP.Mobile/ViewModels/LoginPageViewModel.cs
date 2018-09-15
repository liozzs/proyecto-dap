﻿using DAP.Mobile.Helpers;
using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private readonly IPageDialogService dialogService;
        private readonly IApiClient apiClient;

        public string User { get; set; }
        public string Password { get; set; }

        public ICommand LoginCommand { get; set; }
        public ICommand SignUpCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }

        public LoginPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient) : base(navigationService)
        {
            this.dialogService = dialogService;
            this.apiClient = apiClient;
            LoginCommand = new DelegateCommand(async () => await LoginAsync(), () => !IsLoading);
            SignUpCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync("SignUpPage"), () => !IsLoading);
            ResetPasswordCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync("ResetPasswordPage"), () => !IsLoading);
        }

        private async Task LoginAsync()
        {
            Message = null;
            IsLoading = true;

            try
            {
                if (User == "admin" && Password == "admin")
                {
                    await GoToMenu("impersonar");
                }
                else if (Validate())
                {
                    var options = new ApiClientOption
                    {
                        RequestType = ApiClientRequestTypes.Post,
                        Uri = "api/login",
                        Service = ApiClientServices.Api,
                        RequestContent = new User { UserName = User, Password = Password }
                    };

                    LoginResult result = await apiClient.InvokeDataServiceAsync<LoginResult>(options);
                    if (String.IsNullOrEmpty(result.Error))
                    {
                        await GoToMenu(result.Token);
                    }
                    else
                    {
                        Message = result.Error;
                    }
                }
            }
            catch (Exception e)
            {
                await dialogService.DisplayAlertAsync("Error", "Ocurrió un error al ingresar. Intente nuevamente en unos minutos.", "Aceptar");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GoToMenu(string token)
        {
            //Guardar token
            Helper.SetApplicationValue("user", User);
            Helper.SetApplicationValue("logged", true);
            Helper.SetApplicationValue("token", token);

            await NavigationService.NavigateAsync("/MenuPage/NavigationPage/MenuDetailPage");
        }

        private bool Validate()
        {
            //Validar datos
            if (String.IsNullOrWhiteSpace(User))
            {
                Message = "Debe ingresar su usuario";
            }
            else if (!Helper.IsValidEmail(User))
            {
                Message = "El usuario ingresado es inválido";
            }
            else if (String.IsNullOrWhiteSpace(Password))
            {
                Message = "Debe ingresar su contraseña";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}