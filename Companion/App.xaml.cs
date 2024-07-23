using Companion.Services;
using Companion.ViewModels;

namespace Companion
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}