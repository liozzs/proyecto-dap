using Prism.Navigation;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DAP.Mobile.ViewModels
{
    public class LoginPageViewModel : ViewModelBase
    {
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

        public LoginPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            LoginCommand = new DelegateCommand(async () => await Login(), () => !IsLoading);
            SignUpCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync("SignUpPage"), () => !IsLoading);
            ResetPasswordCommand = new DelegateCommand(async () => await NavigationService.NavigateAsync("ResetPasswordPage"), () => !IsLoading);
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

                await NavigationService.NavigateAsync("/MenuPage/NavigationPage/MenuDetailPage");
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
