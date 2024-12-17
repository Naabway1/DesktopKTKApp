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
using System.Data;
using IsolationLevel = System.Transactions.IsolationLevel;

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
        public List<Week> Weeks { get; private set; }
        public List<string> DayTitle {  get; private set; }

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

        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (value != _username)
                {
                    _username = value;
                    OnPropertyChanged(nameof(Username));
                }
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
            _navVM = vm; //объявление переменных

            Navigate = new RelayCommand(NavigateCommandExecuted); //команды для кнопок
            AuthCommand = new RelayCommand(_ => CheckAuthData());
            RegistrationCommand = new RelayCommand(_ => Registration());

            Groups = KTKdb.SQLGetListOfData("SELECT GroupName FROM Groups"); //Вытягивание данных о группах
            Roles = KTKdb.SQLGetListOfData("SELECT RoleName FROM Roles WHERE RoleName != 'Администратор' "); //Вытягивание данных о ролях (препод/студентик)

            DayTitle = new List<string>()
            {
                "Понедельник",
                "Вторник",
                "Среда",
                "Четверг",
                "Пятница",
                "Суббота"
            };

            Weeks = GenerateWeeks(GetFirstMondayInSeptember(DateTime.Now.Year));
        }

        private static string HashPassword(string password)
        {
            if (password != null)
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
            else
            {
                return null;
            }
        }
        private async void Registration()
        {
            var transactionOptions = new TransactionOptions
            {
                IsolationLevel = IsolationLevel.ReadCommitted
            };

            using (var scope = new TransactionScope((TransactionScopeOption)TransactionScopeAsyncFlowOption.Enabled, transactionOptions))
            {
                try
                {
                    var userExists = await KTKdb.SQLReadAsync($"SELECT * FROM Users WHERE Login = '{_login}'");
                    if (userExists)
                    {
                        MessageBox.Show("Пользователь с таким логином существует! Придумайте другой!");
                        return;
                    }

                    if (_password != _passwordToCheck)
                    {
                        MessageBox.Show("Пароли не совпадают!");
                        return;
                    }

                    var roleIdObj = await KTKdb.SQLReadValueAsync($"SELECT TOP 1 RoleID FROM Roles WHERE RoleName = '{_selectedRole}'");
                    if (roleIdObj == null)
                    {
                        MessageBox.Show("Указанная роль не найдена!");
                        return;
                    }
                    var roleId = int.Parse(roleIdObj.ToString());

                    await KTKdb.SQLInsertDataAsync("Users", new Dictionary<string, object>
                    {
                        {"FullName", _fullName},
                        {"Login", _login},
                        {"Password", HashPassword(_password)},
                        {"RoleID", roleId}
                    });

                    var userIdObj = await KTKdb.SQLReadValueAsync($"SELECT UserID FROM Users WHERE Login = '{_login}'");
                    if (userIdObj == null)
                    {
                        MessageBox.Show("Ошибка получения ID пользователя.");
                        return;
                    }
                    var userId = int.Parse(userIdObj.ToString());

                    if (roleId == int.Parse((await KTKdb.SQLReadValueAsync($"SELECT RoleID FROM Roles WHERE RoleName = 'Студент'"))?.ToString() ?? "0"))
                    {
                        var groupIdObj = await KTKdb.SQLReadValueAsync($"SELECT GroupID FROM Groups WHERE GroupName = '{_selectedGroup}'");
                        if (groupIdObj == null)
                        {
                            MessageBox.Show("Группа не найдена!");
                            return;
                        }
                        var groupId = int.Parse(groupIdObj.ToString());

                        await KTKdb.SQLInsertDataAsync("Students", new Dictionary<string, object>
                        {
                            {"UserID", userId},
                            {"GroupID", groupId}
                        });
                    }
                    else if (roleId == int.Parse((await KTKdb.SQLReadValueAsync($"SELECT RoleID FROM Roles WHERE RoleName = 'Преподаватель'"))?.ToString() ?? "0"))
                    {
                        await KTKdb.SQLInsertDataAsync("Teachers", new Dictionary<string, object>
                        {
                            {"UserID", userId}
                        });
                    }
                    else
                    {
                        MessageBox.Show("Указана некорректная роль.");
                        return;
                    }
                    scope.Complete();
                    Console.WriteLine("Транзакция успешно выполнена");
                    _navVM.NavigateTo("AuthPage", this);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message);
                }
            }
        }
        private void CheckAuthData()
        {
            var roleID = 0;
            try
            {
                roleID = int.Parse(KTKdb.SQLReadValue($"SELECT RoleID FROM Users WHERE Login = '{_login}' and Password = '{HashPassword(_password)}'").ToString());
                Username = $"{KTKdb.SQLReadValue($"SELECT FullName FROM Users WHERE Login = '{_login}'")} ({_login})";
            }
            catch { }

            if (roleID == int.Parse(KTKdb.SQLReadValue($"SELECT RoleID FROM Roles WHERE RoleName = 'Студент'").ToString()))
            {
                _navVM.NavigateTo("MainPageForStudents", this);
            }
            else if (roleID == int.Parse(KTKdb.SQLReadValue($"SELECT RoleID FROM Roles WHERE RoleName = 'Преподаватель'").ToString()))
            {
                _navVM.NavigateTo("MainPageForTeachers", this);
            }
            else if (roleID == int.Parse(KTKdb.SQLReadValue($"SELECT RoleID FROM Roles WHERE RoleName = 'Администратор'").ToString()))
            {
                _navVM.NavigateTo("MainPageForAdmins", this);
            }
            else { MessageBox.Show("Пароль или логин неверный!"); }
        }
        private void NavigateCommandExecuted(object param)
        {
            _navVM.NavigateTo(param?.ToString(), this);
        }
        public List<Week> GenerateWeeks(DateTime startDate)
        {
            List<Week> weeks = new List<Week>();

            DateTime currentStartDate = startDate;

            for (int i = 1; i <= 52; i++)
            {
                DateTime currentEndDate = currentStartDate.AddDays(5);
                weeks.Add(new Week
                {
                    NumberOfWeeks = $"Неделя {i}",
                    StartDate = currentStartDate.ToString("d"),
                    EndDate = currentEndDate.ToString("d")
                });
                currentStartDate = currentStartDate.AddDays(7);
            }
            return weeks;
        }
        public DateTime GetFirstMondayInSeptember(int year)
        {
            DateTime firstSeptember = new DateTime(year, 9, 1);

            DayOfWeek firstDayOfWeek = firstSeptember.DayOfWeek;

            if (firstDayOfWeek == DayOfWeek.Monday)
            {
                return firstSeptember;
            }

            int daysToAdd = (DayOfWeek.Monday - firstDayOfWeek + 7) % 7;

            return firstSeptember.AddDays(daysToAdd);
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
