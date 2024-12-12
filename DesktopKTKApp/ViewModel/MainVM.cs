using DesktopKTKApp.Model;
using DesktopKTKApp.View;
using System;
using System.Security.Cryptography;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Transactions;
using System.Windows;
using System.Collections.Generic;
using System.Text;

namespace DesktopKTKApp.ViewModel
{
    public class MainVM : ObservableObject
    {
        private readonly NavigationVM _navVM;
        private string _selectedRole;
        public string SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (_selectedRole != value)
                {
                    _selectedRole = value;
                    OnPropertyChanged(nameof(SelectedRole));

                    IsGroupsComboBoxEnabled = (_selectedRole.ToString() == "Студент");
                }
            }
        }
        private string _selectedGroup;
        public string SelectedGroup
        {
            get => _selectedGroup;
            set
            {
                if (_selectedGroup != value)
                {
                    _selectedGroup = value;
                    OnPropertyChanged(nameof(SelectedGroup));
                }
            }
        }
        private bool _isGroupsComboBoxEnabled;
        public bool IsGroupsComboBoxEnabled
        {
            get => _isGroupsComboBoxEnabled;
            set
            {
                if (_isGroupsComboBoxEnabled != value)
                {
                    _isGroupsComboBoxEnabled = value;
                    OnPropertyChanged(nameof(IsGroupsComboBoxEnabled));
                }
            }
        }
        public List<string> Roles { get; private set; }
        public List<string> Groups { get; private set; }

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

        private string _passwordToCheck;
        public string PasswordToCheck
        {
            get => _passwordToCheck;
            set
            {
                if (value != _passwordToCheck)
                {
                    _passwordToCheck = value;
                }
                OnPropertyChanged(nameof(PasswordToCheck));
            }
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                if (value != _fullName)
                {
                    _fullName = value;
                }
                OnPropertyChanged(nameof(FullName));
            }
        }

        public ICommand Navigate { get; private set; }
        public ICommand AuthCommand { get; private set; }
        public ICommand RegistrationCommand { get; private set; }

        public MainVM(NavigationVM vm)
        {
            _navVM = vm; // объявление переменных

            Navigate = new RelayCommand(NavigateCommandExecuted); // команды для кнопок
            AuthCommand = new RelayCommand(_ => CheckAuthData());
            RegistrationCommand = new RelayCommand(_ => Registration());

            Groups = KTKdb.SQLGetListOfData("SELECT GroupName FROM Groups");//Вытягивание данных о группах
            Roles = KTKdb.SQLGetListOfData("SELECT RoleName FROM Roles WHERE RoleName != 'Администратор' ");//Вытягивание данных о ролях (препод/студентик)
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
        private void Registration()
        {
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    if (KTKdb.SQLRead($"SELECT * FROM Users where Login = '{_login}' ") != true)
                    {
                        if (_password == _passwordToCheck)
                        {
                            KTKdb.SQLInsertData("Users", new Dictionary<string, object>
                            {
                                {"FullName", _fullName},
                                {"Login", _login},
                                {"Password", HashPassword(_password)},
                                {"RoleID", int.Parse(KTKdb.SQLReadValue($"SELECT TOP 1 RoleID FROM Roles WHERE RoleName = '{_selectedRole}'").ToString())}
                            });
                            if (KTKdb.SQLReadValue($"SELECT RoleID from Users WHERE Login = '{_login}' ") == KTKdb.SQLReadValue($"SELECT RoleID from Roles WHERE RoleName = 'Студент' "))
                            {
                                KTKdb.SQLInsertData("Students", new Dictionary<string, object>
                                {
                                    {"UserID", KTKdb.SQLReadValue($"SELECT UserID from Users where Login = '{_login}' ")},
                                    {"GroupID", KTKdb.SQLReadValue($"SELECT GroupID from Groups where GroupName = '{_selectedGroup}' ")}
                                });
                                scope.Complete();
                                Console.WriteLine("Транзакция успешно выполнена");
                                _navVM.NavigateTo("AuthPage", this);
                            }
                            else if (KTKdb.SQLReadValue($"SELECT RoleID from Users WHERE Login = '{_login}' ") == KTKdb.SQLReadValue($"SELECT RoleID from Roles WHERE RoleName = 'Преподаватель' ")) 
                            {
                                KTKdb.SQLInsertData("Teachers", new Dictionary<string, object>
                                {
                                    {"UserID", KTKdb.SQLReadValue($"SELECT UserID from Users where Login = '{_login}' ")},
                                });
                                scope.Complete();
                                Console.WriteLine("Транзакция успешно выполнена");
                                _navVM.NavigateTo("AuthPage", this);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Пароли не совпадают!");
                        }
                    }
                    else
                    {
                        MessageBox.Show("Пользователь с таким логином существует! Придумайте другой!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }
        private void CheckAuthData()
        {
            if (KTKdb.SQLRead("SELECT RoleID FROM Roles WHERE RoleName = 'Студент'") == KTKdb.SQLRead($"SELECT RoleID FROM Users WHERE Login = '{_login}' and Password = '{HashPassword(_password)}'"))
            {
                _navVM.NavigateTo("MainPageForStudents", this);
            }
            else if (KTKdb.SQLRead("SELECT RoleID FROM Roles WHERE RoleName = 'Преподаватель'") == KTKdb.SQLRead($"SELECT RoleID FROM Users WHERE Login = '{_login}' and Password = '{HashPassword(_password)}'"))
            {
                _navVM.NavigateTo("MainPageForTeachers", this);
            }
            else if (KTKdb.SQLRead("SELECT RoleID FROM Roles WHERE RoleName = 'Администратор'") == KTKdb.SQLRead($"SELECT RoleID FROM Users WHERE Login = '{_login}' and Password = '{HashPassword(_password)}'"))
            {
                _navVM.NavigateTo("MainPageForAdmins", this);
            }
            else { MessageBox.Show("Пароль или логин неверный!"); }
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
                case "MainPageForStudents":
                    page = new MainPageForStudents();
                    break;
                case "MainPageForTeachers":
                    page = new MainPageForTeachers();
                    break;
                case "MainPageForAdmins":
                    page = new MainPageForAdmins();
                    break;
                case "RegistrationPage":
                    page = new RegistrationPage();
                    break;
                case "ForgottenPasswordRecovery":
                    page = new ForgottenPasswordRecovery();
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
