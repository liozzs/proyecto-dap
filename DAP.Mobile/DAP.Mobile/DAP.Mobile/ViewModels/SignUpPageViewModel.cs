using DAP.Mobile.Services;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DAP.Mobile.ViewModels
{
    public class SignUpPageViewModel : ViewModelBase
    {
        private readonly IApiClient apiClient;
        private readonly IPageDialogService dialogService;

        public string Name { get; set; }
        public string Surname { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string ConfirmEmail { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }

        public ICommand CancelCommand { get; set; }
        public ICommand AcceptCommand { get; set; }

        public SignUpPageViewModel(INavigationService navigationService, IPageDialogService dialogService, IApiClient apiClient) : base(navigationService)
        {
            this.apiClient = apiClient;
            this.dialogService = dialogService;

            CancelCommand = new DelegateCommand(async () => await navigationService.GoBackAsync());
            AcceptCommand = new DelegateCommand(async () => await Accept());
        }

        private async Task Accept()
        {
            if (Validate())
            {
                try
                {
                    ApiClientOption option = new ApiClientOption
                    {
                        RequestType = ApiClientRequestTypes.Post,
                        Uri = "api/login/create",
                        Service = ApiClientServices.Api,
                        RequestContent = new { Nombre = $"{Name} {Surname}", Password, Telefono = Telephone, Email}
                    };

                    await apiClient.InvokeDataServiceAsync(option);

                    await dialogService.DisplayAlertAsync("Registro", "Se registró correctamente", "Aceptar");

                    await NavigationService.GoBackAsync();
                }
                catch
                {
                    await dialogService.DisplayAlertAsync("Registro", "Ocurrió un error al realizar la operación. Intente nuevamente en unos minutos.", "Aceptar");
                }
            }
        }

        private bool Validate()
        {
            Message = null;

            if (string.IsNullOrWhiteSpace(Name))
            {
                Message = "Debe ingresar su nombre";
            }
            else if (string.IsNullOrWhiteSpace(Surname))
            {
                Message = "Debe ingresar su apellido";
            }
            else if (string.IsNullOrWhiteSpace(Email))
            {
                Message = "Debe ingresar su email";
            }
            else if (!Helpers.Helper.IsValidEmail(Email))
            {
                Message = "El email ingresado es inválido";
            }
            else if (string.IsNullOrWhiteSpace(ConfirmEmail))
            {
                Message = "Debe confirmar su email";
            }
            else if (Email != ConfirmEmail)
            {
                Message = "Los emails ingresados no coinciden";
            }
            else if (string.IsNullOrWhiteSpace(Password))
            {
                Message = "Debe ingresar su contraseña";
            }
            else if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                Message = "Debe confirmar su contraseña";
            }
            else if (Password != ConfirmPassword)
            {
                Message = "Las contraseñas ingresadas no coinciden";
            }

            return string.IsNullOrEmpty(Message);
        }
    }
}