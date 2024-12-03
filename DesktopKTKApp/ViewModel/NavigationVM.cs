using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DesktopKTKApp.View;

namespace DesktopKTKApp.ViewModel
{
    internal class NavigationVM : ObservableObject
    {
        private readonly Frame _frame;

        public NavigationVM(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(string pageName, object dataContext)
        {
            Page page = null;
            page.DataContext = dataContext;
            switch (pageName)
            {
                case "AuthPage":
                    page = new AuthPage();
                    break;
                case "MainPage":
                    page = new MainPage();
                    break;
                case "RegistrationPage":
                    page = new RegistrationPage();
                    break;
                default:
                    throw new ArgumentException("Неизвестная страница ", nameof(pageName));
            }
            _frame.Navigate(page);
        }
    }
}
