using DesktopKTKApp.Model;
using DesktopKTKApp.View;
using System;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;

namespace DesktopKTKApp.ViewModel
{
    public class MainVM : ObservableObject
    {
        private readonly NavigationVM _navVM;
        private readonly string connectionString = "Server=10.14.206.27;Database=user5;User Id=user5;Password=Lu%5%4e4"; //подключение бд
        private SqlConnection KTKdb;

        private string _login;
        public string Login
        {
            get => _login;
            set
            {
                if (value != _login)
                {
                    _login = value;
                }
                OnPropertyChanged(nameof(Login));
            }
        }
        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (value != _password)
                {
                    _password = value;
                }
                OnPropertyChanged(nameof(Password));
            }
        }
        public ICommand Navigate { get; private set; }
        public ICommand AuthCommand { get; private set; }

        public MainVM (NavigationVM vm)
        {
            _navVM = vm; // объявление переменных

            Navigate = new RelayCommand(NavigateCommandExecuted); // команды для кнопок
            AuthCommand = new RelayCommand(_ => CheckAuthData());

            KTKdb = new SqlConnection(connectionString);
            KTKdb.Open();
        }

        private void CheckAuthData()
        {
            SqlCommand sql = new SqlCommand()
            {
                CommandText = $"SELECT * FROM Users WHERE Login = \'{_login}\' and Password = \'{_password}\'",
                Connection = KTKdb
            };
            SqlDataReader sqlDataReader = sql.ExecuteReader();
            try 
            { 
                if (sqlDataReader.Read() == true)
                {
                    _navVM.NavigateTo("MainPage", this);
                    sqlDataReader.Close();
                }
                else
                {
                    Debug.WriteLine("А пароль то неправильный, XD");
                    sqlDataReader.Close();
                }
            } 
            catch { return; }        
        }
        private void NavigateCommandExecuted(object param)
        {
            _navVM.NavigateTo(param?.ToString(), this);
        }
    }

    public class NavigationVM
    {
        private readonly Frame _frame;

        public NavigationVM(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(object param, object dataContext)
        {
            string pageName = param.ToString();

            Page page;
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
            page.DataContext = dataContext;
            _frame.Navigate(page);

            Debug.WriteLine("Команда выполнена");
        }
    }
}
