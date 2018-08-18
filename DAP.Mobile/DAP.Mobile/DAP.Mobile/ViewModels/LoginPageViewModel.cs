﻿using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DAP.Mobile.ViewModels
{
    public class LoginPageViewModel : BindableBase
    {
        private readonly INavigationService navigationService;

        private string message;

        public string Message
        {
            get { return message; }
            set { SetProperty(ref message, value); }
        }

        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { SetProperty(ref isLoading, value); }
        }

        public string User { get; set; }
        public string Password { get; set; }

        public ICommand LoginCommand { get; set; }
        public ICommand SignUpCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }

        public LoginPageViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
            LoginCommand = new Command(async () => await Login(), () => !IsLoading);
            SignUpCommand = new Command(async () => await navigationService.NavigateAsync("SignUpPage"), () => !IsLoading);
            ResetPasswordCommand = new Command(async () => await navigationService.NavigateAsync("ResetPasswordPage"), () => !IsLoading);
        }

        private async Task Login()
        {
            Message = null;
            IsLoading = true;

            try
            {
                //Validar datos
                if (String.IsNullOrWhiteSpace(User))
                {
                    Message = "Debe ingresar su usuario";
                }
                else if (String.IsNullOrWhiteSpace(Password))
                {
                    Message = "Debe ingresar su contraseña";
                }
            }
            //catch 
            //{
            //}
            finally
            {
                IsLoading = false;
            }
        }
    }
}
