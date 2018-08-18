using Prism.Mvvm;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DAP.Mobile.ViewModels
{
    public class LoginPageViewModel : BindableBase
	{
        private string mensaje;

        public string Mensaje
        {
            get { return mensaje; }
            set { SetProperty(ref mensaje, value); }
        }

        private bool isLoading;

        public bool IsLoading
        {
            get { return isLoading; }
            set { SetProperty(ref isLoading, value); }
        }

        public string Usuario { get; set; }
        public string Password { get; set; }

        public ICommand IngresarCommand { get; set; }
        public ICommand RegistrarCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }

        public LoginPageViewModel()
        {
            IngresarCommand = new Command(async () => await Ingresar(), () => !IsLoading);
            RegistrarCommand = new Command(async () => await Ingresar(), () => !IsLoading);
            ResetPasswordCommand = new Command(async () => await Ingresar(), () => !IsLoading);
        }

        async Task Ingresar()
        {
            Mensaje = null;
            IsLoading = true;

            try
            {
                //Validar datos
                if (String.IsNullOrWhiteSpace(Usuario))
                {
                    Mensaje = "Debe ingresar su usuario";
                }
                else if (String.IsNullOrWhiteSpace(Password))
                {
                    Mensaje = "Debe ingresar su contraseña";
                }
            }
            //catch 
            //{
            //}
            finally
            {
                IsLoading = true;
            }
        }
    }
}
