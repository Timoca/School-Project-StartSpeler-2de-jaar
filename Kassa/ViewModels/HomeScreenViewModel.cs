using Kassa.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kassa.ViewModels
{
	public partial class HomeScreenViewModel : BaseViewModel
	{
		public HomeScreenViewModel(ApiService apiService) : base(apiService)
		{
		}
	}
}