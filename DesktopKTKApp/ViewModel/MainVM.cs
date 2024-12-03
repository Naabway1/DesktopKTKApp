using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DesktopKTKApp.ViewModel
{
    internal class MainVM : ObservableObject
    {
        private readonly NavigationVM _navigationVM;

        public ICommand Navigate { get; }

        public MainVM (NavigationVM navVM)
        {
            _navigationVM = navVM;
            Navigate = new RelayCommand(_ => _navigationVM.NavigateTo(nameof(_), this));
        }
    }
}
