using DAP.Mobile.Models;
using Prism.Commands;
using Prism.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DAP.Mobile.ViewModels
{
    public class NotificationsPageViewModel : ViewModelBase
	{
        public DelegateCommand LoadNotificationsCommand { get; set; }
        public DelegateCommand OpenNotificationCommand { get; set; }
        public ObservableCollection<Notification> Notifications { get; set; }

        public NotificationsPageViewModel(INavigationService navigationService) : base(navigationService)
        {
            Notifications = new ObservableCollection<Notification>();
            OpenNotificationCommand = new DelegateCommand(async () => await OpenNotification());
            LoadNotificationsCommand = new DelegateCommand(async () => await LoadNotifications());
            LoadNotificationsCommand.Execute();
        }

        private Task OpenNotification()
        {
            throw new NotImplementedException();
        }

        private Task LoadNotifications()
        {
            IsLoading = true;
            return Task.Run(() => {                
                for (int i = 0; i < 5; i++)
                {
                    Notifications.Add(new Notification() { Message = $"Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} Mensaje {i} ",
                        Title = $"Título {i}",
                        Date = DateTime.Now.AddMinutes(-i) });
                }

                IsLoading = false;
            });
        }
    }
}
