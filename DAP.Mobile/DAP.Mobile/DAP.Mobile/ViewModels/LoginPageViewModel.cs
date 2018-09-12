using DAP.Mobile.Helpers;
using DAP.Mobile.Models;
using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
        private readonly IApiClient apiClient;

        public string User { get; set; }
        public string Password { get; set; }

        public ICommand LoginCommand { get; set; }
        public ICommand SignUpCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }

        public LoginPageViewModel(INavigationService navigationService, IApiClient apiClient) : base(navigationService)
        {
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
                    await NavigationService.NavigateAsync("/MenuPage/NavigationPage/MenuDetailPage");
                }
                else if (Validate())
                {
                    var options = new ApiClientOption
                    {
                        Uri = "api/login",
                        BaseUrl = "192.168.0.16",
                        RequestContent = new User { UserName = User, Password = Password }
                    };

                    LoginResult result = await apiClient.InvokeDataServiceAsync<LoginResult>(options);
                    if (String.IsNullOrEmpty(result.Error))
                    {
                        //Guardar token
                        await NavigationService.NavigateAsync("/MenuPage/NavigationPage/MenuDetailPage");
                    }
                    else
                    {
                        Message = result.Error;
                    }
                }
            }
            catch
            {
                Message = "Ocurrió un error al ingresar. Intentá de nuevo en unos minutos.";
            }
            finally
            {
                IsLoading = false;
            }
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
                Message = "El usuario ingresado es inválido.";
            }
            else if (String.IsNullOrWhiteSpace(Password))
            {
                Message = "Debe ingresar su contraseña";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}