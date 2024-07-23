using Companion.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Companion.ViewModels
{
    public class LogoutViewModel : BaseViewModel
    {
        private readonly ApiService _apiService;

        public LogoutViewModel(ApiService apiService) : base(apiService)
        {
            _apiService = apiService;
        }

        public async Task LogoutAsync()
        {
            await _apiService.LogoutAsync();
        }
    }
}