using Prism.Mvvm;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DAP.Mobile.ViewModels
{
    public class LoginPageViewModel : BindableBase
	{
        public bool IsBusy { get; set; }

        public ICommand IngresarCommand { get; set; }
        public ICommand RegistrarCommand { get; set; }
        public ICommand ResetPasswordCommand { get; set; }

        public LoginPageViewModel()
        {
            IngresarCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;

            IsBusy = true;

            try
            {
            }
            catch 
            {
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
