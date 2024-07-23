using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.Services
{
    public static class GlobalSettings
    {
        private static List<string> userRoles = new List<string>();

        public static List<string> UserRoles
        {
            get => userRoles;
            set
            {
                if (userRoles != value)
                {
                    userRoles = value;
                    OnPropertyChanged(nameof(userRoles));
                    OnPropertyChanged(nameof(IsBeheerder));
                    OnPropertyChanged(nameof(IsEventManager));
                    OnPropertyChanged(nameof(IsKelner));
                }
            }
        }

        public static bool IsBeheerder => UserRoles.Contains("Beheerder");
        public static bool IsEventManager => UserRoles.Contains("Event manager");
        public static bool IsKelner => UserRoles.Contains("Kelner");

        public static event Action<string>? PropertyChanged;

        private static void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(propertyName);
        }
    }
}