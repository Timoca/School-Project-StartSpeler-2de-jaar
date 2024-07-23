using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Companion.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private string title = default!;

        public BaseViewModel(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<bool> CheckAuthentication()
        {
            return await _apiService.IsUserLoggedInAsync();
        }
    }
}